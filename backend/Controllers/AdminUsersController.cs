using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin-users")]
public class AdminUsersController : CrudControllerBase<AdminUserDTO>
{
    private readonly AdminUserBUS _bus;
    public AdminUsersController(AdminUserBUS bus) : base(bus) => _bus = bus;

    [HttpPatch("{id:long}/change-password")]
    public IActionResult ChangePassword(long id, [FromBody] ChangePasswordRequestDTO request)
    {
        try
        {
            return OkData(_bus.ChangePassword(
                id,
                request.OldPassword,
                request.NewPassword,
                HttpContext.Connection.RemoteIpAddress?.ToString()));
        }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AdminUserBUS _bus;

    public AuthController(AdminUserBUS bus)
    {
        _bus = bus;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDTO request)
    {
        var result = _bus.Login(request.Email, request.Password, GetIpAddress());
        return result == null
            ? BadRequestMessage("Invalid email or password.")
            : OkData(result);
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshTokenRequestDTO request)
    {
        try
        {
            var result = _bus.RefreshAccessToken(request.RefreshToken, GetIpAddress());
            return OkData(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new
            {
                success = false,
                message = ex.Message,
                data = (object?)null
            });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequestDTO request)
    {
        _bus.Logout(request.RefreshToken, GetIpAddress());
        return OkData(true, "Logged out.");
    }

    [Authorize]
    [HttpPost("logout-all")]
    public IActionResult LogoutAll()
    {
        var adminIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(adminIdText, out var adminId))
        {
            return Unauthorized();
        }

        _bus.LogoutAll(adminId, GetIpAddress());
        return OkData(true, "Logged out from all sessions.");
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
