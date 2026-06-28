using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Services;

public sealed class R2AudioStorage : IAudioStorage, IDisposable
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;
    private readonly string _prefix;
    private readonly int _signedUrlMinutes;
    private readonly ILogger<R2AudioStorage> _logger;

    public R2AudioStorage(
        IConfiguration configuration,
        ILogger<R2AudioStorage> logger)
    {
        _logger = logger;

        var accountId = Require(configuration, "R2:AccountId");
        var accessKeyId = Require(configuration, "R2:AccessKeyId");
        var secretAccessKey = Require(configuration, "R2:SecretAccessKey");
        _bucketName = Require(configuration, "R2:BucketName");

        var configuredEndpoint = configuration["R2:Endpoint"]?.Trim();
        var endpoint = string.IsNullOrWhiteSpace(configuredEndpoint)
            ? $"https://{accountId}.r2.cloudflarestorage.com"
            : configuredEndpoint.TrimEnd('/');

        _prefix = NormalizePrefix(configuration["R2:Prefix"] ?? "audio");
        _signedUrlMinutes = GetPositiveInt(
            configuration["R2:SignedUrlMinutes"],
            fallback: 5);

        AWSConfigsS3.UseSignatureVersion4 = true;

        var credentials = new BasicAWSCredentials(
            accessKeyId,
            secretAccessKey);

        var clientConfig = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        _client = new AmazonS3Client(credentials, clientConfig);
    }

    public async Task<AudioStorageWriteResult> SaveMp3Async(
        Stream audioStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioStream);

        var safeName = SanitizeFileName(fileName);
        var objectKey = BuildObjectKey(safeName);

        if (audioStream.CanSeek)
        {
            audioStream.Position = 0;
        }

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = audioStream,
            ContentType = "audio/mpeg",
            AutoCloseStream = false,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };

        request.Metadata["source"] = "VinhKhanhNarration";
        request.Metadata["generated-at-utc"] = DateTime.UtcNow.ToString("O");

        var response = await _client.PutObjectAsync(request, cancellationToken);

        _logger.LogInformation(
            "Uploaded narration audio to R2. Bucket={Bucket}, Key={Key}, ETag={ETag}",
            _bucketName,
            objectKey,
            response.ETag);

        return new AudioStorageWriteResult(objectKey);
    }

    public Task<AudioStorageReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var objectKey = NormalizeObjectKey(storageKey);
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return Task.FromResult<AudioStorageReadResult?>(null);
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(_signedUrlMinutes),
            Protocol = Protocol.HTTPS
        };

        var signedUrl = _client.GetPreSignedURL(request);
        var fileName = Path.GetFileName(objectKey);

        return Task.FromResult<AudioStorageReadResult?>(
            AudioStorageReadResult.FromRedirect(
                signedUrl,
                "audio/mpeg",
                string.IsNullOrWhiteSpace(fileName) ? "audio.mp3" : fileName));
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private string BuildObjectKey(string safeName)
    {
        var datedPath = DateTime.UtcNow.ToString("yyyy/MM");
        var uniqueName = $"{Guid.NewGuid():N}-{safeName}";

        return string.IsNullOrWhiteSpace(_prefix)
            ? $"{datedPath}/{uniqueName}"
            : $"{_prefix}/{datedPath}/{uniqueName}";
    }

    private static string NormalizeObjectKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return string.Empty;
        }

        var value = storageKey.Trim();

        if (value.StartsWith("r2://", StringComparison.OrdinalIgnoreCase))
        {
            value = value[5..];
            var slashIndex = value.IndexOf('/');
            value = slashIndex >= 0 ? value[(slashIndex + 1)..] : string.Empty;
        }
        else if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
        {
            value = absolute.AbsolutePath.TrimStart('/');

            // Path-style R2 URLs include the bucket name as the first segment.
            var firstSlash = value.IndexOf('/');
            if (firstSlash >= 0)
            {
                value = value[(firstSlash + 1)..];
            }
        }

        return Uri.UnescapeDataString(value)
            .Replace('\\', '/')
            .TrimStart('/');
    }

    private static string NormalizePrefix(string value) =>
        value.Trim().Replace('\\', '/').Trim('/');

    private static string SanitizeFileName(string fileName)
    {
        var rawName = Path.GetFileName(fileName);
        var safeName = string.Concat(rawName.Select(ch =>
            char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.' ? ch : '_'));

        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "audio.mp3";
        }

        if (!safeName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            safeName += ".mp3";
        }

        return safeName;
    }

    private static string Require(IConfiguration configuration, string key)
    {
        var value = configuration[key]?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing R2 configuration: {key}. " +
                "Configure the corresponding environment variable on Render.");
        }

        return value;
    }

    private static int GetPositiveInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : fallback;
}
