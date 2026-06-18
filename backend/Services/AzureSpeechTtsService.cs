using System.Net;
using System.Text;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class AzureSpeechTtsService : ITextToSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AzureSpeechTtsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string ProviderName => "AzureSpeech";

    public async Task<byte[]> SynthesizeMp3Async(
        string text,
        string locale,
        string voiceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("TTS text is required.");
        if (string.IsNullOrWhiteSpace(locale))
            throw new ArgumentException("TTS locale is required.");
        if (string.IsNullOrWhiteSpace(voiceName))
            throw new ArgumentException("TTS voice is required.");

        var key = _configuration["Speech:Key"];
        var region = _configuration["Speech:Region"];
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
            throw new InvalidOperationException("Missing Speech:Key or Speech:Region configuration.");

        var endpoint = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";
        var escapedText = WebUtility.HtmlEncode(text.Trim());
        var escapedVoice = WebUtility.HtmlEncode(voiceName.Trim());
        var escapedLocale = WebUtility.HtmlEncode(locale.Trim());
        var ssml = $"<speak version='1.0' xml:lang='{escapedLocale}'><voice name='{escapedVoice}'>{escapedText}</voice></speak>";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
        request.Headers.Add("User-Agent", "VinhKhanhNarration");
        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Azure Speech TTS failed: {(int)response.StatusCode} - {error}");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
