using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO.Lookup;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/place-types")]
public class PlaceTypesController : LookupControllerBase<PlaceTypeDTO>
{
    public PlaceTypesController(PlaceTypeBUS bus) : base(bus) { }
}

[Route("api/content-types")]
public class ContentTypesController : LookupControllerBase<ContentTypeDTO>
{
    public ContentTypesController(ContentTypeBUS bus) : base(bus) { }
}

[Route("api/target-types")]
public class TargetTypesController : LookupControllerBase<TargetTypeDTO>
{
    public TargetTypesController(TargetTypeBUS bus) : base(bus) { }
}

[Route("api/translation-sources")]
public class TranslationSourcesController : LookupControllerBase<TranslationSourceDTO>
{
    public TranslationSourcesController(TranslationSourceBUS bus) : base(bus) { }
}

[Route("api/trigger-modes")]
public class TriggerModesController : LookupControllerBase<TriggerModeDTO>
{
    public TriggerModesController(TriggerModeBUS bus) : base(bus) { }
}

[Route("api/geofence-event-types")]
public class GeofenceEventTypesController : LookupControllerBase<GeofenceEventTypeDTO>
{
    public GeofenceEventTypesController(GeofenceEventTypeBUS bus) : base(bus) { }
}

[Route("api/geofence-event-statuses")]
public class GeofenceEventStatusesController : LookupControllerBase<GeofenceEventStatusDTO>
{
    public GeofenceEventStatusesController(GeofenceEventStatusBUS bus) : base(bus) { }
}

