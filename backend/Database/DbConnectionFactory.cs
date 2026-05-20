using Npgsql;

namespace VinhKhanhNarration.Api.Database;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "vinh_khanh_narration_db";
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Missing DB_PASSWORD in .env file.");
        }

        _connectionString =
            $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}