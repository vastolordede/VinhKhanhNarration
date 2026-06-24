using System.Text.Json;
using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("audit_logs")]
public class AuditLogDTO
{
    [DbColumn("audit_log_id", IsKey = true, IsIdentity = true)]
    public long AuditLogId { get; set; }

    [DbColumn("actor_type")]
    public string ActorType { get; set; } = "System";

    [DbColumn("actor_id")]
    public long? ActorId { get; set; }

    [DbColumn("guest_session_id")]
    public string? GuestSessionId { get; set; }

    [DbColumn("action")]
    public string Action { get; set; } = string.Empty;

    [DbColumn("entity_type")]
    public string? EntityType { get; set; }

    [DbColumn("entity_id")]
    public long? EntityId { get; set; }

    [DbColumn("details", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public JsonElement? Details { get; set; }

    [DbColumn("ip_address")]
    public string? IpAddress { get; set; }

    [DbColumn("user_agent")]
    public string? UserAgent { get; set; }

    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime CreatedAt { get; set; }
}

public class AuditLogQueryDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? ActorType { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
