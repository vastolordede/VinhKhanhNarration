namespace VinhKhanhNarration.Api.Utils;

public static class EnvLoader
{
    public static void Load(string filePath = ".env")
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            if (trimmedLine.StartsWith("#"))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim();

            value = value.Trim('"');

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}