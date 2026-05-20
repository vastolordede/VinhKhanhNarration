using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.BUS;

public class LanguageBUS : ICrudBUS<LanguageDTO, long>
{
    private readonly LanguageDAO _dao;
    public LanguageBUS(LanguageDAO dao) => _dao = dao;

    public long Create(LanguageDTO dto)
    {
        ValidateBeforeSave(dto);
        if (_dao.IsCodeExists(dto.LanguageCode)) throw new InvalidOperationException("Language code already exists.");
        var id = _dao.Insert(dto);
        if (dto.IsDefault) _dao.SetDefault(id);
        return id;
    }

    public bool Update(LanguageDTO dto)
    {
        ValidateBeforeSave(dto);
        var result = _dao.Update(dto);
        if (dto.IsDefault) _dao.SetDefault(dto.LanguageId);
        return result;
    }

    public bool Deactivate(long id)
    {
        var lang = _dao.GetById(id) ?? throw new InvalidOperationException("Language not found.");
        if (lang.IsDefault) throw new InvalidOperationException("Cannot deactivate default language.");
        return _dao.SoftDelete(id);
    }

    public bool Restore(long id) => _dao.Restore(id);
    public LanguageDTO? GetById(long id) => _dao.GetById(id);
    public LanguageDTO? GetByCode(string code) => _dao.GetByCode(code);
    public LanguageDTO? GetDefault() => _dao.GetDefault();
    public List<LanguageDTO> GetAll() => _dao.GetAll();
    public List<LanguageDTO> GetActive() => _dao.GetActive();
    public bool SetDefaultLanguage(long languageId) => _dao.SetDefault(languageId);

    private static void ValidateBeforeSave(LanguageDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LanguageCode)) throw new ArgumentException("LanguageCode is required.");
        if (string.IsNullOrWhiteSpace(dto.LanguageName)) throw new ArgumentException("LanguageName is required.");
    }
}
