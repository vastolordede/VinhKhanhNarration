using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;

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
        if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(dto.OriginalText)) throw new ArgumentException("OriginalText is required.");
        if (dto.ContentTypeId <= 0) throw new ArgumentException("ContentTypeId is required.");
        if (dto.CreatedBy <= 0) throw new ArgumentException("CreatedBy is required.");

        var contentType = _contentTypeDAO.GetById(dto.ContentTypeId);
        var code = contentType?.Code;
        if (code == "Place" && (dto.PlaceId == null || dto.DishId != null)) throw new ArgumentException("Place content requires PlaceId only.");
        if (code == "Dish" && (dto.DishId == null || dto.PlaceId != null)) throw new ArgumentException("Dish content requires DishId only.");
        if (code == "General" && (dto.PlaceId != null || dto.DishId != null)) throw new ArgumentException("General content must not target Place or Dish.");
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
    private static void ValidateTranslation(NarrationTranslationDTO dto) { if (dto.NarrationId <= 0) throw new ArgumentException("NarrationId is required."); if (dto.LanguageId <= 0) throw new ArgumentException("LanguageId is required."); if (dto.TranslationSourceId <= 0) throw new ArgumentException("TranslationSourceId is required."); if (string.IsNullOrWhiteSpace(dto.TranslatedTitle)) throw new ArgumentException("TranslatedTitle is required."); if (string.IsNullOrWhiteSpace(dto.TranslatedText)) throw new ArgumentException("TranslatedText is required."); }
}

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
    private static void ValidateAudioFile(AudioFileDTO dto) { if (dto.TranslationId <= 0) throw new ArgumentException("TranslationId is required."); if (string.IsNullOrWhiteSpace(dto.AudioUrl)) throw new ArgumentException("AudioUrl is required."); if (dto.DurationSeconds.HasValue && dto.DurationSeconds.Value < 0) throw new ArgumentException("DurationSeconds must be >= 0."); }
}
