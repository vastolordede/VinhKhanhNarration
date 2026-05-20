using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO.Lookup;

namespace VinhKhanhNarration.Api.BUS;

public abstract class LookupBUS<TDto> where TDto : LookupDTO, new()
{
    protected readonly LookupDAO<TDto> Dao;

    protected LookupBUS(LookupDAO<TDto> dao) => Dao = dao;

    public long Create(TDto dto)
    {
        ValidateLookup(dto);
        if (Dao.IsCodeExists(dto.Code)) throw new InvalidOperationException("Code already exists.");
        return Dao.Insert(dto);
    }

    public bool Update(TDto dto)
    {
        ValidateLookup(dto);
        return Dao.Update(dto);
    }

    public bool Deactivate(long id) => Dao.SoftDelete(id);
    public bool Restore(long id) => Dao.Restore(id);
    public TDto? GetById(long id) => Dao.GetById(id);
    public TDto? GetByCode(string code) => Dao.GetByCode(code);
    public List<TDto> GetAll() => Dao.GetAll();
    public List<TDto> GetActive() => Dao.GetActive();

    protected static void ValidateLookup(TDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required.");
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required.");
    }
}

public class PlaceTypeBUS : LookupBUS<PlaceTypeDTO> { public PlaceTypeBUS(PlaceTypeDAO dao) : base(dao) { } }
public class ContentTypeBUS : LookupBUS<ContentTypeDTO> { public ContentTypeBUS(ContentTypeDAO dao) : base(dao) { } }
public class TargetTypeBUS : LookupBUS<TargetTypeDTO> { public TargetTypeBUS(TargetTypeDAO dao) : base(dao) { } }
public class TranslationSourceBUS : LookupBUS<TranslationSourceDTO> { public TranslationSourceBUS(TranslationSourceDAO dao) : base(dao) { } }
public class TriggerModeBUS : LookupBUS<TriggerModeDTO> { public TriggerModeBUS(TriggerModeDAO dao) : base(dao) { } }
public class GeofenceEventTypeBUS : LookupBUS<GeofenceEventTypeDTO> { public GeofenceEventTypeBUS(GeofenceEventTypeDAO dao) : base(dao) { } }
public class GeofenceEventStatusBUS : LookupBUS<GeofenceEventStatusDTO> { public GeofenceEventStatusBUS(GeofenceEventStatusDAO dao) : base(dao) { } }
