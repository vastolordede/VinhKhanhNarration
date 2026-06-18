using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

public static class NarrationWorkflowStatuses
{
    public const string Draft = "Draft";
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Published = "Published";
    public const string HiddenByVendor = "HiddenByVendor";
    public const string HiddenByAdmin = "HiddenByAdmin";
    public const string DeletedByVendor = "DeletedByVendor";
    public const string DeletedByAdmin = "DeletedByAdmin";
}

public static class TranslationStatuses
{
    public const string Pending = "Pending";
    public const string Generating = "Generating";
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Failed = "Failed";
    public const string Outdated = "Outdated";
}

public static class AudioStatuses
{
    public const string Pending = "Pending";
    public const string Generating = "Generating";
    public const string Ready = "Ready";
    public const string Failed = "Failed";
    public const string Outdated = "Outdated";
}

[DbTable("narration_contents")]
public class NarrationContentDTO
{
    [DbColumn("narration_id", IsKey = true, IsIdentity = true)] public long NarrationId { get; set; }
    [DbColumn("title")] public string Title { get; set; } = string.Empty;
    [DbColumn("original_text")] public string OriginalText { get; set; } = string.Empty;
    [DbColumn("content_type_id")] public long ContentTypeId { get; set; }
    [DbColumn("place_id")] public long? PlaceId { get; set; }
    [DbColumn("dish_id")] public long? DishId { get; set; }
    [DbColumn("source_language_id")] public long? SourceLanguageId { get; set; }
    [DbColumn("created_by")] public long? CreatedBy { get; set; }
    [DbColumn("submitted_by_vendor_id")] public long? SubmittedByVendorId { get; set; }
    [DbColumn("workflow_status")] public string WorkflowStatus { get; set; } = NarrationWorkflowStatuses.Draft;
    [DbColumn("rejection_reason")] public string? RejectionReason { get; set; }
    [DbColumn("submitted_at")] public DateTime? SubmittedAt { get; set; }
    [DbColumn("reviewed_by")] public long? ReviewedBy { get; set; }
    [DbColumn("reviewed_at")] public DateTime? ReviewedAt { get; set; }
    [DbColumn("published_at")] public DateTime? PublishedAt { get; set; }
    [DbColumn("previous_workflow_status")] public string? PreviousWorkflowStatus { get; set; }
    [DbColumn("moderation_reason")] public string? ModerationReason { get; set; }
    [DbColumn("moderation_by_admin_id")] public long? ModerationByAdminId { get; set; }
    [DbColumn("hidden_at")] public DateTime? HiddenAt { get; set; }
    [DbColumn("deleted_at")] public DateTime? DeletedAt { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("narration_translations")]
public class NarrationTranslationDTO
{
    [DbColumn("translation_id", IsKey = true, IsIdentity = true)] public long TranslationId { get; set; }
    [DbColumn("narration_id")] public long NarrationId { get; set; }
    [DbColumn("language_id")] public long LanguageId { get; set; }
    [DbColumn("translated_title")] public string TranslatedTitle { get; set; } = string.Empty;
    [DbColumn("translated_text")] public string TranslatedText { get; set; } = string.Empty;
    [DbColumn("translation_source_id")] public long TranslationSourceId { get; set; }
    [DbColumn("provider")] public string? Provider { get; set; }
    [DbColumn("status")] public string Status { get; set; } = TranslationStatuses.PendingReview;
    [DbColumn("error_message")] public string? ErrorMessage { get; set; }
    [DbColumn("reviewed_by")] public long? ReviewedBy { get; set; }
    [DbColumn("reviewed_at")] public DateTime? ReviewedAt { get; set; }
    [DbColumn("is_reviewed")] public bool IsReviewed { get; set; }
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("audio_files")]
public class AudioFileDTO
{
    [DbColumn("audio_id", IsKey = true, IsIdentity = true)] public long AudioId { get; set; }
    [DbColumn("translation_id")] public long TranslationId { get; set; }
    [DbColumn("audio_url")] public string? AudioUrl { get; set; }
    [DbColumn("provider")] public string? Provider { get; set; }
    [DbColumn("voice_name")] public string? VoiceName { get; set; }
    [DbColumn("voice_gender")] public string? VoiceGender { get; set; }
    [DbColumn("duration_seconds")] public int? DurationSeconds { get; set; }
    [DbColumn("file_format")] public string FileFormat { get; set; } = "mp3";
    [DbColumn("generated_by")] public string GeneratedBy { get; set; } = "TTS";
    [DbColumn("status")] public string Status { get; set; } = AudioStatuses.Pending;
    [DbColumn("error_message")] public string? ErrorMessage { get; set; }
    [DbColumn("source_text_hash")] public string? SourceTextHash { get; set; }
    [DbColumn("generated_at")] public DateTime? GeneratedAt { get; set; }
    [DbColumn("published_at")] public DateTime? PublishedAt { get; set; }
    [DbColumn("previous_workflow_status")] public string? PreviousWorkflowStatus { get; set; }
    [DbColumn("moderation_reason")] public string? ModerationReason { get; set; }
    [DbColumn("moderation_by_admin_id")] public long? ModerationByAdminId { get; set; }
    [DbColumn("hidden_at")] public DateTime? HiddenAt { get; set; }
    [DbColumn("deleted_at")] public DateTime? DeletedAt { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

public class GenerateTranslationsRequestDTO
{
    public long AdminId { get; set; }
    public string? SourceLanguageCode { get; set; }
}

public class GenerateTranslationsResultDTO
{
    public long NarrationId { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ReviewNarrationRequestDTO
{
    public long AdminId { get; set; }
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; }
}

public class AutoProcessNarrationRequestDTO
{
    public long AdminId { get; set; }
}

public class AutoProcessNarrationResultDTO
{
    public long NarrationId { get; set; }
    public int TranslationsCreated { get; set; }
    public int TranslationsUpdated { get; set; }
    public int AudioTargetCount { get; set; }
    public int AudioReady { get; set; }
    public bool Published { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ReviewNarrationResultDTO
{
    public long NarrationId { get; set; }
    public bool Approved { get; set; }
    public AutoProcessNarrationResultDTO? Processing { get; set; }
}

public class ReviewTranslationRequestDTO
{
    public long AdminId { get; set; }
    public bool Approved { get; set; }
    public string? RejectionReason { get; set; }
}

public class GenerateAudioRequestDTO
{
    public long AdminId { get; set; }
    public string? VoiceName { get; set; }
    public bool PublishAfterGenerate { get; set; } = false;
}

public class PublishNarrationRequestDTO
{
    public long AdminId { get; set; }
}

public class VendorNarrationRequestDTO
{
    public long VendorUserId { get; set; }
    public long SourceLanguageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public long ContentTypeId { get; set; }
    public long? PlaceId { get; set; }
    public long? DishId { get; set; }
}

public class PublicNarrationResultDTO
{
    public long? PlaceId { get; set; }
    public long? DishId { get; set; }
    public long NarrationId { get; set; }
    public long TranslationId { get; set; }
    public long AudioId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
}

public class VendorNarrationStatusDTO
{
    public NarrationContentDTO Narration { get; set; } = new();
    public List<NarrationTranslationDTO> Translations { get; set; } = new();
    public List<AudioFileDTO> AudioFiles { get; set; } = new();
}
