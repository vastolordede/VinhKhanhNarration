using System.Security.Cryptography;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.BUS;

public class GuestAccessBUS
{
    private readonly GuestAccessDAO _dao;
    private readonly GuestSessionDAO _sessionDAO;
    private readonly SessionGenerator _sessionGenerator;
    private readonly IConfiguration _configuration;

    public GuestAccessBUS(
        GuestAccessDAO dao,
        GuestSessionDAO sessionDAO,
        SessionGenerator sessionGenerator,
        IConfiguration configuration)
    {
        _dao = dao;
        _sessionDAO = sessionDAO;
        _sessionGenerator = sessionGenerator;
        _configuration = configuration;
    }

    public GuestAccessStatusDTO GetStatus(string guestSessionId)
    {
        guestSessionId = RequireExistingSession(guestSessionId);
        var pass = _dao.GetActivePass(guestSessionId);
        var session = _sessionDAO.GetById(guestSessionId);

        return new GuestAccessStatusDTO
        {
            HasActivePass = pass != null,
            GuestSession = session,
            AccessPass = pass,
            PendingPayment = null,
            HoursRemaining = pass == null
                ? 0
                : Math.Max(1, (int)Math.Ceiling((pass.ExpiresAt - DateTime.UtcNow).TotalHours))
        };
    }

    public GuestPaymentOrderDTO GetPaymentOrder(string orderCode)
    {
        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Mã thanh toán là bắt buộc.");

        return _dao.GetPaymentByOrderCode(orderCode.Trim())
            ?? throw new InvalidOperationException("Mã thanh toán không tồn tại.");
    }

    public GuestPaymentOrderDTO CreatePaymentOrder(
        CreateGuestPaymentRequestDTO request,
        string? ipAddress)
    {
        var orderCode = $"VK-GUEST-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}";
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL")
            ?? _configuration["FrontendUrl"]
            ?? "http://localhost:5173";

        var order = new GuestPaymentOrderDTO
        {
            GuestSessionId = null,
            OrderCode = orderCode,
            Amount = GetDecimal("GuestAccess:Price", 50000m),
            Status = GuestPaymentStatuses.Pending,
            Provider = "Mock",
            PaymentUrl = $"{frontendUrl.TrimEnd('/')}/app/access?orderCode={Uri.EscapeDataString(orderCode)}",
            PreferredLanguageId = request.PreferredLanguageId,
            DeviceInfo = NormalizeOptional(request.DeviceInfo, 255),
            IPAddress = NormalizeOptional(ipAddress, 50),
            ExpiresAt = DateTime.UtcNow.AddMinutes(GetInt("GuestAccess:OrderExpiryMinutes", 30))
        };

        order.GuestPaymentOrderId = _dao.InsertPayment(order);
        return order;
    }

    public GuestAccessStatusDTO ConfirmMockPayment(string orderCode)
    {
        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Mã thanh toán là bắt buộc.");

        var order = _dao.GetPaymentByOrderCode(orderCode.Trim())
            ?? throw new InvalidOperationException("Mã thanh toán không tồn tại.");

        if (order.ExpiresAt <= DateTime.UtcNow && order.Status != GuestPaymentStatuses.Paid)
            throw new InvalidOperationException("Mã thanh toán đã hết hạn.");

        var generatedSessionId = _sessionGenerator.GenerateGuestSessionId();
        var sessionId = _dao.ConfirmPaymentAndCreateSession(
            order.GuestPaymentOrderId,
            generatedSessionId,
            GetInt("GuestAccess:DurationHours", 24));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new InvalidOperationException("Không thể xác nhận thanh toán.");

        return GetStatus(sessionId);
    }

    public GuestAccessPassDTO EnsureActive(string guestSessionId)
    {
        guestSessionId = RequireActiveSession(guestSessionId);
        return _dao.GetActivePass(guestSessionId)
            ?? throw new UnauthorizedAccessException(
                "Access Pass đã hết hạn. Vui lòng tạo phiên Guest mới để tiếp tục nghe narration.");
    }

    public int ExpireStaleRows() =>
        _dao.ExpirePendingPayments() + _dao.ExpireAccessPassesAndSessions();

    private string RequireExistingSession(string guestSessionId)
    {
        if (string.IsNullOrWhiteSpace(guestSessionId))
            throw new ArgumentException("Guest session là bắt buộc.");

        var normalized = guestSessionId.Trim();
        if (!_dao.GuestSessionExists(normalized, activeOnly: false))
            throw new InvalidOperationException("Guest session không tồn tại.");

        return normalized;
    }

    private string RequireActiveSession(string guestSessionId)
    {
        var normalized = RequireExistingSession(guestSessionId);
        if (!_dao.GuestSessionExists(normalized, activeOnly: true))
            throw new UnauthorizedAccessException("Guest session đã hết hạn.");
        return normalized;
    }

    private int GetInt(string key, int fallback) =>
        int.TryParse(_configuration[key], out var value) && value > 0 ? value : fallback;

    private decimal GetDecimal(string key, decimal fallback) =>
        decimal.TryParse(_configuration[key], out var value) && value > 0 ? value : fallback;

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
