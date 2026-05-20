using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/admin-users")]
public class AdminUsersController : CrudControllerBase<AdminUserDTO>
{
    private readonly AdminUserBUS _bus;
    public AdminUsersController(AdminUserBUS bus) : base(bus) => _bus = bus;

    [HttpPatch("{id:long}/change-password")]
    public IActionResult ChangePassword(long id, [FromBody] ChangePasswordRequestDTO request)
    {
        try { return OkData(_bus.ChangePassword(id, request.OldPassword, request.NewPassword)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AdminUserBUS _bus;
    public AuthController(AdminUserBUS bus) => _bus = bus;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDTO request)
    {
        var result = _bus.Login(request.Email, request.Password);
        return result == null ? BadRequestMessage("Invalid email or password.") : OkData(result);
    }
}
