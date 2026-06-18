using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class LocalAudioStorage : IAudioStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public LocalAudioStorage(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<string> SaveMp3Async(
        Stream audioStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var safeName = string.Concat(fileName.Select(ch =>
            char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' ? ch : '_'));

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var directory = Path.Combine(webRoot, "generated-audio");
        Directory.CreateDirectory(directory);

        var targetPath = Path.Combine(directory, safeName);
        await using var file = File.Create(targetPath);
        await audioStream.CopyToAsync(file, cancellationToken);

        var baseUrl = (_configuration["Storage:PublicBaseUrl"]
            ?? "http://localhost:5151").TrimEnd('/');
        return $"{baseUrl}/generated-audio/{Uri.EscapeDataString(safeName)}";
    }
}
