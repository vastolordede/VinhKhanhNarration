using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.BUS;

public class NarrationContentBUS : ICrudBUS<NarrationContentDTO, long>
{
    private readonly NarrationContentDAO _dao;
    private readonly ContentTypeDAO _contentTypeDAO;

    public NarrationContentBUS(NarrationContentDAO dao, ContentTypeDAO contentTypeDAO)
    {
        _dao = dao;
        _contentTypeDAO = contentTypeDAO;
    }

    public long Create(NarrationContentDTO dto) { ValidateNarrationTarget(dto); return _dao.Insert(dto); }
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
