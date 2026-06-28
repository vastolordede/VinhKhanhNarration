using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin,ContentManager,Reviewer,Vendor")]
[Route("api/secure-files")]
public class SecureFilesController : ControllerBase
{
    private readonly AudioFileDAO _audioDAO;
    private readonly VendorModuleDAO _vendorDAO;
    private readonly IAudioStorage _audioStorage;
    private readonly IWebHostEnvironment _environment;
    private readonly FileExtensionContentTypeProvider _contentTypes = new();

    public SecureFilesController(
        AudioFileDAO audioDAO,
        VendorModuleDAO vendorDAO,
        IAudioStorage audioStorage,
        IWebHostEnvironment environment)
    {
        _audioDAO = audioDAO;
        _vendorDAO = vendorDAO;
        _audioStorage = audioStorage;
        _environment = environment;
    }

    [HttpGet("audio/{audioId:long}")]
    public async Task<IActionResult> Audio(
        long audioId,
        CancellationToken cancellationToken)
    {
        var audio = _audioDAO.GetById(audioId);
        if (audio == null
            || !audio.IsActive
            || audio.Status != AudioStatuses.Ready
            || (string.IsNullOrWhiteSpace(audio.StorageKey)
                && string.IsNullOrWhiteSpace(audio.AudioUrl)))
        {
            return NotFound();
        }

        if (User.IsInRole("Vendor"))
        {
            var vendorId = GetVendorId();
            if (!_audioDAO.CanVendorAccess(audioId, vendorId))
            {
                return Forbid();
            }
        }

        if (!string.IsNullOrWhiteSpace(audio.StorageKey))
        {
            var storedAudio = await _audioStorage.OpenReadAsync(
                audio.StorageKey,
                cancellationToken);

            if (storedAudio == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(storedAudio.RedirectUrl))
            {
                return Redirect(storedAudio.RedirectUrl);
            }

            if (storedAudio.ContentStream == null)
            {
                return NotFound();
            }

            return File(
                storedAudio.ContentStream,
                storedAudio.ContentType,
                enableRangeProcessing: true);
        }

        if (TryGetExternalUrl(audio.AudioUrl!, out var externalUrl))
        {
            return Redirect(externalUrl);
        }

        var filePath = ResolveStoredFile("generated-audio", audio.AudioUrl!);
        if (filePath == null)
        {
            return NotFound();
        }

        return PhysicalFile(
            filePath,
            "audio/mpeg",
            enableRangeProcessing: true);
    }

    [HttpGet("vendor-documents/{documentId:long}")]
    public IActionResult VendorDocument(long documentId)
    {
        var document = _vendorDAO.GetDocumentById(documentId);
        if (document == null) return NotFound();

        if (User.IsInRole("Vendor") && document.VendorUserId != GetVendorId())
            return Forbid();

        if (TryGetExternalUrl(document.FileUrl, out var externalUrl))
            return Redirect(externalUrl);

        var relativeFolder = Path.Combine("vendor-documents", document.VendorUserId.ToString());
        var filePath = ResolveStoredFile(relativeFolder, document.FileUrl);
        if (filePath == null) return NotFound();

        if (!_contentTypes.TryGetContentType(document.FileName, out var contentType))
            contentType = "application/octet-stream";

        Response.Headers["Content-Disposition"] = BuildInlineDisposition(document.FileName);
        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }

    private long GetVendorId()
    {
        var value = User.FindFirstValue("vendorUserId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(value, out var vendorId) || vendorId <= 0)
            throw new UnauthorizedAccessException("Vendor token không hợp lệ.");
        return vendorId;
    }

    private string? ResolveStoredFile(string relativeFolder, string storedUrl)
    {
        var pathValue = Uri.TryCreate(storedUrl, UriKind.Absolute, out var absolute)
            ? absolute.AbsolutePath
            : storedUrl;
        var fileName = Path.GetFileName(Uri.UnescapeDataString(pathValue));
        if (string.IsNullOrWhiteSpace(fileName)) return null;

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var directory = Path.GetFullPath(Path.Combine(webRoot, relativeFolder));
        var filePath = Path.GetFullPath(Path.Combine(directory, fileName));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var directoryPrefix = directory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!filePath.StartsWith(directoryPrefix, comparison) || !System.IO.File.Exists(filePath))
            return null;
        return filePath;
    }

    private static bool TryGetExternalUrl(string storedUrl, out string externalUrl)
    {
        externalUrl = string.Empty;
        if (!Uri.TryCreate(storedUrl, UriKind.Absolute, out var absolute)) return false;
        if (absolute.AbsolutePath.StartsWith("/generated-audio/", StringComparison.OrdinalIgnoreCase) ||
            absolute.AbsolutePath.StartsWith("/vendor-documents/", StringComparison.OrdinalIgnoreCase))
            return false;
        if (absolute.Scheme != Uri.UriSchemeHttp && absolute.Scheme != Uri.UriSchemeHttps)
            return false;
        externalUrl = absolute.ToString();
        return true;
    }

    private static string BuildInlineDisposition(string fileName)
    {
        var safeAscii = string.Concat(fileName.Select(ch =>
            ch >= 32 && ch <= 126 && ch is not '"' and not '\\' ? ch : '_'));
        var encoded = Uri.EscapeDataString(fileName);
        return $"inline; filename=\"{safeAscii}\"; filename*=UTF-8''{encoded}";
    }
}
