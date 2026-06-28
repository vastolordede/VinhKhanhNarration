namespace VinhKhanhNarration.Api.Services.Interfaces;

public sealed record AudioStorageWriteResult(
    string StorageKey,
    string? LegacyUrl = null);

public sealed record AudioStorageReadResult(
    Stream? ContentStream,
    string? RedirectUrl,
    string ContentType,
    string FileName)
{
    public static AudioStorageReadResult FromStream(
        Stream contentStream,
        string contentType,
        string fileName) =>
        new(contentStream, null, contentType, fileName);

    public static AudioStorageReadResult FromRedirect(
        string redirectUrl,
        string contentType,
        string fileName) =>
        new(null, redirectUrl, contentType, fileName);
}

public interface IAudioStorage
{
    Task<AudioStorageWriteResult> SaveMp3Async(
        Stream audioStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<AudioStorageReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
