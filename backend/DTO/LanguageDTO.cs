using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("languages")]
public class LanguageDTO
{
    [DbColumn("language_id", IsKey = true, IsIdentity = true)] public long LanguageId { get; set; }
    [DbColumn("language_code")] public string LanguageCode { get; set; } = string.Empty;
    [DbColumn("language_name")] public string LanguageName { get; set; } = string.Empty;
    [DbColumn("locale")] public string? Locale { get; set; }
    [DbColumn("native_name")] public string? NativeName { get; set; }
    [DbColumn("is_default")] public bool IsDefault { get; set; }
    [DbColumn("is_ui_enabled")] public bool IsUiEnabled { get; set; } = true;
    [DbColumn("is_content_enabled")] public bool IsContentEnabled { get; set; } = true;
    [DbColumn("is_translation_supported")] public bool IsTranslationSupported { get; set; } = true;
    [DbColumn("is_tts_supported")] public bool IsTtsSupported { get; set; } = true;
    [DbColumn("default_voice_id")] public string? DefaultVoiceId { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
