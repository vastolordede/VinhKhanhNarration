using Npgsql;
using VinhKhanhNarration.Api.Database;

namespace VinhKhanhNarration.Api.DAO;

public abstract class BaseDAO
{
    protected readonly DbConnectionFactory Factory;

    protected BaseDAO(DbConnectionFactory factory)
    {
        Factory = factory;
    }

    protected NpgsqlConnection CreateConnection() => Factory.CreateConnection();

    protected static object DbValue(object? value) => value ?? DBNull.Value;
}
