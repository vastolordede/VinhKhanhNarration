using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("qr_codes")]
public class QRCodeDTO
{
    [DbColumn("qr_code_id", IsKey = true, IsIdentity = true)] public long QRCodeId { get; set; }
    [DbColumn("qr_code_value")] public string QRCodeValue { get; set; } = string.Empty;
    [DbColumn("qr_code_image_url")] public string? QRCodeImageUrl { get; set; }
    [DbColumn("target_type_id")] public long TargetTypeId { get; set; }
    [DbColumn("place_id")] public long? PlaceId { get; set; }
    [DbColumn("dish_id")] public long? DishId { get; set; }
    [DbColumn("narration_id")] public long? NarrationId { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

public class QRScanRequestDTO
{
    public string QRCodeValue { get; set; } = string.Empty;
    public long LanguageId { get; set; }
    public string GuestSessionId { get; set; } = string.Empty;
}

public class QRScanResultDTO
{
    public long? PlaceId { get; set; }
    public long? DishId { get; set; }
    public long NarrationId { get; set; }
    public long TranslationId { get; set; }
    public long AudioId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
}

public class GuestSessionDTO
{
    public string GuestSessionId { get; set; } = string.Empty;
    public long? PreferredLanguageId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateGuestSessionRequestDTO
{
    public long? PreferredLanguageId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IPAddress { get; set; }
}

public class ChangeGuestLanguageRequestDTO
{
    public long LanguageId { get; set; }
}

public class GuestPoiStateDTO
{
    public string GuestSessionId { get; set; } = string.Empty;
    public long PlaceId { get; set; }
    public bool IsInside { get; set; }
    public DateTime? LastEnteredAt { get; set; }
    public DateTime? LastExitedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime? CooldownUntil { get; set; }
    public decimal? LastDistanceMeters { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GeofenceEventDTO
{
    public long EventId { get; set; }
    public string GuestSessionId { get; set; } = string.Empty;
    public long PlaceId { get; set; }
    public long? NarrationId { get; set; }
    public long EventTypeId { get; set; }
    public long EventStatusId { get; set; }
    public decimal? UserLatitude { get; set; }
    public decimal? UserLongitude { get; set; }
    public decimal? DistanceMeters { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Note { get; set; }
}

public class GeofenceCheckRequestDTO
{
    public string GuestSessionId { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public long LanguageId { get; set; }
}

public class GeofenceCheckResultDTO
{
    public bool ShouldPlay { get; set; }
    public string Reason { get; set; } = string.Empty;
    public long? PlaceId { get; set; }
    public long? NarrationId { get; set; }
    public long? TranslationId { get; set; }
    public long? AudioId { get; set; }
    public string? AudioUrl { get; set; }
    public decimal? DistanceMeters { get; set; }
}

[DbTable("listening_histories")]
public class ListeningHistoryDTO
{
    [DbColumn("history_id", IsKey = true, IsIdentity = true)] public long HistoryId { get; set; }
    [DbColumn("guest_session_id")] public string? GuestSessionId { get; set; }
    [DbColumn("narration_id")] public long NarrationId { get; set; }
    [DbColumn("language_id")] public long LanguageId { get; set; }
    [DbColumn("audio_id")] public long AudioId { get; set; }
    [DbColumn("qr_code_id")] public long? QRCodeId { get; set; }
    [DbColumn("geofence_event_id")] public long? GeofenceEventId { get; set; }
    [DbColumn("trigger_source")] public string TriggerSource { get; set; } = "Manual";
    [DbColumn("playback_status")] public string PlaybackStatus { get; set; } = "Played";
    [DbColumn("listened_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime ListenedAt { get; set; }
    [DbColumn("device_info")] public string? DeviceInfo { get; set; }
    [DbColumn("ip_address")] public string? IPAddress { get; set; }
    [DbColumn("listen_duration_seconds")] public int? ListenDurationSeconds { get; set; }
}

[DbTable("feedbacks")]
public class FeedbackDTO
{
    [DbColumn("feedback_id", IsKey = true, IsIdentity = true)] public long FeedbackId { get; set; }
    [DbColumn("guest_session_id")] public string? GuestSessionId { get; set; }
    [DbColumn("place_id")] public long? PlaceId { get; set; }
    [DbColumn("dish_id")] public long? DishId { get; set; }
    [DbColumn("narration_id")] public long? NarrationId { get; set; }
    [DbColumn("rating")] public int Rating { get; set; }
    [DbColumn("comment")] public string? Comment { get; set; }
    [DbColumn("is_approved")] public bool IsApproved { get; set; }
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
