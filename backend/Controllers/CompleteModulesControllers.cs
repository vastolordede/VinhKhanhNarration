using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/public/access")]
public class GuestAccessController : BaseApiController
{
    private readonly GuestAccessBUS _bus;
    public GuestAccessController(GuestAccessBUS bus) => _bus = bus;

    [HttpGet("status/{guestSessionId}")]
    public IActionResult Status(string guestSessionId)
    {
        try { return OkData(_bus.GetStatus(guestSessionId)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("orders")]
    public IActionResult CreateOrder([FromBody] CreateGuestPaymentRequestDTO request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            return CreatedData(_bus.CreatePaymentOrder(request, ipAddress));
        }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpGet("orders/{orderCode}")]
    public IActionResult GetOrder(string orderCode)
    {
        try { return OkData(_bus.GetPaymentOrder(orderCode)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("orders/{orderCode}/confirm")]
    public IActionResult Confirm(string orderCode)
    {
        try { return OkData(_bus.ConfirmMockPayment(orderCode)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }
}

[Authorize(Roles = "Vendor")]
[Route("api/vendor/catalog")]
public class VendorCatalogController : BaseApiController
{
    private readonly VendorCatalogBUS _bus;
    public VendorCatalogController(VendorCatalogBUS bus) => _bus = bus;

    [HttpGet]
    public IActionResult GetCatalog()
    {
        try { return OkData(_bus.GetCatalog(GetVendorId())); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPut("place")]
    public IActionResult SavePlace([FromBody] PlaceDTO request)
    {
        try { return OkData(_bus.SavePlace(GetVendorId(), request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPost("dishes")]
    public IActionResult CreateDish([FromBody] VendorDishRequestDTO request)
    {
        try { return CreatedData(_bus.CreateDish(GetVendorId(), request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPut("dishes/{dishId:long}")]
    public IActionResult UpdateDish(long dishId, [FromBody] VendorDishRequestDTO request)
    {
        try { return OkData(_bus.UpdateDish(GetVendorId(), dishId, request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("dishes/{dishId:long}/deactivate")]
    public IActionResult DeactivateDish(long dishId)
    {
        try { return OkData(_bus.DeactivateDish(GetVendorId(), dishId)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("dishes/{dishId:long}/restore")]
    public IActionResult RestoreDish(long dishId)
    {
        try { return OkData(_bus.RestoreDish(GetVendorId(), dishId)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPut("menu/{dishId:long}")]
    public IActionResult SaveMenuItem(
        long dishId,
        [FromBody] VendorMenuItemRequestDTO request)
    {
        try { return OkData(_bus.UpsertMenuItem(GetVendorId(), dishId, request)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpDelete("menu/{dishId:long}")]
    public IActionResult RemoveMenuItem(long dishId)
    {
        try { return OkData(_bus.RemoveMenuItem(GetVendorId(), dishId)); }
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
[Route("api/admin/audit-logs")]
public class AuditLogsController : BaseApiController
{
    private readonly AuditLogBUS _bus;
    public AuditLogsController(AuditLogBUS bus) => _bus = bus;

    [HttpGet]
    public IActionResult GetPaged([FromQuery] AuditLogQueryDTO query) =>
        OkData(_bus.GetPaged(query));
}

[Route("api/public/audio")]
public class PublicAudioController : ControllerBase
{
    private readonly GuestAccessBUS _access;
    private readonly AudioFileDAO _audioDAO;
    private readonly IAudioStorage _audioStorage;
    private readonly IWebHostEnvironment _environment;

    public PublicAudioController(
        GuestAccessBUS access,
        AudioFileDAO audioDAO,
        IAudioStorage audioStorage,
        IWebHostEnvironment environment)
    {
        _access = access;
        _audioDAO = audioDAO;
        _audioStorage = audioStorage;
        _environment = environment;
    }

    [HttpGet("{audioId:long}")]
    public async Task<IActionResult> Stream(
        long audioId,
        [FromQuery] string guestSessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            _access.EnsureActive(guestSessionId);

            var audio = _audioDAO.GetById(audioId);
            if (audio == null
                || !audio.IsActive
                || audio.Status != AudioStatuses.Ready
                || audio.PublishedAt == null
                || (string.IsNullOrWhiteSpace(audio.StorageKey)
                    && string.IsNullOrWhiteSpace(audio.AudioUrl)))
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(audio.StorageKey))
            {
                var storedAudio = await _audioStorage.OpenReadAsync(
                    audio.StorageKey,
                    cancellationToken);

                return ToPlaybackResult(storedAudio);
            }

            return OpenLegacyAudio(audio.AudioUrl!);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    private IActionResult ToPlaybackResult(AudioStorageReadResult? audio)
    {
        if (audio == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(audio.RedirectUrl))
        {
            return Redirect(audio.RedirectUrl);
        }

        if (audio.ContentStream == null)
        {
            return NotFound();
        }

        return File(
            audio.ContentStream,
            audio.ContentType,
            enableRangeProcessing: true);
    }

    private IActionResult OpenLegacyAudio(string audioUrl)
    {
        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var absolute)
            && !absolute.AbsolutePath.StartsWith(
                "/generated-audio/",
                StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(audioUrl);
        }

        var pathValue = Uri.TryCreate(audioUrl, UriKind.Absolute, out absolute)
            ? absolute.AbsolutePath
            : audioUrl;
        var fileName = Path.GetFileName(Uri.UnescapeDataString(pathValue));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var directory = Path.GetFullPath(Path.Combine(webRoot, "generated-audio"));
        var filePath = Path.GetFullPath(Path.Combine(directory, fileName));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var directoryPrefix = directory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!filePath.StartsWith(directoryPrefix, comparison)
            || !System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return PhysicalFile(
            filePath,
            "audio/mpeg",
            enableRangeProcessing: true);
    }
}
