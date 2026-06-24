using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/dashboard")]
public class AdminDashboardController : BaseApiController
{
    private readonly AdminDashboardBUS _bus;

    public AdminDashboardController(AdminDashboardBUS bus) => _bus = bus;

    [HttpGet("statistics")]
    public IActionResult GetStatistics([FromQuery] string period = "month")
    {
        try { return OkData(_bus.GetStatistics(period)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }
}
