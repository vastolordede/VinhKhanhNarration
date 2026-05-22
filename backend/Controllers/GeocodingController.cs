using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[ApiController]
[Route("api/admin/geocoding")]
public class GeocodingController : ControllerBase
{
    private readonly GeocodingBUS _geocodingBUS;

    public GeocodingController(GeocodingBUS geocodingBUS)
    {
        _geocodingBUS = geocodingBUS;
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve([FromBody] GeocodingRequestDTO request)
    {
        var result = await _geocodingBUS.ResolveAddressAsync(request.Address);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy tọa độ cho địa chỉ này."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Geocoding success.",
            data = result
        });
    }
}