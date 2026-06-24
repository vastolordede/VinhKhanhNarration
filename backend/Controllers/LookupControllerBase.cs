using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO.Lookup;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin,ContentManager")]
public abstract class LookupControllerBase<TDto> : BaseApiController where TDto : LookupDTO, new()
{
    private readonly LookupBUS<TDto> _bus;

    protected LookupControllerBase(LookupBUS<TDto> bus) => _bus = bus;

    [HttpPost]
    public IActionResult Create([FromBody] TDto dto)
    {
        try { return CreatedData(_bus.Create(dto)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }

    [HttpGet]
    public IActionResult GetAll() => OkData(_bus.GetAll());

    [AllowAnonymous]
    [HttpGet("active")]
    public IActionResult GetActive() => OkData(_bus.GetActive());

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public IActionResult GetById(long id)
    {
        var item = _bus.GetById(id);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [AllowAnonymous]
    [HttpGet("code/{code}")]
    public IActionResult GetByCode(string code)
    {
        var item = _bus.GetByCode(code);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] TDto dto)
    {
        try { dto.Id = id; return OkData(_bus.Update(dto)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }

    [HttpPatch("{id:long}/deactivate")]
    public IActionResult Deactivate(long id) => OkData(_bus.Deactivate(id));

    [HttpPatch("{id:long}/restore")]
    public IActionResult Restore(long id) => OkData(_bus.Restore(id));
}
