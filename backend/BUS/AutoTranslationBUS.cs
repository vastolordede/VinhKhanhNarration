using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VinhKhanhNarration.Api.BUS;

public class AutoTranslationBUS
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private static readonly HashSet<string> SupportedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vi", "en", "ja", "ko", "zh"
    };

    public AutoTranslationBUS(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> TranslateAsync(
        string text,
        string sourceLanguageCode,
        string targetLanguageCode)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var source = NormalizeCode(sourceLanguageCode);
        var target = NormalizeCode(targetLanguageCode);

        if (source == target)
            return text.Trim();

        var provider = _configuration["Translation:Provider"] ?? "GoogleFree";

        if (provider.Equals("GoogleFree", StringComparison.OrdinalIgnoreCase))
        {
            return await TranslateWithGoogleFreeAsync(text, source, target);
        }

        if (provider.Equals("LibreTranslate", StringComparison.OrdinalIgnoreCase))
        {
            return await TranslateWithLibreTranslateAsync(text, source, target);
        }

        throw new InvalidOperationException($"Unsupported translation provider: {provider}");
    }

    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required.");

        var normalized = code.Trim().ToLowerInvariant();

        if (normalized.StartsWith("vi")) normalized = "vi";
        else if (normalized.StartsWith("en")) normalized = "en";
        else if (normalized.StartsWith("ja")) normalized = "ja";
        else if (normalized.StartsWith("ko")) normalized = "ko";
        else if (normalized.StartsWith("zh")) normalized = "zh";

        if (!SupportedCodes.Contains(normalized))
            throw new ArgumentException($"Unsupported language code: {code}");

        return normalized;
    }

    private async Task<string> TranslateWithGoogleFreeAsync(string text, string source, string target)
    {
        var googleSource = ToGoogleCode(source);
        var googleTarget = ToGoogleCode(target);

        var url =
            "https://translate.googleapis.com/translate_a/single" +
            "?client=gtx" +
            $"&sl={Uri.EscapeDataString(googleSource)}" +
            $"&tl={Uri.EscapeDataString(googleTarget)}" +
            "&dt=t" +
            $"&q={Uri.EscapeDataString(text)}";

        using var response = await _httpClient.GetAsync(url);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google free translation failed: {response.StatusCode} - {responseText}"
            );
        }

        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Google free translation returned invalid response.");
        }

        var translatedBuilder = new StringBuilder();
        var sentenceArray = root[0];

        foreach (var item in sentenceArray.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
            {
                var translatedSegment = item[0].GetString();
                if (!string.IsNullOrWhiteSpace(translatedSegment))
                {
                    translatedBuilder.Append(translatedSegment);
                }
            }
        }

        var translatedText = translatedBuilder.ToString().Trim();

        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new InvalidOperationException("Google free translation returned empty text.");
        }

        return translatedText;
    }

    private async Task<string> TranslateWithLibreTranslateAsync(string text, string source, string target)
    {
        var baseUrl = _configuration["Translation:LibreTranslateUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Missing config: Translation:LibreTranslateUrl"
            );
        }

        var requestBody = new Dictionary<string, object?>
        {
            ["q"] = text,
            ["source"] = source,
            ["target"] = target,
            ["format"] = "text"
        };

        var apiKey = _configuration["Translation:LibreTranslateApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            requestBody["api_key"] = apiKey;
        }

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(
            $"{baseUrl.TrimEnd('/')}/translate",
            content
        );

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Auto translation failed: {response.StatusCode} - {responseText}"
            );
        }

        var result = JsonSerializer.Deserialize<LibreTranslateResponse>(
            responseText,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        if (string.IsNullOrWhiteSpace(result?.TranslatedText))
        {
            throw new InvalidOperationException("Auto translation returned empty text.");
        }

        return result.TranslatedText.Trim();
    }

    private static string ToGoogleCode(string code)
    {
        return code switch
        {
            "zh" => "zh-CN",
            _ => code
        };
    }

    private sealed class LibreTranslateResponse
    {
        [JsonPropertyName("translatedText")]
        public string? TranslatedText { get; set; }
    }
}