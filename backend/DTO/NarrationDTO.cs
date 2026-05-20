using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("narration_contents")]
public class NarrationContentDTO
{
    [DbColumn("narration_id", IsKey = true, IsIdentity = true)] public long NarrationId { get; set; }
    [DbColumn("title")] public string Title { get; set; } = string.Empty;
    [DbColumn("original_text")] public string OriginalText { get; set; } = string.Empty;
    [DbColumn("content_type_id")] public long ContentTypeId { get; set; }
    [DbColumn("place_id")] public long? PlaceId { get; set; }
    [DbColumn("dish_id")] public long? DishId { get; set; }
    [DbColumn("created_by")] public long CreatedBy { get; set; }
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
    [DbColumn("reviewed_by")] public long? ReviewedBy { get; set; }
    [DbColumn("is_reviewed")] public bool IsReviewed { get; set; }
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("audio_files")]
public class AudioFileDTO
{
    [DbColumn("audio_id", IsKey = true, IsIdentity = true)] public long AudioId { get; set; }
    [DbColumn("translation_id")] public long TranslationId { get; set; }
    [DbColumn("audio_url")] public string AudioUrl { get; set; } = string.Empty;
    [DbColumn("voice_name")] public string? VoiceName { get; set; }
    [DbColumn("voice_gender")] public string? VoiceGender { get; set; }
    [DbColumn("duration_seconds")] public int? DurationSeconds { get; set; }
    [DbColumn("file_format")] public string FileFormat { get; set; } = "mp3";
    [DbColumn("generated_by")] public string GeneratedBy { get; set; } = "TTS";
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
