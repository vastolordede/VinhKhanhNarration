using System.Diagnostics;
using System.Text;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class EdgeTtsService : ITextToSpeechService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EdgeTtsService> _logger;

    public EdgeTtsService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<EdgeTtsService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public string ProviderName => "EdgeTTS";

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

        var pythonPath = ResolvePythonPath();
        var scriptPath = ResolveConfiguredPath(
            _configuration["EdgeTts:ScriptPath"],
            Path.Combine("tools", "edge_tts_synthesize.py"));

        if (!File.Exists(pythonPath))
        {
            throw new InvalidOperationException(
                $"Edge TTS Python environment not found: {pythonPath}. " +
                "Run PowerShell script backend/tools/setup_edge_tts.ps1 first.");
        }

        if (!File.Exists(scriptPath))
            throw new InvalidOperationException($"Edge TTS script not found: {scriptPath}");

        var tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "vinhkhanh-edge-tts",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var inputFile = Path.Combine(tempDirectory, "input.txt");
        var outputFile = Path.Combine(tempDirectory, "output.mp3");

        try
        {
            await File.WriteAllTextAsync(
                inputFile,
                text.Trim(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);

            using var process = new Process
            {
                StartInfo = BuildStartInfo(
                    pythonPath,
                    scriptPath,
                    inputFile,
                    outputFile,
                    voiceName),
                EnableRaisingEvents = true
            };

            if (!process.Start())
                throw new InvalidOperationException("Unable to start Edge TTS Python process.");

            var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            var timeoutSeconds = GetPositiveInt("EdgeTts:TimeoutSeconds", 120);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                throw new TimeoutException(
                    $"Edge TTS exceeded the configured timeout of {timeoutSeconds} seconds.");
            }
            catch
            {
                TryKill(process);
                throw;
            }

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            if (process.ExitCode != 0)
            {
                var detail = string.IsNullOrWhiteSpace(standardError)
                    ? standardOutput
                    : standardError;
                throw new InvalidOperationException(
                    $"Edge TTS failed with exit code {process.ExitCode}: {detail.Trim()}");
            }

            if (!File.Exists(outputFile))
                throw new InvalidOperationException("Edge TTS did not create an MP3 output file.");

            var bytes = await File.ReadAllBytesAsync(outputFile, cancellationToken);
            if (bytes.Length == 0)
                throw new InvalidOperationException("Edge TTS returned an empty MP3 file.");

            if (!string.IsNullOrWhiteSpace(standardOutput))
                _logger.LogInformation("Edge TTS completed: {Output}", standardOutput.Trim());

            return bytes;
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private ProcessStartInfo BuildStartInfo(
        string pythonPath,
        string scriptPath,
        string inputFile,
        string outputFile,
        string voiceName)
    {
        var info = new ProcessStartInfo
        {
            FileName = pythonPath,
            WorkingDirectory = _environment.ContentRootPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        info.ArgumentList.Add(scriptPath);
        info.ArgumentList.Add("--input-file");
        info.ArgumentList.Add(inputFile);
        info.ArgumentList.Add("--output-file");
        info.ArgumentList.Add(outputFile);
        info.ArgumentList.Add("--voice");
        info.ArgumentList.Add(voiceName.Trim());
        info.ArgumentList.Add("--rate");
        info.ArgumentList.Add(_configuration["EdgeTts:Rate"] ?? "+0%");
        info.ArgumentList.Add("--volume");
        info.ArgumentList.Add(_configuration["EdgeTts:Volume"] ?? "+0%");
        info.ArgumentList.Add("--pitch");
        info.ArgumentList.Add(_configuration["EdgeTts:Pitch"] ?? "+0Hz");

        return info;
    }

    private string ResolvePythonPath()
    {
        var configured = _configuration["EdgeTts:PythonPath"];
        var defaultRelative = OperatingSystem.IsWindows()
            ? Path.Combine(".venv-tts", "Scripts", "python.exe")
            : Path.Combine(".venv-tts", "bin", "python");

        return ResolveConfiguredPath(configured, defaultRelative);
    }

    private string ResolveConfiguredPath(string? configured, string fallbackRelative)
    {
        var value = string.IsNullOrWhiteSpace(configured)
            ? fallbackRelative
            : configured.Trim();

        return Path.IsPathRooted(value)
            ? Path.GetFullPath(value)
            : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, value));
    }

    private int GetPositiveInt(string key, int fallback)
    {
        return int.TryParse(_configuration[key], out var value) && value > 0
            ? value
            : fallback;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best effort cleanup only.
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch
        {
            // Temp cleanup should not hide a successful synthesis result.
        }
    }
}
