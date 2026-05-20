using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO.Lookup;

namespace VinhKhanhNarration.Api.Controllers;

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

    [HttpGet("active")]
    public IActionResult GetActive() => OkData(_bus.GetActive());

    [HttpGet("{id:long}")]
    public IActionResult GetById(long id)
    {
        var item = _bus.GetById(id);
        return item == null ? NotFoundMessage() : OkData(item);
    }

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
