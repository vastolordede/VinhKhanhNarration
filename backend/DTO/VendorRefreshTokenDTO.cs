using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("vendor_refresh_tokens")]
public class VendorRefreshTokenDTO
{
    [DbColumn("refresh_token_id", IsKey = true, IsIdentity = true)]
    public long RefreshTokenId { get; set; }

    [DbColumn("vendor_user_id")]
    public long VendorUserId { get; set; }

    [DbColumn("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [DbColumn("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [DbColumn("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime CreatedAt { get; set; }

    [DbColumn("created_by_ip")]
    public string? CreatedByIp { get; set; }

    [DbColumn("revoked_by_ip")]
    public string? RevokedByIp { get; set; }

    [DbColumn("replaced_by_token_hash")]
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsExpired && !IsRevoked;
}
