using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class VendorRefreshTokenDAO : GenericCrudDAO<VendorRefreshTokenDTO>
{
    public VendorRefreshTokenDAO(DbConnectionFactory factory) : base(factory) { }

    public VendorRefreshTokenDTO? GetByTokenHash(string tokenHash) =>
        QuerySingle(
            "SELECT * FROM vendor_refresh_tokens WHERE token_hash = @hash LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@hash", tokenHash));

    public void RevokeToken(
        string tokenHash,
        string? revokedByIp,
        string? replacedByTokenHash = null)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_refresh_tokens
            SET revoked_at = CURRENT_TIMESTAMP,
                revoked_by_ip = @ip,
                replaced_by_token_hash = @replaced
            WHERE token_hash = @hash
              AND revoked_at IS NULL;", conn);
        cmd.Parameters.AddWithValue("@hash", tokenHash);
        cmd.Parameters.AddWithValue("@ip", DbValue(revokedByIp));
        cmd.Parameters.AddWithValue("@replaced", DbValue(replacedByTokenHash));
        cmd.ExecuteNonQuery();
    }

    public void RevokeAllActiveTokens(long vendorUserId, string? revokedByIp)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_refresh_tokens
            SET revoked_at = CURRENT_TIMESTAMP,
                revoked_by_ip = @ip
            WHERE vendor_user_id = @vendorUserId
              AND revoked_at IS NULL
              AND expires_at > CURRENT_TIMESTAMP;", conn);
        cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
        cmd.Parameters.AddWithValue("@ip", DbValue(revokedByIp));
        cmd.ExecuteNonQuery();
    }

    public int DeleteExpiredAndRevoked(DateTime cutoffUtc)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            DELETE FROM vendor_refresh_tokens
            WHERE expires_at < @cutoff
               OR (revoked_at IS NOT NULL AND revoked_at < @cutoff);", conn);
        cmd.Parameters.AddWithValue("@cutoff", cutoffUtc);
        return cmd.ExecuteNonQuery();
    }
}
