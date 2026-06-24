using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/public/guest-sessions")]
public class PublicGuestSessionsController : BaseApiController
{
    private readonly GuestSessionBUS _bus;
    public PublicGuestSessionsController(GuestSessionBUS bus) => _bus = bus;

    [HttpPatch("{guestSessionId}/language")]
    public IActionResult ChangeLanguage(string guestSessionId, [FromBody] ChangeGuestLanguageRequestDTO request)
        => OkData(_bus.ChangePreferredLanguage(guestSessionId, request.LanguageId));

    [HttpPatch("{guestSessionId}/touch")]
    public IActionResult Touch(string guestSessionId) => OkData(_bus.Touch(guestSessionId));
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/guest-sessions")]
public class AdminGuestSessionsController : BaseApiController
{
    private readonly GuestSessionBUS _bus;
    public AdminGuestSessionsController(GuestSessionBUS bus) => _bus = bus;
    [HttpGet] public IActionResult GetActive() => OkData(_bus.GetActiveSessions());
    [HttpGet("{guestSessionId}")] public IActionResult GetById(string guestSessionId) => OkData(_bus.GetById(guestSessionId));
    [HttpPatch("{guestSessionId}/deactivate")] public IActionResult Deactivate(string guestSessionId) => OkData(_bus.Deactivate(guestSessionId));
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/guest-poi-states")]
public class GuestPoiStatesController : BaseApiController
{
    private readonly GuestPoiStateBUS _bus;
    public GuestPoiStatesController(GuestPoiStateBUS bus) => _bus = bus;
    [HttpGet("session/{guestSessionId}")] public IActionResult GetBySession(string guestSessionId) => OkData(_bus.GetByGuestSessionId(guestSessionId));
    [HttpGet("session/{guestSessionId}/place/{placeId:long}")] public IActionResult GetState(string guestSessionId, long placeId) => OkData(_bus.GetState(guestSessionId, placeId));
}

[Route("api/public/geofence")]
public class PublicGeofenceController : BaseApiController
{
    private readonly GeofenceBUS _bus;
    private readonly GuestAccessBUS _access;
    public PublicGeofenceController(GeofenceBUS bus, GuestAccessBUS access)
    {
        _bus = bus;
        _access = access;
    }

