using Npgsql;

namespace VinhKhanhNarration.Api.Database;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        _connectionString = BuildConnectionString();
    }

    private static string BuildConnectionString()
    {
        var databaseUrl =
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            Environment.GetEnvironmentVariable("NEON_DATABASE_URL") ??
            Environment.GetEnvironmentVariable("POSTGRES_URL");

        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return NormalizeConnectionString(databaseUrl);
        }

        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var portText = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "vinh_khanh_narration_db";
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Missing database configuration. Set DATABASE_URL or DB_PASSWORD."
            );
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(portText, out var port) ? port : 5432,
            Database = database,
            Username = username,
            Password = password,
            Pooling = true
        };

        var sslMode = Environment.GetEnvironmentVariable("DB_SSLMODE");

        if (!string.IsNullOrWhiteSpace(sslMode))
        {
            builder.SslMode = ParseSslMode(sslMode);
        }

        var trustServerCertificate = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");

        if (bool.TryParse(trustServerCertificate, out var trust))
        {
            builder.TrustServerCertificate = trust;
        }

        return builder.ConnectionString;
    }

    private static string NormalizeConnectionString(string rawConnectionString)
    {
        var raw = rawConnectionString.Trim();

        if (raw.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new NpgsqlConnectionStringBuilder(raw);

            if (builder.Host.Contains("neon.tech", StringComparison.OrdinalIgnoreCase) &&
                builder.SslMode == SslMode.Disable)
            {
                builder.SslMode = SslMode.Require;
            }

            return builder.ConnectionString;
        }

        if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(raw);
            var userInfo = uri.UserInfo.Split(':', 2);

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
                Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "",
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                Pooling = true
            };

            var query = ParseQuery(uri.Query);

            if (query.TryGetValue("sslmode", out var sslMode))
            {
                builder.SslMode = ParseSslMode(sslMode);
            }
            else if (builder.Host.Contains("neon.tech", StringComparison.OrdinalIgnoreCase))
            {
                builder.SslMode = SslMode.Require;
            }

            return builder.ConnectionString;
        }

        return raw;
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        var cleanQuery = query.TrimStart('?');

        foreach (var part in cleanQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);

            if (pair.Length == 2)
            {
                result[Uri.UnescapeDataString(pair[0])] = Uri.UnescapeDataString(pair[1]);
            }
        }

        return result;
    }

    private static SslMode ParseSslMode(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "disable" => SslMode.Disable,
            "allow" => SslMode.Allow,
            "prefer" => SslMode.Prefer,
            "require" => SslMode.Require,
            "verify-ca" => SslMode.VerifyCA,
            "verifyca" => SslMode.VerifyCA,
            "verify-full" => SslMode.VerifyFull,
            "verifyfull" => SslMode.VerifyFull,
            _ => SslMode.Require
        };
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}