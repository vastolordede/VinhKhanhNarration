using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class AdminDashboardDAO : BaseDAO
{
    public AdminDashboardDAO(DbConnectionFactory factory) : base(factory) { }

    public AdminDashboardStatisticsDTO GetStatistics(
        string period,
        DateTime from,
        DateTime to,
        string granularity)
    {
        using var conn = CreateConnection();
        conn.Open();

        var result = new AdminDashboardStatisticsDTO
        {
            Period = period,
            From = from,
            To = to
        };

        using (var summary = new NpgsqlCommand(@"
            SELECT
                (SELECT COUNT(*) FROM places),
                (SELECT COUNT(*) FROM dishes),
                (SELECT COUNT(*) FROM narration_contents),
                (SELECT COUNT(*) FROM vendor_users),
                (SELECT COUNT(*) FROM feedbacks WHERE created_at >= @from AND created_at < @to),
                (SELECT COUNT(*) FROM listening_histories WHERE listened_at >= @from AND listened_at < @to),
                (SELECT COUNT(*) FROM geofence_events WHERE detected_at >= @from AND detected_at < @to),
                (SELECT COUNT(*) FROM guest_sessions
                    WHERE guest_payment_order_id IS NOT NULL
                      AND access_started_at >= @from AND access_started_at < @to),
                (SELECT COUNT(*) FROM payment_orders
                    WHERE status = 'Paid' AND paid_at >= @from AND paid_at < @to),
                (SELECT COALESCE(SUM(access_price), 0) FROM guest_sessions
                    WHERE guest_payment_order_id IS NOT NULL
                      AND access_started_at >= @from AND access_started_at < @to),
                (SELECT COALESCE(SUM(amount), 0) FROM payment_orders
                    WHERE status = 'Paid' AND paid_at >= @from AND paid_at < @to);", conn))
        {
            summary.Parameters.AddWithValue("@from", from);
            summary.Parameters.AddWithValue("@to", to);
            using var reader = summary.ExecuteReader();
            if (reader.Read())
            {
                result.Places = reader.GetInt64(0);
                result.Dishes = reader.GetInt64(1);
                result.Narrations = reader.GetInt64(2);
                result.Vendors = reader.GetInt64(3);
                result.Feedbacks = reader.GetInt64(4);
                result.Listening = reader.GetInt64(5);
                result.Geofence = reader.GetInt64(6);
                result.GuestSessions = reader.GetInt64(7);
                result.VendorPayments = reader.GetInt64(8);
                result.GuestRevenue = reader.GetDecimal(9);
                result.VendorRevenue = reader.GetDecimal(10);
            }
        }

        using (var activity = new NpgsqlCommand(@"
            WITH activity_rows AS (
                SELECT date_trunc(@granularity, listened_at) AS period_start,
                       1::bigint AS listening,
                       0::bigint AS geofence,
                       0::bigint AS feedbacks,
                       0::bigint AS guest_sessions,
                       0::numeric AS guest_revenue,
                       0::numeric AS vendor_revenue
                FROM listening_histories
                WHERE listened_at >= @from AND listened_at < @to

                UNION ALL

                SELECT date_trunc(@granularity, detected_at),
                       0, 1, 0, 0, 0, 0
                FROM geofence_events
                WHERE detected_at >= @from AND detected_at < @to

                UNION ALL

                SELECT date_trunc(@granularity, created_at),
                       0, 0, 1, 0, 0, 0
                FROM feedbacks
                WHERE created_at >= @from AND created_at < @to

                UNION ALL

                SELECT date_trunc(@granularity, access_started_at),
                       0, 0, 0, 1, access_price, 0
                FROM guest_sessions
                WHERE guest_payment_order_id IS NOT NULL
                  AND access_started_at >= @from AND access_started_at < @to

                UNION ALL

                SELECT date_trunc(@granularity, paid_at),
                       0, 0, 0, 0, 0, amount
                FROM payment_orders
                WHERE status = 'Paid'
                  AND paid_at >= @from AND paid_at < @to
            )
            SELECT period_start,
                   SUM(listening)::bigint, SUM(geofence)::bigint, SUM(feedbacks)::bigint,
                   SUM(guest_sessions)::bigint, SUM(guest_revenue), SUM(vendor_revenue)
            FROM activity_rows
            GROUP BY period_start
            ORDER BY period_start;", conn))
        {
            activity.Parameters.AddWithValue("@granularity", granularity);
            activity.Parameters.AddWithValue("@from", from);
            activity.Parameters.AddWithValue("@to", to);
            using var reader = activity.ExecuteReader();
            while (reader.Read())
            {
                result.Activity.Add(new AdminDashboardActivityDTO
                {
                    PeriodStart = reader.GetDateTime(0),
                    Listening = reader.GetInt64(1),
                    Geofence = reader.GetInt64(2),
                    Feedbacks = reader.GetInt64(3),
                    GuestSessions = reader.GetInt64(4),
                    GuestRevenue = reader.GetDecimal(5),
                    VendorRevenue = reader.GetDecimal(6)
                });
            }
        }

        return result;
    }
}
