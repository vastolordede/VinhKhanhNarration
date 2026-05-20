using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS.Interfaces;

namespace VinhKhanhNarration.Api.Controllers;

public abstract class CrudControllerBase<TDto> : BaseApiController where TDto : class
{
    private readonly ICrudBUS<TDto, long> _bus;

    protected CrudControllerBase(ICrudBUS<TDto, long> bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public virtual IActionResult Create([FromBody] TDto dto)
    {
        try { return CreatedData(_bus.Create(dto)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }

    [HttpGet]
    public virtual IActionResult GetAll() => OkData(_bus.GetAll());

    [HttpGet("active")]
    public virtual IActionResult GetActive() => OkData(_bus.GetActive());

    [HttpGet("{id:long}")]
    public virtual IActionResult GetById(long id)
    {
        var item = _bus.GetById(id);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [HttpPut("{id:long}")]
    public virtual IActionResult Update(long id, [FromBody] TDto dto)
    {
        try
        {
            var prop = typeof(TDto).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id"));
            prop?.SetValue(dto, id);
            return OkData(_bus.Update(dto));
        }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }

    [HttpPatch("{id:long}/deactivate")]
    public virtual IActionResult Deactivate(long id) => OkData(_bus.Deactivate(id));

    [HttpPatch("{id:long}/restore")]
    public virtual IActionResult Restore(long id) => OkData(_bus.Restore(id));
}
