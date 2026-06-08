using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.BUS;

public class NarrationContentBUS : ICrudBUS<NarrationContentDTO, long>
{
    private readonly NarrationContentDAO _dao;
private readonly ContentTypeDAO _contentTypeDAO;
private readonly LanguageDAO _languageDAO;
private readonly NarrationTranslationDAO _translationDAO;
private readonly TranslationSourceDAO _translationSourceDAO;
private readonly AutoTranslationBUS _autoTranslationBUS;

public NarrationContentBUS(
    NarrationContentDAO dao,
    ContentTypeDAO contentTypeDAO,
    LanguageDAO languageDAO,
    NarrationTranslationDAO translationDAO,
    TranslationSourceDAO translationSourceDAO,
    AutoTranslationBUS autoTranslationBUS)
{
    _dao = dao;
    _contentTypeDAO = contentTypeDAO;
    _languageDAO = languageDAO;
    _translationDAO = translationDAO;
    _translationSourceDAO = translationSourceDAO;
    _autoTranslationBUS = autoTranslationBUS;
}

    public long Create(NarrationContentDTO dto) { ValidateNarrationTarget(dto); return _dao.Insert(dto); }
    public async Task<CreateNarrationAutoTranslateResultDTO> CreateWithAutoTranslationsAsync(
    CreateNarrationAutoTranslateRequestDTO request)
{
    var sourceCode = AutoTranslationBUS.NormalizeCode(request.SourceLanguageCode);

    if (sourceCode != "vi" && sourceCode != "en")
    {
        throw new ArgumentException("SourceLanguageCode chỉ được nhập 'vi' hoặc 'en'.");
    }

    var content = new NarrationContentDTO
    {
        Title = request.Title,
        OriginalText = request.OriginalText,
        ContentTypeId = request.ContentTypeId,
        PlaceId = request.PlaceId,
        DishId = request.DishId,
        CreatedBy = request.CreatedBy,
        IsActive = request.IsActive
    };

    ValidateNarrationTarget(content);

    var supportedOrder = new[] { "vi", "en", "ja", "ko", "zh" };

    var activeLanguages = _languageDAO
        .GetActive()
        .Where(x => supportedOrder.Contains(AutoTranslationBUS.NormalizeCode(x.LanguageCode)))
        .OrderBy(x => Array.IndexOf(
            supportedOrder,
            AutoTranslationBUS.NormalizeCode(x.LanguageCode)
        ))
        .ToList();

    if (activeLanguages.Count == 0)
    {
        throw new InvalidOperationException("Không tìm thấy ngôn ngữ active trong bảng languages.");
    }

    var manualSource = _translationSourceDAO.GetByCode("Manual")
        ?? throw new InvalidOperationException("Missing translation source: Manual.");

    var aiSource =
        _translationSourceDAO.GetByCode("AI")
        ?? _translationSourceDAO.GetByCode("GoogleTranslate")
        ?? manualSource;

    var preparedTranslations = new List<NarrationTranslationDTO>();

    foreach (var language in activeLanguages)
    {
        var targetCode = AutoTranslationBUS.NormalizeCode(language.LanguageCode);
        var isSource = targetCode == sourceCode;

        var translatedTitle = isSource
            ? request.Title.Trim()
            : await _autoTranslationBUS.TranslateAsync(request.Title, sourceCode, targetCode);

        var translatedText = isSource
            ? request.OriginalText.Trim()
            : await _autoTranslationBUS.TranslateAsync(request.OriginalText, sourceCode, targetCode);

        preparedTranslations.Add(new NarrationTranslationDTO
        {
            LanguageId = language.LanguageId,
            TranslatedTitle = translatedTitle,
            TranslatedText = translatedText,
            TranslationSourceId = isSource ? manualSource.Id : aiSource.Id,
            IsReviewed = isSource,
            ReviewedBy = null
        });
    }

    var narrationId = _dao.Insert(content);

    foreach (var translation in preparedTranslations)
    {
        translation.NarrationId = narrationId;
        translation.TranslationId = _translationDAO.Insert(translation);
    }

    return new CreateNarrationAutoTranslateResultDTO
    {
        NarrationId = narrationId,
        Translations = preparedTranslations
    };
}
   public async Task<BackfillNarrationTranslationsResultDTO> BackfillMissingTranslationsAsync(
    BackfillNarrationTranslationsRequestDTO request)
{
    var result = new BackfillNarrationTranslationsResultDTO();

    var fallbackSourceCode = AutoTranslationBUS.NormalizeCode(request.FallbackSourceLanguageCode);

    if (fallbackSourceCode != "vi" && fallbackSourceCode != "en")
    {
        throw new ArgumentException("FallbackSourceLanguageCode chỉ được nhập 'vi' hoặc 'en'.");
    }

    var maxItems = request.MaxItems <= 0 ? 5 : Math.Min(request.MaxItems, 10);
    var supportedOrder = new[] { "vi", "en", "ja", "ko", "zh" };

    var activeLanguages = _languageDAO
        .GetActive()
        .Where(x => supportedOrder.Contains(AutoTranslationBUS.NormalizeCode(x.LanguageCode)))
        .OrderBy(x => Array.IndexOf(
            supportedOrder,
            AutoTranslationBUS.NormalizeCode(x.LanguageCode)
        ))
        .ToList();

    if (activeLanguages.Count == 0)
    {
        throw new InvalidOperationException("Không tìm thấy ngôn ngữ active trong bảng languages.");
    }

    var manualSource = _translationSourceDAO.GetByCode("Manual")
        ?? throw new InvalidOperationException("Missing translation source: Manual.");

    var aiSource =
        _translationSourceDAO.GetByCode("AI")
        ?? _translationSourceDAO.GetByCode("GoogleTranslate")
        ?? manualSource;

    var narrations = request.IncludeInactive
        ? _dao.GetAll()
        : _dao.GetActive();

    foreach (var narration in narrations.OrderBy(x => x.NarrationId))
    {
        result.NarrationsScanned++;

        if (result.NarrationsProcessed >= maxItems)
        {
            break;
        }

        try
        {
            var existingTranslations = _translationDAO.GetByNarrationId(narration.NarrationId);

            var missingLanguages = activeLanguages
                .Where(language => !existingTranslations.Any(t => t.LanguageId == language.LanguageId))
                .ToList();

            if (missingLanguages.Count == 0)
            {
                result.TranslationsSkipped++;
                continue;
            }

            NarrationTranslationDTO? FindExistingTranslationByCode(string code)
            {
                var language = activeLanguages.FirstOrDefault(x =>
                    AutoTranslationBUS.NormalizeCode(x.LanguageCode) == code
                );

                if (language == null) return null;

                return existingTranslations.FirstOrDefault(x => x.LanguageId == language.LanguageId);
            }

            var sourceTranslation =
                FindExistingTranslationByCode(fallbackSourceCode)
                ?? FindExistingTranslationByCode("vi")
                ?? FindExistingTranslationByCode("en");

            var sourceLanguageCode = fallbackSourceCode;

            if (sourceTranslation != null)
            {
                var sourceLanguage = activeLanguages.FirstOrDefault(x =>
                    x.LanguageId == sourceTranslation.LanguageId
                );

                if (sourceLanguage != null)
                {
                    sourceLanguageCode = AutoTranslationBUS.NormalizeCode(sourceLanguage.LanguageCode);
                }
            }

            var sourceTitle = !string.IsNullOrWhiteSpace(sourceTranslation?.TranslatedTitle)
                ? sourceTranslation.TranslatedTitle
                : narration.Title;

            var sourceText = !string.IsNullOrWhiteSpace(sourceTranslation?.TranslatedText)
                ? sourceTranslation.TranslatedText
                : narration.OriginalText;

            if (string.IsNullOrWhiteSpace(sourceTitle) || string.IsNullOrWhiteSpace(sourceText))
            {
                result.Errors.Add($"NarrationId {narration.NarrationId}: thiếu title hoặc originalText.");
                continue;
            }

            var preparedTranslations = new List<NarrationTranslationDTO>();

            foreach (var language in missingLanguages)
            {
                var targetCode = AutoTranslationBUS.NormalizeCode(language.LanguageCode);
                var isSourceLanguage = targetCode == sourceLanguageCode;

                var translatedTitle = isSourceLanguage
                    ? sourceTitle.Trim()
                    : await _autoTranslationBUS.TranslateAsync(sourceTitle, sourceLanguageCode, targetCode);

                var translatedText = isSourceLanguage
                    ? sourceText.Trim()
                    : await _autoTranslationBUS.TranslateAsync(sourceText, sourceLanguageCode, targetCode);

                preparedTranslations.Add(new NarrationTranslationDTO
                {
                    NarrationId = narration.NarrationId,
                    LanguageId = language.LanguageId,
                    TranslatedTitle = translatedTitle,
                    TranslatedText = translatedText,
                    TranslationSourceId = isSourceLanguage ? manualSource.Id : aiSource.Id,
                    ReviewedBy = null,
                    IsReviewed = isSourceLanguage
                });

                await Task.Delay(300);
            }

            foreach (var translation in preparedTranslations)
            {
                _translationDAO.Insert(translation);
                result.TranslationsCreated++;
            }

            result.NarrationsProcessed++;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"NarrationId {narration.NarrationId}: {ex.Message}");
        }
    }

