using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/narration-contents")]
public class NarrationContentsController : CrudControllerBase<NarrationContentDTO>
{
    private readonly NarrationContentBUS _bus;
    public NarrationContentsController(NarrationContentBUS bus) : base(bus) => _bus = bus;

    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));
    [HttpGet("dish/{dishId:long}")] public IActionResult GetByDish(long dishId) => OkData(_bus.GetByDishId(dishId));
    [HttpGet("place/{placeId:long}/main")] public IActionResult MainByPlace(long placeId) => OkData(_bus.FindNarrationForPlace(placeId));
    [HttpGet("dish/{dishId:long}/main")] public IActionResult MainByDish(long dishId) => OkData(_bus.FindNarrationForDish(dishId));
}

[Route("api/narration-translations")]
public class NarrationTranslationsController : CrudControllerBase<NarrationTranslationDTO>
{
    private readonly NarrationTranslationBUS _bus;
    public NarrationTranslationsController(NarrationTranslationBUS bus) : base(bus) => _bus = bus;

    [HttpGet("narration/{narrationId:long}")] public IActionResult GetByNarration(long narrationId) => OkData(_bus.GetByNarrationId(narrationId));
    [HttpGet("narration/{narrationId:long}/language/{languageId:long}")] public IActionResult GetByNarrationAndLanguage(long narrationId, long languageId) => OkData(_bus.GetByNarrationAndLanguage(narrationId, languageId));

    [HttpPatch("{id:long}/review")]
    public IActionResult Review(long id, [FromQuery] long reviewerAdminId) => OkData(_bus.ReviewTranslation(id, reviewerAdminId));
}

[Route("api/audio-files")]
public class AudioFilesController : CrudControllerBase<AudioFileDTO>
{
    private readonly AudioFileBUS _bus;
    public AudioFilesController(AudioFileBUS bus) : base(bus) => _bus = bus;

    [HttpGet("translation/{translationId:long}")] public IActionResult GetByTranslation(long translationId) => OkData(_bus.GetByTranslationId(translationId));
    [HttpGet("playable")] public IActionResult GetPlayable([FromQuery] long narrationId, [FromQuery] long languageId) => OkData(_bus.GetPlayableAudio(narrationId, languageId));
}
