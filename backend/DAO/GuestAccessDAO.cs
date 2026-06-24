using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class GuestAccessDAO : BaseDAO
{
    public GuestAccessDAO(DbConnectionFactory factory) : base(factory) { }

    public bool GuestSessionExists(string guestSessionId, bool activeOnly = true)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM guest_sessions
            WHERE guest_session_id = @id
              AND (@activeOnly = FALSE OR is_active = TRUE);", conn);
        cmd.Parameters.AddWithValue("@id", guestSessionId);
        cmd.Parameters.AddWithValue("@activeOnly", activeOnly);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public GuestPaymentOrderDTO? GetPaymentByOrderCode(string orderCode)
    {
        ExpirePendingPayments();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT guest_payment_order_id, guest_session_id, order_code, amount,
                   status, provider, payment_url, preferred_language_id,
                   device_info, ip_address, created_at, expires_at, paid_at
            FROM guest_payment_orders
            WHERE order_code = @orderCode
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@orderCode", orderCode);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPayment(reader) : null;
    }

    public long InsertPayment(GuestPaymentOrderDTO order)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO guest_payment_orders
            (guest_session_id, order_code, amount, status, provider,
             payment_url, preferred_language_id, device_info, ip_address, expires_at)
            VALUES
            (NULL, @orderCode, @amount, @status, @provider,
             @paymentUrl, @preferredLanguageId, @deviceInfo, @ipAddress, @expiresAt)
            RETURNING guest_payment_order_id;", conn);
        cmd.Parameters.AddWithValue("@orderCode", order.OrderCode);
        cmd.Parameters.AddWithValue("@amount", order.Amount);
        cmd.Parameters.AddWithValue("@status", order.Status);
        cmd.Parameters.AddWithValue("@provider", order.Provider);
        cmd.Parameters.AddWithValue("@paymentUrl", order.PaymentUrl);
        cmd.Parameters.AddWithValue("@preferredLanguageId", DbValue(order.PreferredLanguageId));
        cmd.Parameters.AddWithValue("@deviceInfo", DbValue(order.DeviceInfo));
        cmd.Parameters.AddWithValue("@ipAddress", DbValue(order.IPAddress));
        cmd.Parameters.AddWithValue("@expiresAt", order.ExpiresAt);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public string? ConfirmPaymentAndCreateSession(
        long paymentId,
        string newGuestSessionId,
        int durationHours)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        string status;
        DateTime orderExpiresAt;
        decimal amount;
        string? existingSessionId;
        long? preferredLanguageId;
        string? deviceInfo;
        string? ipAddress;

        using (var select = new NpgsqlCommand(@"
            SELECT status, expires_at, amount, guest_session_id,
                   preferred_language_id, device_info, ip_address
            FROM guest_payment_orders
            WHERE guest_payment_order_id = @paymentId
            FOR UPDATE;", conn, tx))
        {
            select.Parameters.AddWithValue("@paymentId", paymentId);
            using var reader = select.ExecuteReader();
            if (!reader.Read()) return null;

            status = reader.GetString(0);
            orderExpiresAt = reader.GetDateTime(1);
            amount = reader.GetDecimal(2);
            existingSessionId = reader.IsDBNull(3) ? null : reader.GetString(3);
            preferredLanguageId = reader.IsDBNull(4) ? null : reader.GetInt64(4);
            deviceInfo = reader.IsDBNull(5) ? null : reader.GetString(5);
            ipAddress = reader.IsDBNull(6) ? null : reader.GetString(6);
        }

        if (status == GuestPaymentStatuses.Paid && !string.IsNullOrWhiteSpace(existingSessionId))
        {
            tx.Commit();
            return existingSessionId;
        }

        if (status != GuestPaymentStatuses.Pending || orderExpiresAt <= DateTime.UtcNow)
            return null;

        var startsAt = DateTime.UtcNow;
        var accessExpiresAt = startsAt.AddHours(durationHours);

        using (var createSession = new NpgsqlCommand(@"
            INSERT INTO guest_sessions
            (guest_session_id, preferred_language_id, device_info, ip_address,
             is_active, guest_payment_order_id, access_price,
             access_started_at, access_expires_at)
            VALUES
            (@guestSessionId, @preferredLanguageId, @deviceInfo, @ipAddress,
             TRUE, @paymentId, @amount, @startsAt, @accessExpiresAt);", conn, tx))
        {
            createSession.Parameters.AddWithValue("@guestSessionId", newGuestSessionId);
            createSession.Parameters.AddWithValue("@preferredLanguageId", DbValue(preferredLanguageId));
            createSession.Parameters.AddWithValue("@deviceInfo", DbValue(deviceInfo));
            createSession.Parameters.AddWithValue("@ipAddress", DbValue(ipAddress));
            createSession.Parameters.AddWithValue("@paymentId", paymentId);
            createSession.Parameters.AddWithValue("@amount", amount);
            createSession.Parameters.AddWithValue("@startsAt", startsAt);
            createSession.Parameters.AddWithValue("@accessExpiresAt", accessExpiresAt);
            createSession.ExecuteNonQuery();
        }

        using (var pay = new NpgsqlCommand(@"
            UPDATE guest_payment_orders
            SET status = 'Paid',
                paid_at = @startsAt,
                guest_session_id = @guestSessionId
            WHERE guest_payment_order_id = @paymentId
              AND status = 'Pending';", conn, tx))
        {
            pay.Parameters.AddWithValue("@paymentId", paymentId);
            pay.Parameters.AddWithValue("@startsAt", startsAt);
            pay.Parameters.AddWithValue("@guestSessionId", newGuestSessionId);
            if (pay.ExecuteNonQuery() == 0) return null;
        }

        using (var createPass = new NpgsqlCommand(@"
            INSERT INTO guest_access_passes
            (guest_session_id, guest_payment_order_id, starts_at, expires_at, status)
            VALUES
            (@guestSessionId, @paymentId, @startsAt, @accessExpiresAt, 'Active');", conn, tx))
        {
            createPass.Parameters.AddWithValue("@guestSessionId", newGuestSessionId);
            createPass.Parameters.AddWithValue("@paymentId", paymentId);
            createPass.Parameters.AddWithValue("@startsAt", startsAt);
            createPass.Parameters.AddWithValue("@accessExpiresAt", accessExpiresAt);
            createPass.ExecuteNonQuery();
        }

        tx.Commit();
        return newGuestSessionId;
    }

    public GuestAccessPassDTO? GetActivePass(string guestSessionId)
    {
        ExpireAccessPassesAndSessions();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT access_pass_id, guest_session_id, guest_payment_order_id,
                   starts_at, expires_at, status, created_at, updated_at
            FROM guest_access_passes
            WHERE guest_session_id = @guestSessionId
              AND status = 'Active'
              AND expires_at > CURRENT_TIMESTAMP
            ORDER BY access_pass_id DESC
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@guestSessionId", guestSessionId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPass(reader) : null;
    }

    public int ExpirePendingPayments()
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE guest_payment_orders
            SET status = 'Expired'
            WHERE status = 'Pending'
              AND expires_at <= CURRENT_TIMESTAMP;", conn);
        return cmd.ExecuteNonQuery();
    }

    public int ExpireAccessPassesAndSessions()
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        int passCount;
        using (var expirePasses = new NpgsqlCommand(@"
            UPDATE guest_access_passes
            SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
            WHERE status = 'Active'
              AND expires_at <= CURRENT_TIMESTAMP;", conn, tx))
        {
            passCount = expirePasses.ExecuteNonQuery();
        }

        int sessionCount;
        using (var deactivateSessions = new NpgsqlCommand(@"
            UPDATE guest_sessions
            SET is_active = FALSE,
                deactivated_at = COALESCE(deactivated_at, CURRENT_TIMESTAMP),
                last_seen_at = CURRENT_TIMESTAMP
            WHERE is_active = TRUE
              AND access_expires_at IS NOT NULL
              AND access_expires_at <= CURRENT_TIMESTAMP;", conn, tx))
        {
            sessionCount = deactivateSessions.ExecuteNonQuery();
        }

        tx.Commit();
        return passCount + sessionCount;
    }

    private static GuestPaymentOrderDTO MapPayment(NpgsqlDataReader reader) => new()
    {
        GuestPaymentOrderId = reader.GetInt64(0),
        GuestSessionId = reader.IsDBNull(1) ? null : reader.GetString(1),
        OrderCode = reader.GetString(2),
        Amount = reader.GetDecimal(3),
        Status = reader.GetString(4),
        Provider = reader.GetString(5),
        PaymentUrl = reader.GetString(6),
        PreferredLanguageId = reader.IsDBNull(7) ? null : reader.GetInt64(7),
        DeviceInfo = reader.IsDBNull(8) ? null : reader.GetString(8),
        IPAddress = reader.IsDBNull(9) ? null : reader.GetString(9),
        CreatedAt = reader.GetDateTime(10),
        ExpiresAt = reader.GetDateTime(11),
        PaidAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
    };

    private static GuestAccessPassDTO MapPass(NpgsqlDataReader reader) => new()
    {
        AccessPassId = reader.GetInt64(0),
        GuestSessionId = reader.GetString(1),
        GuestPaymentOrderId = reader.GetInt64(2),
        StartsAt = reader.GetDateTime(3),
        ExpiresAt = reader.GetDateTime(4),
        Status = reader.GetString(5),
        CreatedAt = reader.GetDateTime(6),
        UpdatedAt = reader.GetDateTime(7)
    };
}
