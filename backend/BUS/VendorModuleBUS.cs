using System.Security.Cryptography;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.BUS;

public class VendorModuleBUS
{
    private readonly VendorModuleDAO _dao;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenGenerator _jwt;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public VendorModuleBUS(
        VendorModuleDAO dao,
        PasswordHasher hasher,
        JwtTokenGenerator jwt,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _dao = dao;
        _hasher = hasher;
        _jwt = jwt;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<long> RegisterAsync(
        VendorRegistrationFormDTO request,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);

        if (_dao.EmailExists(request.Email.Trim()))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var vendorId = _dao.InsertVendor(
            request.OwnerName.Trim(),
            request.ShopName.Trim(),
            request.Email.Trim(),
            request.Phone.Trim(),
            _hasher.HashPassword(request.Password),
            request.PlaceId);

        var business = await SaveDocumentAsync(
            request.BusinessLicense!, vendorId, "BusinessLicense", cancellationToken);
        _dao.InsertDocument(
            vendorId,
            "BusinessLicense",
            request.BusinessLicense!.FileName,
            business,
            null);

        var foodSafety = await SaveDocumentAsync(
            request.FoodSafetyCertificate!, vendorId, "FoodSafety", cancellationToken);
        _dao.InsertDocument(
            vendorId,
            "FoodSafety",
            request.FoodSafetyCertificate!.FileName,
            foodSafety,
            request.FoodSafetyExpiresAt);

        _dao.InsertNotification(
            vendorId,
            "RegistrationSubmitted",
            "Đã gửi hồ sơ đăng ký",
            "Hồ sơ đang chờ Admin kiểm tra giấy phép kinh doanh và an toàn thực phẩm.",
            "Vendor",
            vendorId);

        return vendorId;
    }

    public VendorLoginResponseDTO Login(VendorLoginRequestDTO request)
    {
        var record = _dao.GetVendorLoginByEmail(request.Email.Trim())
            ?? throw new InvalidOperationException("Email hoặc mật khẩu không đúng.");

        if (!_hasher.VerifyPassword(request.Password, record.PasswordHash))
            throw new InvalidOperationException("Email hoặc mật khẩu không đúng.");

        if (record.User.AccountStatus == VendorAccountStatuses.Suspended)
            throw new InvalidOperationException("Tài khoản đang bị tạm ngưng.");

        RefreshLifecycle(record.User.VendorUserId);
        var vendor = _dao.GetVendorById(record.User.VendorUserId)!;
        var expiresAt = _jwt.AccessTokenExpiresAtUtc;

        return new VendorLoginResponseDTO
        {
            AccessToken = _jwt.GenerateVendorAccessToken(vendor, expiresAt),
            AccessTokenExpiresAt = expiresAt,
            Vendor = vendor
        };
    }

    public VendorDashboardDTO GetDashboard(long vendorUserId)
    {
        RefreshLifecycle(vendorUserId);
        var vendor = _dao.GetVendorById(vendorUserId)
            ?? throw new InvalidOperationException("Vendor không tồn tại.");
        var subscription = _dao.GetLatestSubscription(vendorUserId);
        var pendingPayment = _dao.GetLatestPendingPayment(vendorUserId);
        var daysRemaining = subscription == null
            ? 0
            : Math.Max(0, (int)Math.Ceiling((subscription.ExpiresAt - DateTime.UtcNow).TotalDays));

        return new VendorDashboardDTO
        {
            Vendor = vendor,
            Subscription = subscription,
            PendingPayment = pendingPayment,
            UnreadNotifications = _dao.GetUnreadCount(vendorUserId),
            DaysRemaining = daysRemaining,
            CanManageContent = vendor.AccountStatus is
                VendorAccountStatuses.Active or VendorAccountStatuses.ExpiringSoon,
            ShouldWarnExpiry = daysRemaining is > 0 and <= 15
        };
    }

