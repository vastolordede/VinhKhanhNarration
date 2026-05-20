using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("languages")]
public class LanguageDTO
{
    [DbColumn("language_id", IsKey = true, IsIdentity = true)] public long LanguageId { get; set; }
    [DbColumn("language_code")] public string LanguageCode { get; set; } = string.Empty;
    [DbColumn("language_name")] public string LanguageName { get; set; } = string.Empty;
    [DbColumn("is_default")] public bool IsDefault { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
