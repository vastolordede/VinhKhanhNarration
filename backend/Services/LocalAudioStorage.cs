using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class LocalAudioStorage : IAudioStorage
{
    private const string StorageFolder = "generated-audio";
    private readonly IWebHostEnvironment _environment;

    public LocalAudioStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<AudioStorageWriteResult> SaveMp3Async(
        Stream audioStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioStream);

        var safeName = SanitizeFileName(fileName);
        var directory = GetStorageDirectory();
        Directory.CreateDirectory(directory);

        var targetPath = Path.Combine(directory, safeName);

        if (audioStream.CanSeek)
        {
            audioStream.Position = 0;
        }

        await using var file = new FileStream(
            targetPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await audioStream.CopyToAsync(file, cancellationToken);

        var storageKey = $"{StorageFolder}/{safeName}";
        var legacyUrl = $"/{storageKey}";

        return new AudioStorageWriteResult(storageKey, legacyUrl);
    }

    public Task<AudioStorageReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = ExtractSafeFileName(storageKey);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.FromResult<AudioStorageReadResult?>(null);
        }

        var directory = GetStorageDirectory();
        var filePath = Path.GetFullPath(Path.Combine(directory, fileName));
        var directoryPrefix = directory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!filePath.StartsWith(directoryPrefix, comparison)
            || !File.Exists(filePath))
        {
            return Task.FromResult<AudioStorageReadResult?>(null);
        }

        Stream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<AudioStorageReadResult?>(
            AudioStorageReadResult.FromStream(
                stream,
                "audio/mpeg",
                fileName));
    }

    private string GetStorageDirectory()
    {
        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        return Path.GetFullPath(Path.Combine(webRoot, StorageFolder));
    }

    private static string SanitizeFileName(string fileName)
    {
        var rawName = Path.GetFileName(fileName);
        var safeName = string.Concat(rawName.Select(ch =>
            char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' ? ch : '_'));

        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = $"audio-{Guid.NewGuid():N}.mp3";
        }

        if (!safeName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            safeName += ".mp3";
        }

        return safeName;
    }

    private static string? ExtractSafeFileName(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return null;
        }

        var normalized = storageKey.Trim();

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absolute))
        {
            normalized = absolute.AbsolutePath;
        }

        normalized = Uri.UnescapeDataString(normalized).Replace('\\', '/');
        var fileName = Path.GetFileName(normalized);

        return string.IsNullOrWhiteSpace(fileName)
            ? null
            : SanitizeFileName(fileName);
    }
}
