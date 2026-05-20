using Microsoft.AspNetCore.Mvc.Testing;

namespace VinhKhanhNarration.Api.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        LoadBackendEnv();
    }

    private static void LoadBackendEnv()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            var envPath = Path.Combine(current.FullName, "backend", ".env");

            if (File.Exists(envPath))
            {
                LoadEnvFile(envPath);
                return;
            }

            current = current.Parent;
        }
    }

    private static void LoadEnvFile(string filePath)
    {
        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim().Trim('"');

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
