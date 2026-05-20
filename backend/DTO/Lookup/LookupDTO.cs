namespace VinhKhanhNarration.Api.DTO.Lookup;

public abstract class LookupDTO
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PlaceTypeDTO : LookupDTO { }
public class ContentTypeDTO : LookupDTO { }
public class TargetTypeDTO : LookupDTO { }
public class TranslationSourceDTO : LookupDTO { }
public class TriggerModeDTO : LookupDTO { }
public class GeofenceEventTypeDTO : LookupDTO { }
public class GeofenceEventStatusDTO : LookupDTO { }