    return result;
}
    public bool Update(NarrationContentDTO dto) { ValidateNarrationTarget(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public NarrationContentDTO? GetById(long id) => _dao.GetById(id);
    public List<NarrationContentDTO> GetAll() => _dao.GetAll();
    public List<NarrationContentDTO> GetActive() => _dao.GetActive();
    public List<NarrationContentDTO> GetByPlaceId(long placeId) => _dao.GetByPlaceId(placeId);
    public List<NarrationContentDTO> GetByDishId(long dishId) => _dao.GetByDishId(dishId);
    public NarrationContentDTO? FindNarrationForPlace(long placeId) => _dao.GetMainNarrationByPlaceId(placeId);
    public NarrationContentDTO? FindNarrationForDish(long dishId) => _dao.GetMainNarrationByDishId(dishId);

    private void ValidateNarrationTarget(NarrationContentDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(dto.Title))
        errors["title"] = "Title is required.";

    if (string.IsNullOrWhiteSpace(dto.OriginalText))
        errors["originalText"] = "Original Text is required.";

    if (dto.ContentTypeId <= 0)
        errors["contentTypeId"] = "Content Type is required.";

    if (dto.CreatedBy <= 0)
        errors["createdBy"] = "Created By Admin is required.";

    if (errors.Count == 0)
    {
        var contentType = _contentTypeDAO.GetById(dto.ContentTypeId);
        var code = contentType?.Code;

        if (string.IsNullOrWhiteSpace(code))
        {
            errors["contentTypeId"] = "Content Type is invalid.";
        }
        else if (code == "Place")
        {
            if (dto.PlaceId == null)
                errors["placeId"] = "Place is required for Place Narration.";

            if (dto.DishId != null)
                errors["dishId"] = "Dish must be empty for Place Narration.";
        }
        else if (code == "Dish")
        {
            if (dto.DishId == null)
                errors["dishId"] = "Dish is required for Dish Narration.";

            if (dto.PlaceId != null)
                errors["placeId"] = "Place must be empty for Dish Narration.";
        }
        else if (code == "General")
        {
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
    public NarrationTranslationBUS(NarrationTranslationDAO dao) => _dao = dao;
    public long Create(NarrationTranslationDTO dto) { ValidateTranslation(dto); if (_dao.IsTranslationExists(dto.NarrationId, dto.LanguageId)) throw new InvalidOperationException("Translation for this language already exists."); return _dao.Insert(dto); }
    public bool Update(NarrationTranslationDTO dto) { ValidateTranslation(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public NarrationTranslationDTO? GetById(long id) => _dao.GetById(id);
    public List<NarrationTranslationDTO> GetAll() => _dao.GetAll();
    public List<NarrationTranslationDTO> GetActive() => _dao.GetActive();
    public List<NarrationTranslationDTO> GetByNarrationId(long narrationId) => _dao.GetByNarrationId(narrationId);
    public NarrationTranslationDTO? GetByNarrationAndLanguage(long narrationId, long languageId) => _dao.GetByNarrationAndLanguage(narrationId, languageId);
    public bool ReviewTranslation(long translationId, long reviewerAdminId) => _dao.MarkAsReviewed(translationId, reviewerAdminId);
    public bool IsTranslationReady(long narrationId, long languageId) => _dao.GetByNarrationAndLanguage(narrationId, languageId)?.IsReviewed == true;
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

    if (dto.IsReviewed && dto.ReviewedBy == null)
        errors["isReviewed"] = "Reviewed translations require reviewer admin.";

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}}

public class AudioFileBUS : ICrudBUS<AudioFileDTO, long>
{
    private readonly AudioFileDAO _dao;
    private readonly NarrationTranslationDAO _translationDAO;
    public AudioFileBUS(AudioFileDAO dao, NarrationTranslationDAO translationDAO) { _dao = dao; _translationDAO = translationDAO; }
    public long Create(AudioFileDTO dto) { ValidateAudioFile(dto); return _dao.Insert(dto); }
    public bool Update(AudioFileDTO dto) { ValidateAudioFile(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public AudioFileDTO? GetById(long id) => _dao.GetById(id);
    public List<AudioFileDTO> GetAll() => _dao.GetAll();
    public List<AudioFileDTO> GetActive() => _dao.GetActive();
    public List<AudioFileDTO> GetByTranslationId(long translationId) => _dao.GetByTranslationId(translationId);
    public AudioFileDTO? GetPlayableAudio(long narrationId, long languageId)
    {
        var translation = _translationDAO.GetByNarrationAndLanguage(narrationId, languageId);
        return translation == null ? null : _dao.GetActiveAudioByTranslationId(translation.TranslationId);
    }
private static void ValidateAudioFile(AudioFileDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (dto.TranslationId <= 0)
        errors["translationId"] = "Translation is required.";

    if (string.IsNullOrWhiteSpace(dto.AudioUrl))
        errors["audioUrl"] = "Audio URL is required.";

    if (dto.DurationSeconds.HasValue && dto.DurationSeconds.Value < 0)
        errors["durationSeconds"] = "Duration Seconds must be greater than or equal to 0.";

    if (!string.IsNullOrWhiteSpace(dto.VoiceGender) &&
        dto.VoiceGender != "Male" &&
        dto.VoiceGender != "Female" &&
        dto.VoiceGender != "Neutral")
    {
        errors["voiceGender"] = "Voice Gender must be Male, Female, or Neutral.";
    }

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}}
