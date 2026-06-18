using System.Text;
using System.Text.Json;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class AzureTranslatorService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AzureTranslatorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string ProviderName => "AzureTranslator";

    public async Task<string> TranslateAsync(
        string text,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var source = NormalizeCode(sourceLanguageCode);
        var target = NormalizeCode(targetLanguageCode);
        if (source == target) return text.Trim();

        var key = _configuration["Translator:Key"];
        var region = _configuration["Translator:Region"];
        var endpoint = (_configuration["Translator:Endpoint"]
            ?? "https://api.cognitive.microsofttranslator.com").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Missing Translator:Key configuration.");

        var url = $"{endpoint}/translate?api-version=3.0&from={Uri.EscapeDataString(source)}&to={Uri.EscapeDataString(target)}";
        var json = JsonSerializer.Serialize(new[] { new { Text = text } });

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        if (!string.IsNullOrWhiteSpace(region))
            request.Headers.Add("Ocp-Apim-Subscription-Region", region);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Azure Translator failed: {(int)response.StatusCode} - {responseText}");

        using var document = JsonDocument.Parse(responseText);
        var translated = document.RootElement[0]
            .GetProperty("translations")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(translated))
            throw new InvalidOperationException("Azure Translator returned empty text.");

        return translated.Trim();
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required.");

        var normalized = code.Trim().ToLowerInvariant();
        var dash = normalized.IndexOf('-');
        return dash > 0 ? normalized[..dash] : normalized;
    }
}
