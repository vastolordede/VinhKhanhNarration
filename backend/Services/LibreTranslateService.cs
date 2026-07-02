using System.Net;
using System.Text;
using System.Text.Json;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

/// <summary>
/// Dịch nội dung bằng một LibreTranslate instance do dự án tự host.
/// Không gọi endpoint Google Translate không chính thức nên tránh CAPTCHA/429
/// do lưu lượng dùng chung từ GoogleFree.
/// </summary>
public sealed class LibreTranslateService : ITranslationService
{
    private static readonly HashSet<string> SupportedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vi", "en", "ja", "ko", "zh"
    };

    private static readonly HashSet<HttpStatusCode> RetryableStatusCodes =
    [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    ];

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LibreTranslateService> _logger;

    public LibreTranslateService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LibreTranslateService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string ProviderName => "LibreTranslate";

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

        if (source.Equals(target, StringComparison.OrdinalIgnoreCase))
            return text.Trim();

        var baseUrl = (
            _configuration["Translation:LibreTranslateUrl"]
            ?? "http://localhost:5000"
        ).TrimEnd('/');

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                "Translation:LibreTranslateUrl must be an absolute HTTP/HTTPS URL.");
        }

        var retryAttempts = ReadPositiveInt(
            "Translation:RetryAttempts",
            defaultValue: 3,
            maximumValue: 8);

        var retryBaseDelayMilliseconds = ReadPositiveInt(
            "Translation:RetryBaseDelayMilliseconds",
            defaultValue: 750,
            maximumValue: 30_000);

        Exception? lastException = null;

        for (var attempt = 1; attempt <= retryAttempts; attempt++)
        {
            try
            {
                using var request = BuildRequest(
                    $"{baseUrl}/translate",
                    text,
                    source,
                    target);

                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                    return ParseTranslatedText(responseText);

                var error = new InvalidOperationException(
                    $"LibreTranslate failed: HTTP {(int)response.StatusCode} " +
                    $"({response.ReasonPhrase}). {CreateSafeResponseSummary(responseText)}");

                if (!RetryableStatusCodes.Contains(response.StatusCode) || attempt == retryAttempts)
                    throw error;

                lastException = error;
                await DelayBeforeRetryAsync(
                    attempt,
                    retryBaseDelayMilliseconds,
                    response.Headers.RetryAfter?.Delta,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var error = new TimeoutException(
                    "LibreTranslate request timed out. Check the service URL, resources and model loading status.");

                if (attempt == retryAttempts)
                    throw error;

                lastException = error;
                await DelayBeforeRetryAsync(
                    attempt,
                    retryBaseDelayMilliseconds,
                    retryAfter: null,
                    cancellationToken);
            }
            catch (HttpRequestException error)
            {
                if (attempt == retryAttempts)
                {
                    throw new InvalidOperationException(
                        "Cannot connect to LibreTranslate. Check Translation:LibreTranslateUrl " +
                        "and make sure the self-hosted service is running.",
                        error);
                }

                lastException = error;
                await DelayBeforeRetryAsync(
                    attempt,
                    retryBaseDelayMilliseconds,
                    retryAfter: null,
                    cancellationToken);
            }
        }

        throw new InvalidOperationException(
            "LibreTranslate failed after all retry attempts.",
            lastException);
    }

    private HttpRequestMessage BuildRequest(
        string url,
        string text,
        string source,
        string target)
    {
        var requestBody = new Dictionary<string, object?>
        {
            ["q"] = text,
            ["source"] = source,
            ["target"] = target,
            ["format"] = "text"
        };

        var apiKey = _configuration["Translation:LibreTranslateApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            requestBody["api_key"] = apiKey.Trim();

        var json = JsonSerializer.Serialize(requestBody);

        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static string ParseTranslatedText(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);

            if (!document.RootElement.TryGetProperty("translatedText", out var translatedElement))
                throw new InvalidOperationException(
                    "LibreTranslate response does not contain translatedText.");

            var translatedText = translatedElement.ValueKind switch
            {
                JsonValueKind.String => translatedElement.GetString(),
                JsonValueKind.Array => string.Join(
                    Environment.NewLine,
                    translatedElement
                        .EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.String)
                        .Select(item => item.GetString())
                        .Where(item => !string.IsNullOrWhiteSpace(item))),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(translatedText))
                throw new InvalidOperationException("LibreTranslate returned empty text.");

            return translatedText.Trim();
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException(
                "LibreTranslate returned invalid JSON. " +
                CreateSafeResponseSummary(responseText),
                error);
        }
    }

    private async Task DelayBeforeRetryAsync(
        int attempt,
        int baseDelayMilliseconds,
        TimeSpan? retryAfter,
        CancellationToken cancellationToken)
    {
        var exponentialDelay = Math.Min(
            baseDelayMilliseconds * Math.Pow(2, attempt - 1),
            30_000);

        var delay = retryAfter is { } serverDelay && serverDelay > TimeSpan.Zero
            ? serverDelay
            : TimeSpan.FromMilliseconds(exponentialDelay + Random.Shared.Next(100, 450));

        _logger.LogWarning(
            "LibreTranslate attempt {Attempt} failed. Retrying after {DelayMilliseconds} ms.",
            attempt,
            (int)delay.TotalMilliseconds);

        await Task.Delay(delay, cancellationToken);
    }

    private int ReadPositiveInt(string key, int defaultValue, int maximumValue)
    {
        var rawValue = _configuration[key];

        if (!int.TryParse(rawValue, out var value) || value <= 0)
            return defaultValue;

        return Math.Min(value, maximumValue);
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required.", nameof(code));

        var normalized = code.Trim().ToLowerInvariant().Replace('_', '-');

        if (normalized.StartsWith("vi", StringComparison.Ordinal)) normalized = "vi";
        else if (normalized.StartsWith("en", StringComparison.Ordinal)) normalized = "en";
        else if (normalized.StartsWith("ja", StringComparison.Ordinal)) normalized = "ja";
        else if (normalized.StartsWith("ko", StringComparison.Ordinal)) normalized = "ko";
        else if (normalized.StartsWith("zh", StringComparison.Ordinal)) normalized = "zh";

        if (!SupportedCodes.Contains(normalized))
            throw new ArgumentException($"Unsupported language code: {code}", nameof(code));

        return normalized;
    }

    private static string CreateSafeResponseSummary(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
            return "The server returned an empty response.";

        var compact = string.Join(
            ' ',
            responseText
                .Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim()));

        const int maximumLength = 500;
        return compact.Length <= maximumLength
            ? compact
            : compact[..maximumLength] + "...";
    }
}
