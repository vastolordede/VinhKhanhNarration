using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/qr-codes")]
public class QRCodesController : CrudControllerBase<QRCodeDTO>
{
    private readonly QRCodeBUS _bus;
    public QRCodesController(QRCodeBUS bus) : base(bus) => _bus = bus;

    [HttpGet("value/{value}")] public IActionResult GetByValue(string value) => OkData(_bus.GetByValue(value));
}

[Route("api/public/qr")]
public class PublicQRController : BaseApiController
{
    private readonly QRCodeBUS _bus;
    public PublicQRController(QRCodeBUS bus) => _bus = bus;

    [HttpPost("resolve")]
    public IActionResult Resolve([FromBody] QRScanRequestDTO request)
    {
        try { return OkData(_bus.ResolveQRCode(request.QRCodeValue, request.LanguageId, request.GuestSessionId)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

[Route("api/public/guest-sessions")]
public class PublicGuestSessionsController : BaseApiController
{
    private readonly GuestSessionBUS _bus;
    public PublicGuestSessionsController(GuestSessionBUS bus) => _bus = bus;

    [HttpPost]
    public IActionResult Create([FromBody] CreateGuestSessionRequestDTO request)
        => CreatedData(_bus.CreateSession(request.DeviceInfo, request.IPAddress, request.PreferredLanguageId));

    [HttpPatch("{guestSessionId}/language")]
    public IActionResult ChangeLanguage(string guestSessionId, [FromBody] ChangeGuestLanguageRequestDTO request)
        => OkData(_bus.ChangePreferredLanguage(guestSessionId, request.LanguageId));

    [HttpPatch("{guestSessionId}/touch")]
    public IActionResult Touch(string guestSessionId) => OkData(_bus.Touch(guestSessionId));
}

[Route("api/admin/guest-sessions")]
public class AdminGuestSessionsController : BaseApiController
{
    private readonly GuestSessionBUS _bus;
    public AdminGuestSessionsController(GuestSessionBUS bus) => _bus = bus;
    [HttpGet] public IActionResult GetActive() => OkData(_bus.GetActiveSessions());
    [HttpGet("{guestSessionId}")] public IActionResult GetById(string guestSessionId) => OkData(_bus.GetById(guestSessionId));
    [HttpPatch("{guestSessionId}/deactivate")] public IActionResult Deactivate(string guestSessionId) => OkData(_bus.Deactivate(guestSessionId));
}

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
    public PublicGeofenceController(GeofenceBUS bus) => _bus = bus;

    [HttpPost("check")]
    public IActionResult Check([FromBody] GeofenceCheckRequestDTO request)
    {
        try { return OkData(_bus.CheckLocation(request.GuestSessionId, request.Latitude, request.Longitude, request.LanguageId)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }
}

public class UpdateGeofenceStatusRequestDTO { public long EventStatusId { get; set; } }

[Route("api/admin/geofence-events")]
public class GeofenceEventsController : BaseApiController
{
    private readonly GeofenceBUS _bus;
    public GeofenceEventsController(GeofenceBUS bus) => _bus = bus;
    [HttpGet] public IActionResult GetAll() => OkData(_bus.GetAll());
    [HttpGet("{eventId:long}")] public IActionResult GetById(long eventId) => OkData(_bus.GetById(eventId));
    [HttpGet("session/{guestSessionId}")] public IActionResult GetBySession(string guestSessionId) => OkData(_bus.GetByGuestSessionId(guestSessionId));
    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));
    [HttpGet("date-range")] public IActionResult GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to) => OkData(_bus.GetByDateRange(from, to));
    [HttpPatch("{eventId:long}/status")] public IActionResult UpdateStatus(long eventId, [FromBody] UpdateGeofenceStatusRequestDTO request) => OkData(_bus.UpdateStatus(eventId, request.EventStatusId));
}

public class UpdatePlaybackStatusRequestDTO { public string Status { get; set; } = string.Empty; }
public class UpdateListenDurationRequestDTO { public int Seconds { get; set; } }

[Route("api/public/listening-histories")]
public class PublicListeningHistoriesController : BaseApiController
{
    private readonly ListeningHistoryBUS _bus;
    public PublicListeningHistoriesController(ListeningHistoryBUS bus) => _bus = bus;
    [HttpPost] public IActionResult Create([FromBody] ListeningHistoryDTO dto) => CreatedData(_bus.Create(dto));
    [HttpPatch("{historyId:long}/status")] public IActionResult UpdateStatus(long historyId, [FromBody] UpdatePlaybackStatusRequestDTO request) => OkData(_bus.UpdatePlaybackStatus(historyId, request.Status));
    [HttpPatch("{historyId:long}/duration")] public IActionResult UpdateDuration(long historyId, [FromBody] UpdateListenDurationRequestDTO request) => OkData(_bus.UpdateListenDuration(historyId, request.Seconds));
}

[Route("api/admin/listening-histories")]
public class ListeningHistoriesController : BaseApiController
{
    private readonly ListeningHistoryBUS _bus;
    public ListeningHistoriesController(ListeningHistoryBUS bus) => _bus = bus;
    [HttpGet] public IActionResult GetAll() => OkData(_bus.GetAll());
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

[Route("api/admin/feedbacks")]
public class FeedbacksController : BaseApiController
{
    private readonly FeedbackBUS _bus;
    public FeedbacksController(FeedbackBUS bus) => _bus = bus;
    [HttpGet] public IActionResult GetAll() => OkData(_bus.GetAll());
    [HttpGet("approved")] public IActionResult GetApproved() => OkData(_bus.GetApproved());
    [HttpGet("pending")] public IActionResult GetPending() => OkData(_bus.GetPending());
    [HttpGet("{feedbackId:long}")] public IActionResult GetById(long feedbackId) => OkData(_bus.GetById(feedbackId));
    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetByPlaceId(placeId));
    [HttpGet("dish/{dishId:long}")] public IActionResult GetByDish(long dishId) => OkData(_bus.GetByDishId(dishId));
    [HttpGet("narration/{narrationId:long}")] public IActionResult GetByNarration(long narrationId) => OkData(_bus.GetByNarrationId(narrationId));
    [HttpPatch("{feedbackId:long}/approve")] public IActionResult Approve(long feedbackId) => OkData(_bus.Approve(feedbackId));
    [HttpPatch("{feedbackId:long}/reject")] public IActionResult Reject(long feedbackId) => OkData(_bus.Reject(feedbackId));
}
