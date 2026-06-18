namespace VinhKhanhNarration.Api.Services.Interfaces;

public interface ITextToSpeechService
{
    string ProviderName { get; }
    Task<byte[]> SynthesizeMp3Async(
        string text,
        string locale,
        string voiceName,
        CancellationToken cancellationToken = default);
}
