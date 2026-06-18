using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/narration-contents")]
public class NarrationContentsController : CrudControllerBase<NarrationContentDTO>
{
    private readonly NarrationContentBUS _bus;

    public NarrationContentsController(NarrationContentBUS bus) : base(bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public override IActionResult Create([FromBody] NarrationContentDTO dto) =>
        StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message = "Admin không được tạo nội dung. Chỉ Vendor được thêm và sửa nội dung."
        });

    [HttpPut("{id:long}")]
    public override IActionResult Update(long id, [FromBody] NarrationContentDTO dto) =>
        StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message = "Admin không được sửa nội dung nguồn của Vendor."
        });

    [HttpPatch("{id:long}/deactivate")]
    public override IActionResult Deactivate(long id) =>
        StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message = "Hãy dùng endpoint moderation và nhập lý do."
        });

    [HttpPatch("{id:long}/restore")]
    public override IActionResult Restore(long id) =>
        StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message = "Hãy dùng endpoint moderation."
        });

    [HttpGet("place/{placeId:long}")]
    public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));

    [HttpGet("dish/{dishId:long}")]
    public IActionResult GetByDish(long dishId) => OkData(_bus.GetByDishId(dishId));

    [HttpGet("pending-review")]
    public IActionResult GetPendingReview() => OkData(_bus.GetPendingReview());

    [HttpPatch("{narrationId:long}/review")]
    public async Task<IActionResult> Review(
        long narrationId,
        [FromBody] ReviewNarrationRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.AdminId = GetAdminId();
            return OkData(await _bus.ReviewNarrationAsync(
                narrationId,
                request,
                cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{narrationId:long}/moderation")]
    public IActionResult Moderate(
        long narrationId,
        [FromBody] NarrationModerationRequestDTO request)
    {
        try
        {
            return OkData(_bus.AdminModerateNarration(
                narrationId,
                GetAdminId(),
                request));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("{narrationId:long}/process-pipeline")]
    public async Task<IActionResult> ProcessPipeline(
        long narrationId,
        [FromBody] AutoProcessNarrationRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.AdminId = GetAdminId();
            return OkData(await _bus.ProcessApprovedNarrationAsync(
                narrationId,
                request.AdminId,
                cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("{narrationId:long}/generate-translations")]
    public async Task<IActionResult> GenerateTranslations(
        long narrationId,
        [FromBody] GenerateTranslationsRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.AdminId = GetAdminId();
            return OkData(await _bus.GenerateTranslationsAsync(
                narrationId,
                request,
                cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{narrationId:long}/publish")]
    public IActionResult Publish(long narrationId, [FromBody] PublishNarrationRequestDTO request)
    {
        try
        {
            request.AdminId = GetAdminId();
            return OkData(_bus.Publish(narrationId, request));
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

[Authorize(Roles = "Vendor")]
[Route("api/vendor/narrations")]
public class VendorNarrationsController : BaseApiController
{
    private readonly NarrationContentBUS _bus;

    public VendorNarrationsController(NarrationContentBUS bus)
    {
        _bus = bus;
    }

    [HttpGet]
    public IActionResult GetMine()
    {
        try { return OkData(_bus.GetVendorNarrations(GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost]
    public IActionResult CreateAndSubmit([FromBody] VendorNarrationRequestDTO request)
    {
        try
        {
            request.VendorUserId = GetVendorId();
            return CreatedData(_bus.CreateVendorDraft(request));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPut("{narrationId:long}")]
    public IActionResult UpdateAndSubmit(
        long narrationId,
        [FromBody] VendorNarrationRequestDTO request)
    {
        try
        {
            request.VendorUserId = GetVendorId();
            return OkData(_bus.UpdateVendorDraft(narrationId, request));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{narrationId:long}/submit")]
    public IActionResult Submit(long narrationId)
    {
        try { return OkData(_bus.SubmitVendorNarration(narrationId, GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{narrationId:long}/{operation:regex(^(hide|delete|restore)$)}")]
public IActionResult Moderate(long narrationId, string operation)
{
    try
    {
        return OkData(_bus.VendorModerateNarration(
            narrationId,
            GetVendorId(),
            operation));
    }
    catch (Exception ex) { return BadRequestException(ex); }
}

    [HttpGet("{narrationId:long}/status")]
    public IActionResult GetStatus(long narrationId)
    {
        try { return OkData(_bus.GetVendorNarrationStatus(narrationId, GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

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
[Route("api/narration-translations")]
public class NarrationTranslationsController : CrudControllerBase<NarrationTranslationDTO>
{
    private readonly NarrationTranslationBUS _bus;

    public NarrationTranslationsController(NarrationTranslationBUS bus) : base(bus)
    {
        _bus = bus;
    }

    [HttpGet("narration/{narrationId:long}")]
    public IActionResult GetByNarration(long narrationId) =>
        OkData(_bus.GetByNarrationId(narrationId));

    [HttpGet("narration/{narrationId:long}/language/{languageId:long}")]
    public IActionResult GetByNarrationAndLanguage(long narrationId, long languageId) =>
        OkData(_bus.GetByNarrationAndLanguage(narrationId, languageId));

    [HttpPatch("{translationId:long}/review")]
    public IActionResult Review(
        long translationId,
        [FromBody] ReviewTranslationRequestDTO request)
    {
        try { return OkData(_bus.ReviewTranslation(translationId, request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/audio-files")]
public class AudioFilesController : CrudControllerBase<AudioFileDTO>
{
    private readonly AudioFileBUS _bus;

    public AudioFilesController(AudioFileBUS bus) : base(bus)
    {
        _bus = bus;
    }

    [HttpGet("translation/{translationId:long}")]
    public IActionResult GetByTranslation(long translationId) =>
        OkData(_bus.GetByTranslationId(translationId));

    [HttpPost("generate/{translationId:long}")]
    public async Task<IActionResult> Generate(
        long translationId,
        [FromBody] GenerateAudioRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminValue = User.FindFirstValue("adminId")
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(adminValue, out var adminId)) request.AdminId = adminId;
            return OkData(await _bus.GenerateAsync(
                translationId,
                request,
                cancellationToken));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }
}

[Route("api/public/narrations")]
public class PublicNarrationsController : BaseApiController
{
    private readonly PublicNarrationBUS _bus;

    public PublicNarrationsController(PublicNarrationBUS bus)
    {
        _bus = bus;
    }

    [HttpGet("place/{placeId:long}")]
    public IActionResult ResolvePlace(long placeId, [FromQuery] long languageId)
    {
        try { return OkData(_bus.ResolvePlace(placeId, languageId)); }
        catch (InvalidOperationException ex) { return NotFoundMessage(ex.Message); }
    }

    [HttpGet("dish/{dishId:long}")]
    public IActionResult ResolveDish(long dishId, [FromQuery] long languageId)
    {
        try { return OkData(_bus.ResolveDish(dishId, languageId)); }
        catch (InvalidOperationException ex) { return NotFoundMessage(ex.Message); }
    }

    [HttpGet("{narrationId:long}")]
    public IActionResult ResolveNarration(long narrationId, [FromQuery] long languageId)
    {
        try { return OkData(_bus.ResolveNarration(narrationId, languageId)); }
        catch (InvalidOperationException ex) { return NotFoundMessage(ex.Message); }
    }
}
