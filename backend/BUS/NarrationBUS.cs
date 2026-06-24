using System.Security.Cryptography;
using System.Text;
using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.DTO.Common;
using VinhKhanhNarration.Api.Services.Interfaces;

namespace VinhKhanhNarration.Api.BUS;

public class NarrationContentBUS : ICrudBUS<NarrationContentDTO, long>
{
    private readonly NarrationContentDAO _dao;
    private readonly ContentTypeDAO _contentTypeDAO;
    private readonly LanguageDAO _languageDAO;
    private readonly NarrationTranslationDAO _translationDAO;
    private readonly AudioFileDAO _audioDAO;
    private readonly TranslationSourceDAO _translationSourceDAO;
    private readonly ITranslationService _translationService;
    private readonly AudioFileBUS _audioFileBUS;
    private readonly VendorModuleBUS _vendorModuleBUS;
    private readonly PlaceDAO _placeDAO;
    private readonly DishDAO _dishDAO;

    public NarrationContentBUS(
        NarrationContentDAO dao,
        ContentTypeDAO contentTypeDAO,
        LanguageDAO languageDAO,
        NarrationTranslationDAO translationDAO,
        AudioFileDAO audioDAO,
        TranslationSourceDAO translationSourceDAO,
        ITranslationService translationService,
        AudioFileBUS audioFileBUS,
        VendorModuleBUS vendorModuleBUS,
        PlaceDAO placeDAO,
        DishDAO dishDAO)
    {
        _dao = dao;
        _contentTypeDAO = contentTypeDAO;
        _languageDAO = languageDAO;
        _translationDAO = translationDAO;
        _audioDAO = audioDAO;
        _translationSourceDAO = translationSourceDAO;
        _translationService = translationService;
        _audioFileBUS = audioFileBUS;
        _vendorModuleBUS = vendorModuleBUS;
        _placeDAO = placeDAO;
        _dishDAO = dishDAO;
    }

    public long Create(NarrationContentDTO dto)
    {
        dto.WorkflowStatus = NarrationWorkflowStatuses.Draft;
        dto.RejectionReason = null;
        dto.SubmittedAt = null;
        dto.ReviewedBy = null;
        dto.ReviewedAt = null;
        dto.PublishedAt = null;
        ValidateNarration(dto);
        return _dao.Insert(dto);
    }