    [HttpPost("check")]
    public IActionResult Check([FromBody] GeofenceCheckRequestDTO request)
    {
        try
        {
            var pass = _access.EnsureActive(request.GuestSessionId);
            var result = _bus.CheckLocation(
                request.GuestSessionId, pass.AccessPassId, request.Latitude, request.Longitude, request.LanguageId);
            if (result.AudioId.HasValue)
            {
                result.AudioUrl = Url.Action(
                    "Stream",
                    "PublicAudio",
                    new { audioId = result.AudioId.Value, guestSessionId = request.GuestSessionId },
                    Request.Scheme);
            }
            return OkData(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                success = false,
                message = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/geofence-events")]
public class GeofenceEventsController : BaseApiController
{
    private readonly GeofenceBUS _bus;
    public GeofenceEventsController(GeofenceBUS bus) => _bus = bus;
    [HttpGet]
public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    => OkData(_bus.GetPaged(page, pageSize));
    [HttpGet("{eventId:long}")] public IActionResult GetById(long eventId) => OkData(_bus.GetById(eventId));
    [HttpGet("session/{guestSessionId}")] public IActionResult GetBySession(string guestSessionId) => OkData(_bus.GetByGuestSessionId(guestSessionId));
    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));
    [HttpGet("date-range")] public IActionResult GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to) => OkData(_bus.GetByDateRange(from, to));
}

public class UpdatePlaybackStatusRequestDTO
{
    public string GuestSessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
public class UpdateListenDurationRequestDTO
{
    public string GuestSessionId { get; set; } = string.Empty;
    public int Seconds { get; set; }
}

[Route("api/public/listening-histories")]
public class PublicListeningHistoriesController : BaseApiController
{
    private readonly ListeningHistoryBUS _bus;
    private readonly GuestAccessBUS _access;
    public PublicListeningHistoriesController(ListeningHistoryBUS bus, GuestAccessBUS access)
    {
        _bus = bus;
        _access = access;
    }
    [HttpPost]
    public IActionResult Create([FromBody] ListeningHistoryDTO dto)
    {
        try
        {
            var pass = _access.EnsureActive(dto.GuestSessionId ?? string.Empty);
            dto.AccessPassId = pass.AccessPassId;
            return CreatedData(_bus.Create(dto));
        }
        catch (UnauthorizedAccessException ex) { return PaymentRequired(ex.Message); }
        catch (Exception ex) { return BadRequestException(ex); }
    }
    [HttpPatch("{historyId:long}/status")]
    public IActionResult UpdateStatus(long historyId, [FromBody] UpdatePlaybackStatusRequestDTO request)
    {
        try
        {
            _access.EnsureActive(request.GuestSessionId);
            return OkData(_bus.UpdatePlaybackStatus(
                historyId, request.GuestSessionId, request.Status));
        }
        catch (UnauthorizedAccessException ex) { return PaymentRequired(ex.Message); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpPatch("{historyId:long}/duration")]
    public IActionResult UpdateDuration(long historyId, [FromBody] UpdateListenDurationRequestDTO request)
    {
        try
        {
            _access.EnsureActive(request.GuestSessionId);
            return OkData(_bus.UpdateListenDuration(
                historyId, request.GuestSessionId, request.Seconds));
        }
        catch (UnauthorizedAccessException ex) { return PaymentRequired(ex.Message); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    private IActionResult PaymentRequired(string message) =>
        StatusCode(StatusCodes.Status402PaymentRequired, new
        {
            success = false,
            message,
            data = (object?)null
        });
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/listening-histories")]
public class ListeningHistoriesController : BaseApiController
{
    private readonly ListeningHistoryBUS _bus;
    public ListeningHistoriesController(ListeningHistoryBUS bus) => _bus = bus;
    [HttpGet]
public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    => OkData(_bus.GetPaged(page, pageSize));
    [HttpGet("{historyId:long}")] public IActionResult GetById(long historyId) => OkData(_bus.GetById(historyId));
    [HttpGet("session/{guestSessionId}")] public IActionResult GetBySession(string guestSessionId) => OkData(_bus.GetByGuestSessionId(guestSessionId));
    [HttpGet("narration/{narrationId:long}")] public IActionResult GetByNarration(long narrationId) => OkData(_bus.GetByNarrationId(narrationId));
    [HttpGet("date-range")] public IActionResult GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to) => OkData(_bus.GetByDateRange(from, to));
}

[Route("api/public/feedbacks")]
public class PublicFeedbacksController : BaseApiController
{
    private readonly FeedbackBUS _bus;
    public PublicFeedbacksController(FeedbackBUS bus) => _bus = bus;
    [HttpPost] public IActionResult Create([FromBody] FeedbackDTO dto)
    {
        try { return CreatedData(_bus.CreateFeedback(dto)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

[Authorize(Roles = "Admin,ContentManager,Reviewer")]
[Route("api/admin/feedbacks")]
public class FeedbacksController : BaseApiController
{
    private readonly FeedbackBUS _bus;
    public FeedbacksController(FeedbackBUS bus) => _bus = bus;
    [HttpGet]
public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    => OkData(_bus.GetPaged(page, pageSize));
    [HttpGet("approved")] public IActionResult GetApproved() => OkData(_bus.GetApproved());
    [HttpGet("pending")] public IActionResult GetPending() => OkData(_bus.GetPending());
    [HttpGet("{feedbackId:long}")] public IActionResult GetById(long feedbackId) => OkData(_bus.GetById(feedbackId));
    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));
    [HttpGet("dish/{dishId:long}")] public IActionResult GetByDish(long dishId) => OkData(_bus.GetByDishId(dishId));
    [HttpGet("narration/{narrationId:long}")] public IActionResult GetByNarration(long narrationId) => OkData(_bus.GetByNarrationId(narrationId));
    [HttpPatch("{feedbackId:long}/approve")] public IActionResult Approve(long feedbackId) => OkData(_bus.Approve(feedbackId));
    [HttpPatch("{feedbackId:long}/reject")] public IActionResult Reject(long feedbackId) => OkData(_bus.Reject(feedbackId));
}
