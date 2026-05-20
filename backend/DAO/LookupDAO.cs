using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO.Lookup;

namespace VinhKhanhNarration.Api.DAO;

public abstract class LookupDAO<TDto> : BaseDAO where TDto : LookupDTO, new()
{
    protected abstract string TableName { get; }
    protected abstract string IdColumn { get; }

    protected LookupDAO(DbConnectionFactory factory) : base(factory) { }

    public long Insert(TDto dto)
    {
        var sql = $"INSERT INTO {TableName} (code, name, description, is_active) VALUES (@code, @name, @description, @isActive) RETURNING {IdColumn};";
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", dto.Code);
        cmd.Parameters.AddWithValue("@name", dto.Name);
        cmd.Parameters.AddWithValue("@description", DbValue(dto.Description));
        cmd.Parameters.AddWithValue("@isActive", dto.IsActive);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public bool Update(TDto dto)
    {
        var sql = $"UPDATE {TableName} SET code = @code, name = @name, description = @description, is_active = @isActive WHERE {IdColumn} = @id;";
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", dto.Id);
        cmd.Parameters.AddWithValue("@code", dto.Code);
        cmd.Parameters.AddWithValue("@name", dto.Name);
        cmd.Parameters.AddWithValue("@description", DbValue(dto.Description));
        cmd.Parameters.AddWithValue("@isActive", dto.IsActive);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool SoftDelete(long id) => ExecuteActive(id, false);
    public bool Restore(long id) => ExecuteActive(id, true);

    private bool ExecuteActive(long id, bool active)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"UPDATE {TableName} SET is_active = @active WHERE {IdColumn} = @id;", conn);
        cmd.Parameters.AddWithValue("@active", active);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public TDto? GetById(long id)
    {
        return QuerySingle($"SELECT * FROM {TableName} WHERE {IdColumn} = @id;", cmd => cmd.Parameters.AddWithValue("@id", id));
    }

    public TDto? GetByCode(string code)
    {
        return QuerySingle($"SELECT * FROM {TableName} WHERE code = @code;", cmd => cmd.Parameters.AddWithValue("@code", code));
    }

    public List<TDto> GetAll() => QueryList($"SELECT * FROM {TableName} ORDER BY {IdColumn} DESC;");
    public List<TDto> GetActive() => QueryList($"SELECT * FROM {TableName} WHERE is_active = TRUE ORDER BY {IdColumn} DESC;");

    public bool IsCodeExists(string code)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand($"SELECT COUNT(1) FROM {TableName} WHERE code = @code;", conn);
        cmd.Parameters.AddWithValue("@code", code);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private TDto? QuerySingle(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        bind(cmd);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private List<TDto> QueryList(string sql)
    {
        var list = new List<TDto>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    private TDto Map(NpgsqlDataReader reader)
    {
        return new TDto
        {
            Id = Convert.ToInt64(reader[IdColumn]),
            Code = Convert.ToString(reader["code"]) ?? string.Empty,
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            Description = reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"]),
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = Convert.ToDateTime(reader["created_at"]),
            UpdatedAt = Convert.ToDateTime(reader["updated_at"])
        };
    }
}

public class PlaceTypeDAO : LookupDAO<PlaceTypeDTO>
{
    public PlaceTypeDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "place_types";
    protected override string IdColumn => "place_type_id";
}

public class ContentTypeDAO : LookupDAO<ContentTypeDTO>
{
    public ContentTypeDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "content_types";
    protected override string IdColumn => "content_type_id";
}

public class TargetTypeDAO : LookupDAO<TargetTypeDTO>
{
    public TargetTypeDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "target_types";
    protected override string IdColumn => "target_type_id";
}

public class TranslationSourceDAO : LookupDAO<TranslationSourceDTO>
{
    public TranslationSourceDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "translation_sources";
    protected override string IdColumn => "translation_source_id";
}

public class TriggerModeDAO : LookupDAO<TriggerModeDTO>
{
    public TriggerModeDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "trigger_modes";
    protected override string IdColumn => "trigger_mode_id";
}

public class GeofenceEventTypeDAO : LookupDAO<GeofenceEventTypeDTO>
{
    public GeofenceEventTypeDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "geofence_event_types";
    protected override string IdColumn => "event_type_id";
}

public class GeofenceEventStatusDAO : LookupDAO<GeofenceEventStatusDTO>
{
    public GeofenceEventStatusDAO(DbConnectionFactory factory) : base(factory) { }
    protected override string TableName => "geofence_event_statuses";
    protected override string IdColumn => "event_status_id";
}
