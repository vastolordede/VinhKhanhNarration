using System.Reflection;
using Npgsql;
using VinhKhanhNarration.Api.DAO.Interfaces;
using VinhKhanhNarration.Api.DAO.Mapping;
using VinhKhanhNarration.Api.Database;

namespace VinhKhanhNarration.Api.DAO;

public abstract class GenericCrudDAO<TDto> : BaseDAO, ICrudDAO<TDto, long>
    where TDto : class, new()
{
    private readonly string _tableName;
    private readonly List<(PropertyInfo Prop, DbColumnAttribute Attr)> _columns;
    private readonly (PropertyInfo Prop, DbColumnAttribute Attr) _key;

    protected GenericCrudDAO(DbConnectionFactory factory) : base(factory)
    {
        var tableAttr = typeof(TDto).GetCustomAttribute<DbTableAttribute>()
            ?? throw new InvalidOperationException($"DTO {typeof(TDto).Name} is missing DbTableAttribute.");

        _tableName = tableAttr.Name;
        _columns = typeof(TDto).GetProperties()
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<DbColumnAttribute>()))
            .Where(x => x.Attr != null)
            .Select(x => (x.Prop, x.Attr!))
            .ToList();

        _key = _columns.FirstOrDefault(x => x.Attr.IsKey);
        if (_key.Prop == null)
            throw new InvalidOperationException($"DTO {typeof(TDto).Name} is missing key column.");
    }

    public virtual long Insert(TDto dto)
    {
        var insertCols = _columns
            .Where(x => !x.Attr.IsIdentity && !x.Attr.IgnoreOnInsert)
            .Where(x => x.Attr.Name is not "created_at" and not "updated_at")
            .ToList();

        var cols = string.Join(", ", insertCols.Select(x => x.Attr.Name));
        var vals = string.Join(", ", insertCols.Select(x => "@" + x.Prop.Name));
        var sql = $"INSERT INTO {_tableName} ({cols}) VALUES ({vals}) RETURNING {_key.Attr.Name};";

        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var c in insertCols)
            cmd.Parameters.AddWithValue("@" + c.Prop.Name, DbValue(c.Prop.GetValue(dto)));

        var result = cmd.ExecuteScalar();
        return Convert.ToInt64(result);
    }

    public virtual bool Update(TDto dto)
    {
        var updateCols = _columns
            .Where(x => !x.Attr.IsKey && !x.Attr.IgnoreOnUpdate)
            .Where(x => x.Attr.Name is not "created_at" and not "updated_at")
            .ToList();

        var sets = string.Join(", ", updateCols.Select(x => $"{x.Attr.Name} = @{x.Prop.Name}"));
        var sql = $"UPDATE {_tableName} SET {sets} WHERE {_key.Attr.Name} = @Key;";

        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var c in updateCols)
            cmd.Parameters.AddWithValue("@" + c.Prop.Name, DbValue(c.Prop.GetValue(dto)));
        cmd.Parameters.AddWithValue("@Key", DbValue(_key.Prop.GetValue(dto)));

        return cmd.ExecuteNonQuery() > 0;
    }

    public virtual bool SoftDelete(long id)
    {
        if (HasColumn("is_active"))
            return ExecuteBool($"UPDATE {_tableName} SET is_active = FALSE WHERE {_key.Attr.Name} = @id;", id);

        return ExecuteBool($"DELETE FROM {_tableName} WHERE {_key.Attr.Name} = @id;", id);
    }

    public virtual bool Restore(long id)
    {
        if (!HasColumn("is_active")) return false;
        return ExecuteBool($"UPDATE {_tableName} SET is_active = TRUE WHERE {_key.Attr.Name} = @id;", id);
    }

    public virtual TDto? GetById(long id)
    {
        return QuerySingle($"SELECT * FROM {_tableName} WHERE {_key.Attr.Name} = @id;", cmd =>
        {
            cmd.Parameters.AddWithValue("@id", id);
        });
    }

    public virtual List<TDto> GetAll()
    {
        return QueryList($"SELECT * FROM {_tableName} ORDER BY {_key.Attr.Name} DESC;");
    }

    public virtual List<TDto> GetActive()
    {
        if (!HasColumn("is_active")) return GetAll();
        return QueryList($"SELECT * FROM {_tableName} WHERE is_active = TRUE ORDER BY {_key.Attr.Name} DESC;");
    }

    protected TDto? QuerySingle(string sql, Action<NpgsqlCommand>? bind = null)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        bind?.Invoke(cmd);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    protected List<TDto> QueryList(string sql, Action<NpgsqlCommand>? bind = null)
    {
        var list = new List<TDto>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        bind?.Invoke(cmd);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    protected bool ExecuteBool(string sql, object id)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", DbValue(id));
        return cmd.ExecuteNonQuery() > 0;
    }

    protected bool Exists(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        bind(cmd);
        var result = cmd.ExecuteScalar();
        return result != null && Convert.ToInt64(result) > 0;
    }

    protected TDto Map(NpgsqlDataReader reader)
    {
        var dto = new TDto();
        foreach (var c in _columns)
        {
            var ordinal = reader.GetOrdinal(c.Attr.Name);
            if (reader.IsDBNull(ordinal))
            {
                c.Prop.SetValue(dto, null);
                continue;
            }
            var value = reader.GetValue(ordinal);
            var targetType = Nullable.GetUnderlyingType(c.Prop.PropertyType) ?? c.Prop.PropertyType;
            c.Prop.SetValue(dto, Convert.ChangeType(value, targetType));
        }
        return dto;
    }

    private bool HasColumn(string columnName) => _columns.Any(x => x.Attr.Name == columnName);
}