    public List<AdminVendorListItemDTO> GetAdminVendorList()
    {
        var vendors = _dao.GetAllVendors();
        foreach (var vendor in vendors)
        {
            RefreshLifecycle(vendor.VendorUserId);
            vendor.Documents = _dao.GetDocuments(vendor.VendorUserId);
            vendor.Subscription = _dao.GetLatestSubscription(vendor.VendorUserId);
            vendor.PendingPayment = _dao.GetLatestPendingPayment(vendor.VendorUserId);
        }
        return vendors;
    }

    public PaymentOrderDTO ReviewRegistration(
        long vendorUserId,
        long adminId,
        ReviewVendorRequestDTO request)
    {
        if (!request.Approved && string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Phải nhập lý do từ chối.");

        var documents = _dao.GetDocuments(vendorUserId);
        var business = documents.FirstOrDefault(x => x.DocumentType == "BusinessLicense");
        var foodSafety = documents.FirstOrDefault(x => x.DocumentType == "FoodSafety");
        if (business == null || foodSafety == null)
            throw new InvalidOperationException("Hồ sơ chưa đủ hai giấy phép.");
        if (request.Approved && (!foodSafety.ExpiresAt.HasValue || foodSafety.ExpiresAt.Value.Date <= DateTime.UtcNow.Date))
            throw new InvalidOperationException("Giấy an toàn thực phẩm đã hết hạn.");

        if (!_dao.UpdateVendorReview(vendorUserId, adminId, request.Approved, request.Reason?.Trim()))
            throw new InvalidOperationException("Hồ sơ không ở trạng thái có thể duyệt.");

        _dao.ReviewPendingDocuments(vendorUserId, adminId, request.Approved, request.Reason?.Trim());

        if (!request.Approved)
        {
            _dao.InsertNotification(
                vendorUserId,
                "RegistrationRejected",
                "Hồ sơ đăng ký bị từ chối",
                request.Reason!.Trim(),
                "Vendor",
                vendorUserId);
            return new PaymentOrderDTO();
        }

        var order = CreatePaymentOrder(vendorUserId, null, "Registration", request.Amount);
        _dao.InsertNotification(
            vendorUserId,
            "RegistrationApproved",
            "Hồ sơ đã được duyệt",
            "Admin đã duyệt hồ sơ. Vui lòng thanh toán để kích hoạt gói 6 tháng.",
            "PaymentOrder",
            order.PaymentOrderId);
        return order;
    }

    public async Task<long> SubmitRenewalAsync(
        long vendorUserId,
        VendorRenewalFormDTO request,
        CancellationToken cancellationToken = default)
    {
        if (request.FoodSafetyCertificate == null || request.FoodSafetyCertificate.Length == 0)
            throw new ArgumentException("Phải tải giấy an toàn thực phẩm.");
        if (request.FoodSafetyExpiresAt.Date <= DateTime.UtcNow.Date)
            throw new ArgumentException("Giấy an toàn thực phẩm phải còn hiệu lực.");

        var fileUrl = await SaveDocumentAsync(
            request.FoodSafetyCertificate,
            vendorUserId,
            "FoodSafety",
            cancellationToken);
        var documentId = _dao.InsertDocument(
            vendorUserId,
            "FoodSafety",
            request.FoodSafetyCertificate.FileName,
            fileUrl,
            request.FoodSafetyExpiresAt);
        var renewalId = _dao.InsertRenewalRequest(vendorUserId, documentId);

        _dao.InsertNotification(
            vendorUserId,
            "RenewalSubmitted",
            "Đã gửi hồ sơ gia hạn",
            "Giấy an toàn thực phẩm mới đang chờ Admin kiểm tra.",
            "RenewalRequest",
            renewalId);

        return renewalId;
    }

    public List<VendorRenewalRequestDTO> GetPendingRenewals() =>
        _dao.GetPendingRenewals();

    public PaymentOrderDTO ReviewRenewal(
        long requestId,
        long adminId,
        ReviewRenewalRequestDTO request)
    {
        if (!request.Approved && string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Phải nhập lý do từ chối.");

        var renewal = _dao.GetRenewalRequest(requestId)
            ?? throw new InvalidOperationException("Yêu cầu gia hạn không tồn tại.");

        if (!_dao.ReviewRenewal(requestId, adminId, request.Approved, request.Reason?.Trim()))
            throw new InvalidOperationException("Yêu cầu gia hạn không thể duyệt.");

        _dao.ReviewPendingDocuments(
            renewal.VendorUserId,
            adminId,
            request.Approved,
            request.Reason?.Trim());

        if (!request.Approved)
        {
            _dao.InsertNotification(
                renewal.VendorUserId,
                "RenewalRejected",
                "Hồ sơ gia hạn bị từ chối",
                request.Reason!.Trim(),
                "RenewalRequest",
                requestId);
            return new PaymentOrderDTO();
        }

        var order = CreatePaymentOrder(
            renewal.VendorUserId,
            requestId,
            "Renewal",
            request.Amount);
        _dao.InsertNotification(
            renewal.VendorUserId,
            "RenewalApproved",
            "Hồ sơ gia hạn đã được duyệt",
            "Vui lòng thanh toán để cộng thêm 6 tháng sử dụng.",
            "PaymentOrder",
            order.PaymentOrderId);
        return order;
    }

    public VendorDashboardDTO ConfirmMockPayment(string orderCode, long vendorUserId)
    {
        var order = _dao.GetPaymentByOrderCode(orderCode)
            ?? throw new InvalidOperationException("Mã thanh toán không tồn tại.");
        if (order.VendorUserId != vendorUserId)
            throw new UnauthorizedAccessException("Mã thanh toán không thuộc tài khoản này.");
        if (order.Status == "Paid") return GetDashboard(vendorUserId);
        if (order.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Mã thanh toán đã hết hạn.");
        if (!_dao.MarkPaymentPaid(order.PaymentOrderId))
            throw new InvalidOperationException("Không thể xác nhận thanh toán.");

        var current = _dao.GetLatestSubscription(vendorUserId);
        var startsAt = DateTime.UtcNow;
        var baseDate = current != null && current.ExpiresAt > startsAt
            ? current.ExpiresAt
            : startsAt;
        var expiresAt = baseDate.AddMonths(6);

        _dao.InsertSubscription(
            vendorUserId,
            order.PaymentOrderId,
            startsAt,
            expiresAt);
        _dao.SetVendorAccountStatus(vendorUserId, VendorAccountStatuses.Active, true);

        var vendor = _dao.GetVendorById(vendorUserId)!;
        _dao.AssignPlaceOwner(vendor.PlaceId, vendorUserId);
        if (order.RenewalRequestId.HasValue)
            _dao.MarkRenewalPaid(order.RenewalRequestId.Value);

        _dao.InsertNotification(
            vendorUserId,
            "PaymentSucceeded",
            "Thanh toán demo thành công",
            $"Tài khoản được kích hoạt đến {expiresAt:dd/MM/yyyy HH:mm}.",
            "PaymentOrder",
            order.PaymentOrderId);

        return GetDashboard(vendorUserId);
    }

    public List<VendorNotificationDTO> GetNotifications(long vendorUserId) =>
        _dao.GetNotifications(vendorUserId);

    public bool MarkNotificationRead(long vendorUserId, long notificationId) =>
        _dao.MarkNotificationRead(notificationId, vendorUserId);

    public void EnsureCanManageContent(long vendorUserId)
    {
        var dashboard = GetDashboard(vendorUserId);
        if (!dashboard.CanManageContent)
            throw new InvalidOperationException(
                "Tài khoản chưa kích hoạt hoặc đã hết hạn. Chỉ được truy cập chức năng gia hạn.");
    }

    public void NotifyNarration(
        long vendorUserId,
        string type,
        string title,
        string message,
        long narrationId)
    {
        _dao.InsertNotification(
            vendorUserId,
            type,
            title,
            message,
            "Narration",
            narrationId);
    }

    private PaymentOrderDTO CreatePaymentOrder(
        long vendorUserId,
        long? renewalRequestId,
        string purpose,
        decimal amount)
    {
        var orderCode = $"VK-{purpose[..3].ToUpperInvariant()}-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}";
        var frontendUrl =
            Environment.GetEnvironmentVariable("FRONTEND_URL")
            ?? _configuration["FrontendUrl"]
            ?? "http://localhost:5173";
        var paymentUrl = $"{frontendUrl.TrimEnd('/')}/vendor/mock-payment?orderCode={Uri.EscapeDataString(orderCode)}";
        var qrUrl = "https://api.qrserver.com/v1/create-qr-code/?size=300x300&data="
            + Uri.EscapeDataString(paymentUrl);

        var order = new PaymentOrderDTO
        {
            VendorUserId = vendorUserId,
            RenewalRequestId = renewalRequestId,
            Purpose = purpose,
            OrderCode = orderCode,
            Amount = amount <= 0 ? 500000 : amount,
            Status = "Pending",
            Provider = "Mock",
            PaymentUrl = paymentUrl,
            QrImageUrl = qrUrl,
            ExpiresAt = DateTime.UtcNow.AddDays(3)
        };
        order.PaymentOrderId = _dao.InsertPaymentOrder(order);
        return order;
    }

    private void RefreshLifecycle(long vendorUserId)
    {
        var subscription = _dao.GetLatestSubscription(vendorUserId);
        if (subscription == null || subscription.Status != "Active") return;

        var remaining = subscription.ExpiresAt - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            _dao.ExpireSubscription(subscription.SubscriptionId);
            _dao.SetVendorAccountStatus(vendorUserId, VendorAccountStatuses.Expired, false);
            if (!_dao.NotificationExists(vendorUserId, "SubscriptionExpired", "Subscription", subscription.SubscriptionId))
            {
                _dao.InsertNotification(
                    vendorUserId,
                    "SubscriptionExpired",
                    "Gói dịch vụ đã hết hạn",
                    "Tài khoản bị khóa nội dung công khai. Hãy gửi giấy an toàn thực phẩm để gia hạn.",
                    "Subscription",
                    subscription.SubscriptionId);
            }
            return;
        }

        if (remaining.TotalDays <= 15)
        {
            _dao.SetVendorAccountStatus(vendorUserId, VendorAccountStatuses.ExpiringSoon, true);
            if (!_dao.NotificationExists(vendorUserId, "SubscriptionExpiring", "Subscription", subscription.SubscriptionId))
            {
                _dao.InsertNotification(
                    vendorUserId,
                    "SubscriptionExpiring",
                    "Gói dịch vụ sắp hết hạn",
                    $"Gói còn khoảng {Math.Max(1, (int)Math.Ceiling(remaining.TotalDays))} ngày. Vui lòng gia hạn.",
                    "Subscription",
                    subscription.SubscriptionId);
            }
        }
        else
        {
            _dao.SetVendorAccountStatus(vendorUserId, VendorAccountStatuses.Active, true);
        }
    }

    private async Task<string> SaveDocumentAsync(
        IFormFile file,
        long vendorUserId,
        string documentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new HashSet<string> { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension))
            throw new ArgumentException("Giấy phép chỉ nhận PDF hoặc ảnh.");
        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("Mỗi file giấy phép tối đa 10 MB.");

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "vendor-documents", vendorUserId.ToString());
        Directory.CreateDirectory(folder);
        var storedName = $"{documentType}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, storedName);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/vendor-documents/{vendorUserId}/{storedName}";
    }

    private static void ValidateRegistration(VendorRegistrationFormDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName))
            throw new ArgumentException("Tên chủ sạp là bắt buộc.");
        if (string.IsNullOrWhiteSpace(request.ShopName))
            throw new ArgumentException("Tên sạp là bắt buộc.");
        if (!ValidationHelper.IsValidEmail(request.Email))
            throw new ArgumentException("Email không hợp lệ.");
        if (request.Password.Length < 6)
            throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự.");
        if (request.BusinessLicense == null || request.BusinessLicense.Length == 0)
            throw new ArgumentException("Phải tải giấy phép kinh doanh.");
        if (request.FoodSafetyCertificate == null || request.FoodSafetyCertificate.Length == 0)
            throw new ArgumentException("Phải tải giấy an toàn thực phẩm.");
        if (request.FoodSafetyExpiresAt.Date <= DateTime.UtcNow.Date)
            throw new ArgumentException("Giấy an toàn thực phẩm phải còn hiệu lực.");
    }
}