    public bool Update(NarrationContentDTO dto)
    {
        var existing = _dao.GetById(dto.NarrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (existing.WorkflowStatus == NarrationWorkflowStatuses.Published)
            throw new InvalidOperationException(
                "Published narration cannot be edited directly. Create a new draft or unpublish it first.");

        ValidateNarration(dto);

        var sourceChanged = existing.Title != dto.Title
            || existing.OriginalText != dto.OriginalText
            || existing.SourceLanguageId != dto.SourceLanguageId;

        var updated = _dao.Update(dto);
        if (updated && sourceChanged)
        {
            _translationDAO.MarkOutdatedByNarration(dto.NarrationId);
            foreach (var translation in _translationDAO.GetByNarrationId(dto.NarrationId))
                _audioDAO.MarkOutdatedByTranslationId(translation.TranslationId);
        }

        return updated;
    }

    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public NarrationContentDTO? GetById(long id) => _dao.GetById(id);
    public List<NarrationContentDTO> GetAll() => _dao.GetAll();
    public List<NarrationContentDTO> GetActive() => _dao.GetActive();
    public List<NarrationContentDTO> GetByPlaceId(long placeId) => _dao.GetByPlaceId(placeId);
    public List<NarrationContentDTO> GetByDishId(long dishId) => _dao.GetByDishId(dishId);
    public List<NarrationContentDTO> GetPendingReview() =>
        _dao.GetByWorkflowStatus(NarrationWorkflowStatuses.PendingReview);

    public long CreateVendorDraft(VendorNarrationRequestDTO request)
    {
        if (request.VendorUserId <= 0)
            throw new ArgumentException("VendorUserId is required.");

        _vendorModuleBUS.EnsureCanManageContent(request.VendorUserId);

        var dto = new NarrationContentDTO
        {
            Title = request.Title.Trim(),
            OriginalText = request.OriginalText.Trim(),
            ContentTypeId = request.ContentTypeId,
            PlaceId = request.PlaceId,
            DishId = request.DishId,
            SourceLanguageId = request.SourceLanguageId,
            CreatedBy = null,
            SubmittedByVendorId = request.VendorUserId,
            WorkflowStatus = NarrationWorkflowStatuses.PendingReview,
            SubmittedAt = DateTime.UtcNow,
            IsActive = false
        };

        ValidateNarration(dto);
        var id = _dao.Insert(dto);
        _vendorModuleBUS.NotifyNarration(
            request.VendorUserId,
            "NarrationSubmitted",
            "Đã gửi nội dung chờ duyệt",
            $"Nội dung '{dto.Title}' đã được gửi cho Admin.",
            id);
        return id;
    }

    public bool UpdateVendorDraft(long narrationId, VendorNarrationRequestDTO request)
    {
        if (request.VendorUserId <= 0)
            throw new ArgumentException("VendorUserId is required.");

        _vendorModuleBUS.EnsureCanManageContent(request.VendorUserId);

        var existing = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (existing.SubmittedByVendorId != request.VendorUserId)
            throw new UnauthorizedAccessException("Vendor does not own this narration.");

        if (existing.WorkflowStatus == NarrationWorkflowStatuses.PendingReview)
            throw new InvalidOperationException("Nội dung đang chờ Admin duyệt.");

        var sourceChanged = existing.Title != request.Title.Trim()
            || existing.OriginalText != request.OriginalText.Trim()
            || existing.SourceLanguageId != request.SourceLanguageId;

        existing.Title = request.Title.Trim();
        existing.OriginalText = request.OriginalText.Trim();
        existing.ContentTypeId = request.ContentTypeId;
        existing.PlaceId = request.PlaceId;
        existing.DishId = request.DishId;
        existing.SourceLanguageId = request.SourceLanguageId;
        existing.PreviousWorkflowStatus = existing.WorkflowStatus;
        existing.WorkflowStatus = NarrationWorkflowStatuses.PendingReview;
        existing.IsActive = false;
        existing.RejectionReason = null;
        existing.ModerationReason = null;
        existing.ModerationByAdminId = null;
        existing.SubmittedAt = DateTime.UtcNow;
        existing.ReviewedBy = null;
        existing.ReviewedAt = null;
        existing.HiddenAt = null;
        existing.DeletedAt = null;

        ValidateNarration(existing);
        var updated = _dao.Update(existing);

        if (updated && sourceChanged)
        {
            _translationDAO.MarkOutdatedByNarration(narrationId);
            foreach (var translation in _translationDAO.GetByNarrationId(narrationId))
                _audioDAO.MarkOutdatedByTranslationId(translation.TranslationId);
        }

        if (updated)
        {
            _vendorModuleBUS.NotifyNarration(
                request.VendorUserId,
                "NarrationResubmitted",
                "Đã gửi lại nội dung",
                $"Nội dung '{existing.Title}' đã được cập nhật và gửi duyệt lại.",
                narrationId);
        }

        return updated;
    }

    public List<NarrationContentDTO> GetVendorNarrations(long vendorUserId)
    {
        if (vendorUserId <= 0)
            throw new ArgumentException("VendorUserId is required.");
        return _dao.GetByVendorId(vendorUserId);
    }

    public bool SubmitVendorNarration(long narrationId, long vendorUserId)
    {
        if (vendorUserId <= 0)
            throw new ArgumentException("VendorUserId is required.");

        _vendorModuleBUS.EnsureCanManageContent(vendorUserId);
        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");
        if (narration.SubmittedByVendorId != vendorUserId)
            throw new UnauthorizedAccessException("Vendor does not own this narration.");
        if (narration.WorkflowStatus == NarrationWorkflowStatuses.PendingReview)
            return true;

        if (!_dao.SubmitForReview(narrationId, vendorUserId))
            throw new InvalidOperationException(
                "Narration cannot be submitted. Check ownership and workflow status.");

        return true;
    }

    public bool VendorModerateNarration(long narrationId, long vendorUserId, string action)
    {
        _vendorModuleBUS.EnsureCanManageContent(vendorUserId);
        var normalized = action.Trim().ToLowerInvariant();
        if (!_dao.VendorModerate(narrationId, vendorUserId, normalized))
            throw new InvalidOperationException("Không thể thực hiện thao tác trên nội dung này.");
        return true;
    }

    public bool AdminModerateNarration(
        long narrationId,
        long adminId,
        NarrationModerationRequestDTO request)
    {
        var normalized = request.Action.Trim().ToLowerInvariant();
        if (normalized is "hide" or "delete" && string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Admin phải nhập lý do.");

        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (!_dao.AdminModerate(
            narrationId, adminId, normalized, request.Reason?.Trim()))
        {
            throw new InvalidOperationException("Không thể thực hiện thao tác kiểm duyệt.");
        }

        if (narration.SubmittedByVendorId.HasValue)
        {
            var actionLabel = normalized switch
            {
                "hide" => "bị Admin ẩn",
                "delete" => "bị Admin xóa mềm",
                "restore" => "được Admin khôi phục",
                _ => normalized
            };
            _vendorModuleBUS.NotifyNarration(
                narration.SubmittedByVendorId.Value,
                $"NarrationAdmin{normalized}",
                $"Nội dung {actionLabel}",
                string.IsNullOrWhiteSpace(request.Reason)
                    ? $"Nội dung '{narration.Title}' {actionLabel}."
                    : $"Nội dung '{narration.Title}' {actionLabel}. Lý do: {request.Reason}",
                narrationId);
        }

        return true;
    }

    public VendorNarrationStatusDTO GetVendorNarrationStatus(
        long narrationId,
        long vendorUserId)
    {
        if (vendorUserId <= 0)
            throw new ArgumentException("VendorUserId is required.");

        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (narration.SubmittedByVendorId != vendorUserId)
            throw new UnauthorizedAccessException("Vendor does not own this narration.");

        var translations = _translationDAO.GetByNarrationId(narrationId);
        var audioFiles = translations
            .SelectMany(x => _audioDAO.GetByTranslationId(x.TranslationId))
            .OrderByDescending(x => x.AudioId)
            .ToList();

        return new VendorNarrationStatusDTO
        {
            Narration = narration,
            Translations = translations,
            AudioFiles = audioFiles
        };
    }

    public async Task<ReviewNarrationResultDTO> ReviewNarrationAsync(
        long narrationId,
        ReviewNarrationRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        if (request.AdminId <= 0)
            throw new ArgumentException("AdminId is required.");
        if (!request.Approved && string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ArgumentException("Rejection reason is required.");

        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (!_dao.Review(
            narrationId,
            request.AdminId,
            request.Approved,
            request.RejectionReason?.Trim()))
        {
            throw new InvalidOperationException("Narration is not pending review.");
        }

        var result = new ReviewNarrationResultDTO
        {
            NarrationId = narrationId,
            Approved = request.Approved
        };

        if (narration.SubmittedByVendorId.HasValue)
        {
            _vendorModuleBUS.NotifyNarration(
                narration.SubmittedByVendorId.Value,
                request.Approved ? "NarrationApproved" : "NarrationRejected",
                request.Approved ? "Nội dung đã được duyệt" : "Nội dung bị từ chối",
                request.Approved
                    ? $"Nội dung '{narration.Title}' đã được duyệt và đang tự động dịch, tạo audio."
                    : $"Nội dung '{narration.Title}' bị từ chối. Lý do: {request.RejectionReason}",
                narrationId);
        }

        if (request.Approved)
        {
            result.Processing = await ProcessApprovedNarrationAsync(
                narrationId,
                request.AdminId,
                cancellationToken);
        }

        return result;
    }

    public async Task<GenerateTranslationsResultDTO> GenerateTranslationsAsync(
        long narrationId,
        GenerateTranslationsRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (narration.WorkflowStatus is not (
            NarrationWorkflowStatuses.Approved or NarrationWorkflowStatuses.Published))
        {
            throw new InvalidOperationException(
                "Admin must approve source narration before translation.");
        }

        LanguageDTO? sourceLanguage = null;
        if (narration.SourceLanguageId.HasValue)
            sourceLanguage = _languageDAO.GetById(narration.SourceLanguageId.Value);

        if (sourceLanguage == null && !string.IsNullOrWhiteSpace(request.SourceLanguageCode))
            sourceLanguage = _languageDAO.GetByCode(request.SourceLanguageCode.Trim());

        sourceLanguage ??= _languageDAO.GetDefault();
        if (sourceLanguage == null)
            throw new InvalidOperationException("Source language not found.");

        var manualSource = _translationSourceDAO.GetByCode("Manual")
            ?? throw new InvalidOperationException("Missing translation source: Manual.");
        var machineSource = _translationSourceDAO.GetByCode("AI")
            ?? _translationSourceDAO.GetByCode("AzureTranslator")
            ?? manualSource;

        var languages = _languageDAO.GetActive()
            .Where(x => x.IsContentEnabled
                && (x.IsTranslationSupported || x.LanguageId == sourceLanguage.LanguageId))
            .OrderBy(x => x.LanguageId)
            .ToList();

        var result = new GenerateTranslationsResultDTO { NarrationId = narrationId };

        foreach (var language in languages)
        {
            try
            {
                var isSource = language.LanguageId == sourceLanguage.LanguageId;
                var title = isSource
                    ? narration.Title.Trim()
                    : await _translationService.TranslateAsync(
                        narration.Title,
                        sourceLanguage.LanguageCode,
                        language.LanguageCode,
                        cancellationToken);
                var text = isSource
                    ? narration.OriginalText.Trim()
                    : await _translationService.TranslateAsync(
                        narration.OriginalText,
                        sourceLanguage.LanguageCode,
                        language.LanguageCode,
                        cancellationToken);

                var existed = _translationDAO.IsTranslationExists(
                    narrationId,
                    language.LanguageId);

                var translationId = _translationDAO.UpsertGenerated(
                    new NarrationTranslationDTO
                    {
                        NarrationId = narrationId,
                        LanguageId = language.LanguageId,
                        TranslatedTitle = title,
                        TranslatedText = text,
                        TranslationSourceId = isSource ? manualSource.Id : machineSource.Id,
                        Provider = isSource ? "ManualSource" : _translationService.ProviderName,
                        Status = TranslationStatuses.Approved,
                        IsReviewed = true,
                        ReviewedBy = request.AdminId > 0
                            ? request.AdminId
                            : narration.ReviewedBy,
                        ReviewedAt = DateTime.UtcNow
                    });

                _audioDAO.MarkOutdatedByTranslationId(translationId);
                if (existed) result.Updated++;
                else result.Created++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{language.LanguageCode}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<AutoProcessNarrationResultDTO> ProcessApprovedNarrationAsync(
        long narrationId,
        long adminId,
        CancellationToken cancellationToken = default)
    {
        if (adminId <= 0)
            throw new ArgumentException("AdminId is required.");

        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (narration.WorkflowStatus is not (
            NarrationWorkflowStatuses.Approved or NarrationWorkflowStatuses.Published))
        {
            throw new InvalidOperationException(
                "Narration source must be approved before automatic processing.");
        }

        var result = new AutoProcessNarrationResultDTO
        {
            NarrationId = narrationId
        };

        var translationResult = await GenerateTranslationsAsync(
            narrationId,
            new GenerateTranslationsRequestDTO
            {
                AdminId = adminId
            },
            cancellationToken);

        result.TranslationsCreated = translationResult.Created;
        result.TranslationsUpdated = translationResult.Updated;
        result.Errors.AddRange(translationResult.Errors);

        var audioLanguages = _languageDAO.GetActive()
            .Where(x => x.IsContentEnabled && x.IsTtsSupported)
            .OrderBy(x => x.LanguageId)
            .ToList();

        result.AudioTargetCount = audioLanguages.Count;

        foreach (var language in audioLanguages)
        {
            var translation = _translationDAO.GetApprovedByNarrationAndLanguage(
                narrationId,
                language.LanguageId);

            if (translation == null)
            {
                result.Errors.Add(
                    $"{language.LanguageCode}: approved translation is unavailable.");
                continue;
            }

            try
            {
                await _audioFileBUS.GenerateAsync(
                    translation.TranslationId,
                    new GenerateAudioRequestDTO
                    {
                        AdminId = adminId,
                        PublishAfterGenerate = true
                    },
                    cancellationToken);

                result.AudioReady++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(
                    $"{language.LanguageCode} audio: {ex.Message}");
            }
        }

        if (result.Errors.Count == 0)
        {
            try
            {
                Publish(
                    narrationId,
                    new PublishNarrationRequestDTO
                    {
                        AdminId = adminId
                    });

                result.Published = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"publish: {ex.Message}");
            }
        }

        return result;
    }

    public bool Publish(long narrationId, PublishNarrationRequestDTO request)
    {
        if (request.AdminId <= 0)
            throw new ArgumentException("AdminId is required.");

        var narration = _dao.GetById(narrationId)
            ?? throw new InvalidOperationException("Narration not found.");

        if (narration.WorkflowStatus is not (
            NarrationWorkflowStatuses.Approved or NarrationWorkflowStatuses.Published))
        {
            throw new InvalidOperationException("Narration source is not approved.");
        }

        var requiredLanguages = _languageDAO.GetActive()
            .Where(x => x.IsContentEnabled && x.IsTtsSupported)
            .OrderBy(x => x.LanguageId)
            .ToList();

        if (requiredLanguages.Count == 0)
            throw new InvalidOperationException("No active content/TTS language is configured.");

        foreach (var language in requiredLanguages)
        {
            var translation = _translationDAO.GetApprovedByNarrationAndLanguage(
                narrationId,
                language.LanguageId)
                ?? throw new InvalidOperationException(
                    $"Translation {language.LanguageCode} is not approved.");

            var audio = _audioDAO.GetReadyByTranslationId(translation.TranslationId)
                ?? throw new InvalidOperationException(
                    $"Audio {language.LanguageCode} is not ready.");

            if (string.IsNullOrWhiteSpace(audio.AudioUrl))
                throw new InvalidOperationException(
                    $"Audio {language.LanguageCode} has no URL.");
        }

        if (!_dao.Publish(narrationId, request.AdminId))
            throw new InvalidOperationException("Narration could not be published.");

        if (!_audioDAO.PublishByNarration(narrationId))
            throw new InvalidOperationException("No ready audio was published.");

        return true;
    }

    private void ValidateNarration(NarrationContentDTO dto)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors["title"] = "Title is required.";
        if (string.IsNullOrWhiteSpace(dto.OriginalText))
            errors["originalText"] = "Original Text is required.";
        if (dto.ContentTypeId <= 0)
            errors["contentTypeId"] = "Content Type is required.";

        if (!dto.SourceLanguageId.HasValue || dto.SourceLanguageId.Value <= 0)
        {
            errors["sourceLanguageId"] = "Source Language is required.";
        }
        else
        {
            var language = _languageDAO.GetById(dto.SourceLanguageId.Value);
            if (language == null || !language.IsActive || !language.IsContentEnabled)
                errors["sourceLanguageId"] = "Source Language is invalid or disabled.";
        }

        if (errors.Count == 0)
        {
            var contentType = _contentTypeDAO.GetById(dto.ContentTypeId);
            var code = contentType?.Code;

            if (string.IsNullOrWhiteSpace(code))
            {
                errors["contentTypeId"] = "Content Type is invalid.";
            }
            else if (code.Equals("Place", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.PlaceId == null)
                {
                    errors["placeId"] = "Place is required for Place Narration.";
                }
                else if (dto.SubmittedByVendorId is long vendorUserId &&
                         !_placeDAO.IsOwnedByVendor(dto.PlaceId.Value, vendorUserId))
                {
                    errors["placeId"] = "Vendor chỉ được tạo narration cho sạp của mình.";
                }

                if (dto.DishId != null)
                    errors["dishId"] = "Dish must be empty for Place Narration.";
            }
            else if (code.Equals("Dish", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.DishId == null)
                {
                    errors["dishId"] = "Dish is required for Dish Narration.";
                }
                else if (dto.SubmittedByVendorId is long vendorUserId &&
                         !_dishDAO.IsOwnedByVendor(dto.DishId.Value, vendorUserId))
                {
                    errors["dishId"] = "Vendor chỉ được tạo narration cho món ăn của mình.";
                }

                if (dto.PlaceId != null)
                    errors["placeId"] = "Place must be empty for Dish Narration.";
            }
            else if (code.Equals("General", StringComparison.OrdinalIgnoreCase))
            {
                if (dto.SubmittedByVendorId.HasValue)
                    errors["contentTypeId"] = "Vendor chỉ được tạo narration cho sạp hoặc món ăn của mình.";
                if (dto.PlaceId != null)
                    errors["placeId"] = "Place must be empty for General Narration.";
                if (dto.DishId != null)
                    errors["dishId"] = "Dish must be empty for General Narration.";
            }
        }

        if (errors.Count > 0)
            throw new ApiValidationException(errors);
    }
}

public class NarrationTranslationBUS : ICrudBUS<NarrationTranslationDTO, long>
{
    private readonly NarrationTranslationDAO _dao;
    private readonly AudioFileDAO _audioDAO;

    public NarrationTranslationBUS(
        NarrationTranslationDAO dao,
        AudioFileDAO audioDAO)
    {
        _dao = dao;
        _audioDAO = audioDAO;
    }

    public long Create(NarrationTranslationDTO dto)
    {
        ValidateTranslation(dto);
        if (_dao.IsTranslationExists(dto.NarrationId, dto.LanguageId))
            throw new InvalidOperationException(
                "Translation for this narration/language already exists.");

        dto.Status = TranslationStatuses.PendingReview;
        dto.IsReviewed = false;
        dto.ReviewedBy = null;
        dto.ReviewedAt = null;
        return _dao.Insert(dto);
    }

    public bool Update(NarrationTranslationDTO dto)
    {
        ValidateTranslation(dto);

        var current = _dao.GetById(dto.TranslationId)
            ?? throw new InvalidOperationException("Translation not found.");

        var contentChanged = current.TranslatedTitle != dto.TranslatedTitle
            || current.TranslatedText != dto.TranslatedText;
        var audioTextChanged = current.TranslatedText != dto.TranslatedText;

        if (contentChanged)
        {
            dto.Status = TranslationStatuses.Approved;
            dto.IsReviewed = true;
            dto.ReviewedBy = current.ReviewedBy;
            dto.ReviewedAt = DateTime.UtcNow;
            dto.ErrorMessage = null;
        }

        var updated = _dao.Update(dto);
        if (updated && audioTextChanged)
            _audioDAO.MarkOutdatedByTranslationId(dto.TranslationId);

        return updated;
    }

    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public NarrationTranslationDTO? GetById(long id) => _dao.GetById(id);
    public List<NarrationTranslationDTO> GetAll() => _dao.GetAll();
    public List<NarrationTranslationDTO> GetActive() => _dao.GetActive();
    public List<NarrationTranslationDTO> GetByNarrationId(long narrationId) =>
        _dao.GetByNarrationId(narrationId);
    public NarrationTranslationDTO? GetByNarrationAndLanguage(
        long narrationId,
        long languageId) =>
        _dao.GetByNarrationAndLanguage(narrationId, languageId);

    public bool ReviewTranslation(
        long translationId,
        ReviewTranslationRequestDTO request)
    {
        if (request.AdminId <= 0)
            throw new ArgumentException("AdminId is required.");
        if (!request.Approved && string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ArgumentException("Rejection reason is required.");

        if (!_dao.Review(
            translationId,
            request.AdminId,
            request.Approved,
            request.RejectionReason?.Trim()))
        {
            throw new InvalidOperationException(
                "Translation cannot be reviewed in its current status.");
        }

        return true;
    }

    private static void ValidateTranslation(NarrationTranslationDTO dto)
    {
        var errors = new Dictionary<string, string>();
        if (dto.NarrationId <= 0)
            errors["narrationId"] = "Narration is required.";
        if (dto.LanguageId <= 0)
            errors["languageId"] = "Language is required.";
        if (dto.TranslationSourceId <= 0)
            errors["translationSourceId"] = "Translation Source is required.";
        if (string.IsNullOrWhiteSpace(dto.TranslatedTitle))
            errors["translatedTitle"] = "Translated Title is required.";
        if (string.IsNullOrWhiteSpace(dto.TranslatedText))
            errors["translatedText"] = "Translated Text is required.";

        if (errors.Count > 0)
            throw new ApiValidationException(errors);
    }
}

public class AudioFileBUS : ICrudBUS<AudioFileDTO, long>
{
    private readonly AudioFileDAO _dao;
    private readonly NarrationTranslationDAO _translationDAO;
    private readonly LanguageDAO _languageDAO;
    private readonly ITextToSpeechService _ttsService;
    private readonly IAudioStorage _audioStorage;

    public AudioFileBUS(
        AudioFileDAO dao,
        NarrationTranslationDAO translationDAO,
        LanguageDAO languageDAO,
        ITextToSpeechService ttsService,
        IAudioStorage audioStorage)
    {
        _dao = dao;
        _translationDAO = translationDAO;
        _languageDAO = languageDAO;
        _ttsService = ttsService;
        _audioStorage = audioStorage;
    }

    public long Create(AudioFileDTO dto) =>
        throw new InvalidOperationException(
            "Manual Audio URL creation is disabled. Use generate endpoint.");

    public bool Update(AudioFileDTO dto) =>
        throw new InvalidOperationException(
            "Manual Audio URL update is disabled. Use generate endpoint.");

    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public AudioFileDTO? GetById(long id) => _dao.GetById(id);
    public List<AudioFileDTO> GetAll() => _dao.GetAll();
    public List<AudioFileDTO> GetActive() => _dao.GetActive();
    public List<AudioFileDTO> GetByTranslationId(long translationId) =>
        _dao.GetByTranslationId(translationId);

    public async Task<AudioFileDTO> GenerateAsync(
        long translationId,
        GenerateAudioRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        if (request.AdminId <= 0)
            throw new ArgumentException("AdminId is required.");

        var translation = _translationDAO.GetById(translationId)
            ?? throw new InvalidOperationException("Translation not found.");

        if (translation.Status != TranslationStatuses.Approved
            || !translation.IsReviewed)
        {
            throw new InvalidOperationException(
                "Admin must approve translation before generating audio.");
        }

        var language = _languageDAO.GetById(translation.LanguageId)
            ?? throw new InvalidOperationException("Language not found.");

        if (!language.IsActive
            || !language.IsContentEnabled
            || !language.IsTtsSupported)
        {
            throw new InvalidOperationException(
                "TTS is disabled for this language.");
        }

        var locale = language.Locale;
        var voiceName = string.IsNullOrWhiteSpace(request.VoiceName)
            ? language.DefaultVoiceId
            : request.VoiceName.Trim();

        if (string.IsNullOrWhiteSpace(locale)
            || string.IsNullOrWhiteSpace(voiceName))
        {
            throw new InvalidOperationException(
                "Language locale/default voice is not configured.");
        }

        var sourceHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(translation.TranslatedText)));

        var existing = _dao.GetLatestByTranslationId(translationId);
        if (existing?.Status == AudioStatuses.Ready
            && existing.IsActive
            && existing.SourceTextHash == sourceHash
            && existing.VoiceName == voiceName
            && !string.IsNullOrWhiteSpace(existing.AudioUrl))
        {
            return existing;
        }

        var pending = new AudioFileDTO
        {
            TranslationId = translationId,
            Provider = _ttsService.ProviderName,
            VoiceName = voiceName,
            FileFormat = "mp3",
            GeneratedBy = "TTS",
            Status = AudioStatuses.Generating,
            SourceTextHash = sourceHash,
            IsActive = true
        };
        var pendingId = _dao.Insert(pending);

        try
        {
            var bytes = await _ttsService.SynthesizeMp3Async(
                translation.TranslatedText,
                locale,
                voiceName,
                cancellationToken);

            await using var stream = new MemoryStream(bytes);
            var fileName =
                $"narration-{translation.NarrationId}-translation-{translationId}-{DateTime.UtcNow:yyyyMMddHHmmss}.mp3";
            var audioUrl = await _audioStorage.SaveMp3Async(
                stream,
                fileName,
                cancellationToken);

            _dao.DeactivateByTranslationId(translationId);

            var ready = new AudioFileDTO
            {
                TranslationId = translationId,
                AudioUrl = audioUrl,
                Provider = _ttsService.ProviderName,
                VoiceName = voiceName,
                FileFormat = "mp3",
                GeneratedBy = "TTS",
                Status = AudioStatuses.Ready,
                SourceTextHash = sourceHash,
                GeneratedAt = DateTime.UtcNow,
                PublishedAt = request.PublishAfterGenerate
                    ? DateTime.UtcNow
                    : null,
                IsActive = true
            };
            ready.AudioId = _dao.Insert(ready);
            return ready;
        }
        catch (Exception ex)
        {
            var failed = _dao.GetById(pendingId);
            if (failed != null)
            {
                failed.Status = AudioStatuses.Failed;
                failed.ErrorMessage = ex.Message;
                failed.IsActive = false;
                _dao.Update(failed);
            }

            throw;
        }
    }
}

public class PublicNarrationBUS
{
    private readonly NarrationContentDAO _narrationDAO;
    private readonly NarrationTranslationDAO _translationDAO;
    private readonly AudioFileDAO _audioDAO;
    private readonly LanguageDAO _languageDAO;

    public PublicNarrationBUS(
        NarrationContentDAO narrationDAO,
        NarrationTranslationDAO translationDAO,
        AudioFileDAO audioDAO,
        LanguageDAO languageDAO)
    {
        _narrationDAO = narrationDAO;
        _translationDAO = translationDAO;
        _audioDAO = audioDAO;
        _languageDAO = languageDAO;
    }

    public PublicNarrationResultDTO ResolvePlace(long placeId, long languageId)
    {
        var narration = _narrationDAO.GetMainPublishedByPlaceId(placeId)
            ?? throw new InvalidOperationException("NARRATION_NOT_AVAILABLE");
        return Resolve(narration, languageId, placeId, null);
    }

    public PublicNarrationResultDTO ResolveDish(long dishId, long languageId)
    {
        var narration = _narrationDAO.GetMainPublishedByDishId(dishId)
            ?? throw new InvalidOperationException("NARRATION_NOT_AVAILABLE");
        return Resolve(narration, languageId, null, dishId);
    }

    public PublicNarrationResultDTO ResolveNarration(
        long narrationId,
        long languageId)
    {
        var narration = _narrationDAO.GetPublishedByIdForPublic(narrationId)
            ?? throw new InvalidOperationException("NARRATION_NOT_AVAILABLE");

        return Resolve(
            narration,
            languageId,
            narration.PlaceId,
            narration.DishId);
    }

    private PublicNarrationResultDTO Resolve(
        NarrationContentDTO narration,
        long languageId,
        long? placeId,
        long? dishId)
    {
        var language = _languageDAO.GetById(languageId)
            ?? throw new InvalidOperationException("LANGUAGE_NOT_AVAILABLE");

        if (!language.IsActive || !language.IsContentEnabled)
            throw new InvalidOperationException("LANGUAGE_NOT_AVAILABLE");

        var translation = _translationDAO.GetApprovedByNarrationAndLanguage(
            narration.NarrationId,
            languageId)
            ?? throw new InvalidOperationException("CONTENT_LANGUAGE_NOT_AVAILABLE");

        var audio = _audioDAO.GetPublishedPlayableByTranslationId(
            translation.TranslationId)
            ?? throw new InvalidOperationException("AUDIO_NOT_READY");

        return new PublicNarrationResultDTO
        {
            PlaceId = placeId,
            DishId = dishId,
            NarrationId = narration.NarrationId,
            TranslationId = translation.TranslationId,
            AudioId = audio.AudioId,
            Title = translation.TranslatedTitle,
            Text = translation.TranslatedText,
            AudioUrl = audio.AudioUrl!,
            LanguageCode = language.LanguageCode,
            Locale = language.Locale ?? language.LanguageCode
        };
    }
}
