using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("admin_users")]
public class AdminUserDTO
{
    [DbColumn("admin_id", IsKey = true, IsIdentity = true)] public long AdminId { get; set; }
    [DbColumn("full_name")] public string FullName { get; set; } = string.Empty;
    [DbColumn("email")] public string Email { get; set; } = string.Empty;
    [DbColumn("password_hash")] public string PasswordHash { get; set; } = string.Empty;
    [DbColumn("role")] public string Role { get; set; } = string.Empty;
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
