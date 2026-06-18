namespace VinhKhanhNarration.Api.Services.Interfaces;

public interface ITranslationService
{
    string ProviderName { get; }
    Task<string> TranslateAsync(
        string text,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken = default);
}
