using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/vendor-auth")]
public class VendorAuthController : BaseApiController
{
    private readonly VendorModuleBUS _bus;

    public VendorAuthController(VendorModuleBUS bus) => _bus = bus;

    [HttpPost("register")]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> Register(
        [FromForm] VendorRegistrationFormDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            return CreatedData(await _bus.RegisterAsync(request, cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] VendorLoginRequestDTO request)
    {
        try { return OkData(_bus.Login(request, GetIpAddress())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] VendorRefreshTokenRequestDTO request)
    {
        try { return OkData(_bus.RefreshAccessToken(request.RefreshToken, GetIpAddress())); }
        catch (Exception ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] VendorLogoutRequestDTO request)
    {
        _bus.Logout(request.RefreshToken, GetIpAddress());
        return OkData(true, "Đã đăng xuất.");
    }

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}

[Authorize(Roles = "Vendor")]
[Route("api/vendor/account")]
public class VendorAccountController : BaseApiController
{
    private readonly VendorModuleBUS _bus;

    public VendorAccountController(VendorModuleBUS bus) => _bus = bus;

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        try { return OkData(_bus.GetDashboard(GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] VendorProfileUpdateDTO request)
    {
        try { return OkData(_bus.UpdateProfile(GetVendorId(), request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("change-password")]
    public IActionResult ChangePassword([FromBody] VendorChangePasswordRequestDTO request)
    {
        try { return OkData(_bus.ChangePassword(GetVendorId(), request, GetIpAddress())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("logout-all")]
    public IActionResult LogoutAll()
    {
        _bus.LogoutAll(GetVendorId(), GetIpAddress());
        return OkData(true, "Đã đăng xuất khỏi tất cả thiết bị.");
    }

    [HttpPost("renewal")]
    [RequestSizeLimit(12_000_000)]
    public async Task<IActionResult> SubmitRenewal(
        [FromForm] VendorRenewalFormDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            return CreatedData(await _bus.SubmitRenewalAsync(
                GetVendorId(), request, cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("mock-payments/{orderCode}/confirm")]
    public IActionResult ConfirmMockPayment(string orderCode)
    {
        try { return OkData(_bus.ConfirmMockPayment(orderCode, GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpGet("notifications")]
    public IActionResult Notifications()
    {
        try { return OkData(_bus.GetNotifications(GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("notifications/{notificationId:long}/read")]
    public IActionResult MarkNotificationRead(long notificationId)
    {
        try { return OkData(_bus.MarkNotificationRead(GetVendorId(), notificationId)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private long GetVendorId()
    {
        var value = User.FindFirstValue("vendorUserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(value, out var id) || id <= 0)
            throw new UnauthorizedAccessException("Vendor token không hợp lệ.");
        return id;
    }
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/vendors")]
public class AdminVendorsController : BaseApiController
{
    private readonly VendorModuleBUS _bus;

    public AdminVendorsController(VendorModuleBUS bus) => _bus = bus;

    [HttpGet]
    public IActionResult GetAll()
    {
        try { return OkData(_bus.GetAdminVendorList()); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{vendorUserId:long}/review")]
    public IActionResult ReviewRegistration(
        long vendorUserId,
        [FromBody] ReviewVendorRequestDTO request)
    {
        try
        {
            return OkData(_bus.ReviewRegistration(
                vendorUserId, GetAdminId(), request));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpGet("renewals/pending")]
    public IActionResult PendingRenewals()
    {
        try { return OkData(_bus.GetPendingRenewals()); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("renewals/{requestId:long}/review")]
    public IActionResult ReviewRenewal(
        long requestId,
        [FromBody] ReviewRenewalRequestDTO request)
    {
        try
        {
            return OkData(_bus.ReviewRenewal(
                requestId, GetAdminId(), request));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    private long GetAdminId()
    {
        var value = User.FindFirstValue("adminId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(value, out var id) || id <= 0)
            throw new UnauthorizedAccessException("Admin token không hợp lệ.");
        return id;
    }
}
