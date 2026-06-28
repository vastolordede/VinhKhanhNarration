using System.Text;
using System.Text.Json;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class GoogleFreeTranslationService : ITranslationService
{
    private static readonly HashSet<string> SupportedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vi", "en", "ja", "ko", "zh"
    };

    private readonly HttpClient _httpClient;

    public GoogleFreeTranslationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderName => "GoogleFree";

    public async Task<string> TranslateAsync(
        string text,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var source = NormalizeCode(sourceLanguageCode);
        var target = NormalizeCode(targetLanguageCode);

        if (source == target)
            return text.Trim();

        var url =
            "https://translate.googleapis.com/translate_a/single" +
            "?client=gtx" +
            $"&sl={Uri.EscapeDataString(ToGoogleCode(source))}" +
            $"&tl={Uri.EscapeDataString(ToGoogleCode(target))}" +
            "&dt=t" +
            $"&q={Uri.EscapeDataString(text)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google free translation failed: {(int)response.StatusCode} - {responseText}");
        }

        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            throw new InvalidOperationException("Google free translation returned invalid response.");

        var translated = new StringBuilder();
        foreach (var item in root[0].EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() == 0)
                continue;

            var segment = item[0].GetString();
            if (!string.IsNullOrWhiteSpace(segment))
                translated.Append(segment);
        }

        var result = translated.ToString().Trim();
        if (string.IsNullOrWhiteSpace(result))
            throw new InvalidOperationException("Google free translation returned empty text.");

        return result;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required.");

        var normalized = code.Trim().ToLowerInvariant();
        var dashIndex = normalized.IndexOf('-');
        if (dashIndex > 0)
            normalized = normalized[..dashIndex];

        if (!SupportedCodes.Contains(normalized))
            throw new ArgumentException($"Unsupported language code: {code}");

        return normalized;
    }

    private static string ToGoogleCode(string code) =>
        code.Equals("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : code;
}
