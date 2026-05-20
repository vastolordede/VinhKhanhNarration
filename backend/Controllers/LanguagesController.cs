using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/languages")]
public class LanguagesController : CrudControllerBase<LanguageDTO>
{
    private readonly LanguageBUS _bus;
    public LanguagesController(LanguageBUS bus) : base(bus) => _bus = bus;

    [HttpGet("default")]
    public IActionResult GetDefault()
    {
        var item = _bus.GetDefault();
        return item == null ? NotFoundMessage("Default language not found.") : OkData(item);
    }

    [HttpGet("code/{code}")]
    public IActionResult GetByCode(string code)
    {
        var item = _bus.GetByCode(code);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [HttpPatch("{id:long}/set-default")]
    public IActionResult SetDefault(long id) => OkData(_bus.SetDefaultLanguage(id));
}
