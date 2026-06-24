using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.DAO;

public class AuditLogDAO : BaseDAO
{
    public AuditLogDAO(DbConnectionFactory factory) : base(factory) { }

    public long Insert(
        string actorType,
        long? actorId,
        string? guestSessionId,
        string action,
        string? entityType,
        long? entityId,
        object? details,
        string? ipAddress,
        string? userAgent)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO audit_logs
            (actor_type, actor_id, guest_session_id, action, entity_type,
             entity_id, details, ip_address, user_agent)
            VALUES
            (@actorType, @actorId, @guestSessionId, @action, @entityType,
             @entityId, @details, @ipAddress, @userAgent)
            RETURNING audit_log_id;", conn);
        cmd.Parameters.AddWithValue("@actorType", actorType);
        cmd.Parameters.AddWithValue("@actorId", DbValue(actorId));
        cmd.Parameters.AddWithValue("@guestSessionId", DbValue(guestSessionId));
        cmd.Parameters.AddWithValue("@action", action);
        cmd.Parameters.AddWithValue("@entityType", DbValue(entityType));
        cmd.Parameters.AddWithValue("@entityId", DbValue(entityId));

        var json = details == null ? null : JsonSerializer.Serialize(details);
        var detailParam = cmd.Parameters.Add("@details", NpgsqlDbType.Jsonb);
        detailParam.Value = json == null ? DBNull.Value : json;

        cmd.Parameters.AddWithValue("@ipAddress", DbValue(ipAddress));
        cmd.Parameters.AddWithValue("@userAgent", DbValue(userAgent));
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public PagedResultDTO<AuditLogDTO> GetPaged(AuditLogQueryDTO query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var filters = new List<string> { "1 = 1" };

        if (!string.IsNullOrWhiteSpace(query.ActorType)) filters.Add("actor_type = @actorType");
        if (!string.IsNullOrWhiteSpace(query.Action)) filters.Add("action ILIKE @action");
        if (!string.IsNullOrWhiteSpace(query.EntityType)) filters.Add("entity_type = @entityType");
        if (query.From.HasValue) filters.Add("created_at >= @from");
        if (query.To.HasValue) filters.Add("created_at <= @to");

        var where = string.Join(" AND ", filters);
        using var conn = CreateConnection();
        conn.Open();

        long total;
        using (var count = new NpgsqlCommand($"SELECT COUNT(1) FROM audit_logs WHERE {where};", conn))
        {
            Bind(count, query);
            total = Convert.ToInt64(count.ExecuteScalar());
        }

        var items = new List<AuditLogDTO>();
        using (var cmd = new NpgsqlCommand($@"
            SELECT audit_log_id, actor_type, actor_id, guest_session_id,
                   action, entity_type, entity_id, details,
                   ip_address, user_agent, created_at
            FROM audit_logs
            WHERE {where}
            ORDER BY audit_log_id DESC
            OFFSET @offset LIMIT @limit;", conn))
        {
            Bind(cmd, query);
            cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@limit", pageSize);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                JsonElement? details = null;
                if (!reader.IsDBNull(7))
                {
                    using var document = JsonDocument.Parse(reader.GetString(7));
                    details = document.RootElement.Clone();
                }

                items.Add(new AuditLogDTO
                {
                    AuditLogId = reader.GetInt64(0),
                    ActorType = reader.GetString(1),
                    ActorId = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                    GuestSessionId = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Action = reader.GetString(4),
                    EntityType = reader.IsDBNull(5) ? null : reader.GetString(5),
                    EntityId = reader.IsDBNull(6) ? null : reader.GetInt64(6),
                    Details = details,
                    IpAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
                    UserAgent = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreatedAt = reader.GetDateTime(10)
                });
            }
        }

        return new PagedResultDTO<AuditLogDTO>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    private static void Bind(NpgsqlCommand cmd, AuditLogQueryDTO query)
    {
        if (!string.IsNullOrWhiteSpace(query.ActorType))
            cmd.Parameters.AddWithValue("@actorType", query.ActorType.Trim());
        if (!string.IsNullOrWhiteSpace(query.Action))
            cmd.Parameters.AddWithValue("@action", $"%{query.Action.Trim()}%");
        if (!string.IsNullOrWhiteSpace(query.EntityType))
            cmd.Parameters.AddWithValue("@entityType", query.EntityType.Trim());
        if (query.From.HasValue) cmd.Parameters.AddWithValue("@from", query.From.Value);
        if (query.To.HasValue) cmd.Parameters.AddWithValue("@to", query.To.Value);
    }
}
