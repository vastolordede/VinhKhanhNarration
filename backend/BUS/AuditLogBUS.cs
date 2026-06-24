using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.BUS;

public class AuditLogBUS
{
    private readonly AuditLogDAO _dao;
    public AuditLogBUS(AuditLogDAO dao) => _dao = dao;

    public PagedResultDTO<AuditLogDTO> GetPaged(AuditLogQueryDTO query) =>
        _dao.GetPaged(query);

    public long Write(
        string actorType,
        long? actorId,
        string? guestSessionId,
        string action,
        string? entityType,
        long? entityId,
        object? details,
        string? ipAddress,
        string? userAgent) =>
        _dao.Insert(
            actorType,
            actorId,
            guestSessionId,
            action,
            entityType,
            entityId,
            details,
            ipAddress,
            userAgent);
}
