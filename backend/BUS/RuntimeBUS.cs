using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.BUS;

static class PaginationHelper
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize switch
        {
            < 1 => 20,
            > 100 => 100,
            _ => pageSize
        };

        return (normalizedPage, normalizedPageSize);
    }
}
public class GuestSessionBUS
{
    private readonly GuestSessionDAO _dao;
    private readonly SessionGenerator _sessionGenerator;
    public GuestSessionBUS(GuestSessionDAO dao, SessionGenerator sessionGenerator) { _dao = dao; _sessionGenerator = sessionGenerator; }
    public GuestSessionDTO CreateSession(string? deviceInfo, string? ipAddress, long? preferredLanguageId = null)
    {
        var dto = new GuestSessionDTO { GuestSessionId = _sessionGenerator.GenerateGuestSessionId(), DeviceInfo = deviceInfo, IPAddress = ipAddress, PreferredLanguageId = preferredLanguageId, IsActive = true };
        _dao.Insert(dto); return dto;
    }
    public GuestSessionDTO? GetById(string id) => _dao.GetById(id);
    public bool ChangePreferredLanguage(string id, long languageId) => _dao.UpdatePreferredLanguage(id, languageId);
    public bool Touch(string id) => _dao.UpdateLastSeen(id);
    public bool Deactivate(string id) => _dao.Deactivate(id);
    public bool CleanupExpiredSessions(DateTime beforeTime) { foreach (var s in _dao.GetExpiredSessions(beforeTime)) _dao.Deactivate(s.GuestSessionId); return true; }
    public List<GuestSessionDTO> GetActiveSessions() => _dao.GetActiveSessions();
}

public class GuestPoiStateBUS
{
    private readonly GuestPoiStateDAO _dao;
    public GuestPoiStateBUS(GuestPoiStateDAO dao) => _dao = dao;
    public GuestPoiStateDTO? GetState(string guestSessionId, long placeId) => _dao.GetState(guestSessionId, placeId);
    public List<GuestPoiStateDTO> GetByGuestSessionId(string guestSessionId) => _dao.GetByGuestSessionId(guestSessionId);
    public bool UpdateInsideState(string guestSessionId, long placeId, bool isInside, decimal distanceMeters) => _dao.UpdateInsideState(guestSessionId, placeId, isInside, distanceMeters);
    public bool UpdateCooldown(string guestSessionId, long placeId, DateTime cooldownUntil) => _dao.UpdateCooldown(guestSessionId, placeId, cooldownUntil);
    public bool CleanupByGuestSessionId(string guestSessionId) => _dao.DeleteByGuestSessionId(guestSessionId);
}


public class GeofenceBUS
{
    private readonly PlaceDAO _placeDAO;
    private readonly GuestPoiStateDAO _stateDAO;
    private readonly GeofenceEventDAO _eventDAO;
    private readonly GeofenceEventTypeDAO _eventTypeDAO;
    private readonly GeofenceEventStatusDAO _eventStatusDAO;
    private readonly PublicNarrationBUS _publicNarrationBUS;
    private readonly GeoDistanceCalculator _distanceCalculator;

    public GeofenceBUS(
        PlaceDAO placeDAO,
        GuestPoiStateDAO stateDAO,
        GeofenceEventDAO eventDAO,
        GeofenceEventTypeDAO eventTypeDAO,
        GeofenceEventStatusDAO eventStatusDAO,
        PublicNarrationBUS publicNarrationBUS,
        GeoDistanceCalculator distanceCalculator)
    {
        _placeDAO = placeDAO;
        _stateDAO = stateDAO;
        _eventDAO = eventDAO;
        _eventTypeDAO = eventTypeDAO;
        _eventStatusDAO = eventStatusDAO;
        _publicNarrationBUS = publicNarrationBUS;
        _distanceCalculator = distanceCalculator;
    }

    public PagedResultDTO<GeofenceEventDTO> GetPaged(int page, int pageSize)
    {
        var normalized = PaginationHelper.Normalize(page, pageSize);
        return new PagedResultDTO<GeofenceEventDTO>
        {
            Items = _eventDAO.GetPaged(normalized.Page, normalized.PageSize),
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            TotalItems = _eventDAO.CountAll()
        };
    }

    public GeofenceCheckResultDTO CheckLocation(
        string guestSessionId,
        decimal latitude,
        decimal longitude,
        long languageId)
    {
        var place = FindBestNearbyPlace(latitude, longitude);
        if (place == null)
            return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "No nearby POI." };

        var distance = _distanceCalculator.CalculateDistanceMeters(
            latitude,
            longitude,
            place.Latitude!.Value,
            place.Longitude!.Value);

        var state = _stateDAO.GetState(guestSessionId, place.PlaceId);
        _stateDAO.UpdateInsideState(guestSessionId, place.PlaceId, true, distance);

        if (IsDebounced(state, place))
        {
            CreateEvent(guestSessionId, place.PlaceId, null, "Near", "IgnoredDebounce",
                latitude, longitude, distance, "Ignored by debounce.");
            return new GeofenceCheckResultDTO
            {
                ShouldPlay = false,
                Reason = "IgnoredDebounce",
                PlaceId = place.PlaceId,
                DistanceMeters = distance
            };
        }

        if (IsInCooldown(state))
        {
            CreateEvent(guestSessionId, place.PlaceId, null, "Near", "IgnoredCooldown",
                latitude, longitude, distance, "Ignored by cooldown.");
            return new GeofenceCheckResultDTO
            {
                ShouldPlay = false,
                Reason = "IgnoredCooldown",
                PlaceId = place.PlaceId,
                DistanceMeters = distance
            };
        }

        PublicNarrationResultDTO narration;
        try
        {
            narration = _publicNarrationBUS.ResolvePlace(place.PlaceId, languageId);
        }
        catch (InvalidOperationException ex)
        {
            return new GeofenceCheckResultDTO
            {
                ShouldPlay = false,
                Reason = ex.Message,
                PlaceId = place.PlaceId,
                DistanceMeters = distance
            };
        }

        var geofenceEvent = CreateEvent(
            guestSessionId,
            place.PlaceId,
            narration.NarrationId,
            "Near",
            "Played",
            latitude,
            longitude,
            distance,
            "Narration opened by geofence. Playback history is created by the player.");

        UpdatePoiStateAfterTrigger(guestSessionId, place, distance);

        return new GeofenceCheckResultDTO
        {
            ShouldPlay = true,
            Reason = "Ready",
            PlaceId = place.PlaceId,
            NarrationId = narration.NarrationId,
            TranslationId = narration.TranslationId,
            AudioId = narration.AudioId,
            GeofenceEventId = geofenceEvent.EventId,
            Title = narration.Title,
            Text = narration.Text,
            AudioUrl = narration.AudioUrl,
            DistanceMeters = distance
        };
    }

    public GeofenceEventDTO CreateEvent(
        string guestSessionId,
        long placeId,
        long? narrationId,
        string eventTypeCode,
        string eventStatusCode,
        decimal latitude,
        decimal longitude,
        decimal distance,
        string? note)
    {
        var typeId = _eventTypeDAO.GetByCode(eventTypeCode)?.Id
            ?? throw new InvalidOperationException($"Event type '{eventTypeCode}' not found.");
        var statusId = _eventStatusDAO.GetByCode(eventStatusCode)?.Id
            ?? throw new InvalidOperationException($"Event status '{eventStatusCode}' not found.");
        var dto = new GeofenceEventDTO
        {
            GuestSessionId = guestSessionId,
            PlaceId = placeId,
            NarrationId = narrationId,
            EventTypeId = typeId,
            EventStatusId = statusId,
            UserLatitude = latitude,
            UserLongitude = longitude,
            DistanceMeters = distance,
            Note = note
        };
        dto.EventId = _eventDAO.Insert(dto);
        return dto;
    }

    public GeofenceEventDTO? GetById(long eventId) => _eventDAO.GetById(eventId);
    public List<GeofenceEventDTO> GetAll() => _eventDAO.GetAll();
    public List<GeofenceEventDTO> GetByGuestSessionId(string guestSessionId) => _eventDAO.GetByGuestSessionId(guestSessionId);
    public List<GeofenceEventDTO> GetByPlaceId(long placeId) => _eventDAO.GetByPlaceId(placeId);
    public List<GeofenceEventDTO> GetByDateRange(DateTime from, DateTime to) => _eventDAO.GetByDateRange(from, to);
    public bool UpdateStatus(long eventId, long eventStatusId) => _eventDAO.UpdateStatus(eventId, eventStatusId);

    private PlaceDTO? FindBestNearbyPlace(decimal latitude, decimal longitude)
    {
        var candidate = _placeDAO.GetPoiEnabledPlaces()
            .Where(p => p.Latitude != null && p.Longitude != null)
            .Select(p => new
            {
                Place = p,
                Distance = _distanceCalculator.CalculateDistanceMeters(
                    latitude,
                    longitude,
                    p.Latitude!.Value,
                    p.Longitude!.Value)
            })
            .Where(x => x.Distance <= x.Place.TriggerRadiusMeters)
            .OrderByDescending(x => x.Place.Priority)
            .ThenBy(x => x.Distance)
            .FirstOrDefault();
        return candidate?.Place;
    }

    private static bool IsInCooldown(GuestPoiStateDTO? state) =>
        state?.CooldownUntil != null && state.CooldownUntil.Value > DateTime.UtcNow;

    private static bool IsDebounced(GuestPoiStateDTO? state, PlaceDTO place) =>
        state?.LastTriggeredAt != null
        && state.LastTriggeredAt.Value.AddSeconds(place.DebounceSeconds) > DateTime.UtcNow;

    private void UpdatePoiStateAfterTrigger(string guestSessionId, PlaceDTO place, decimal distance)
    {
        _stateDAO.Upsert(new GuestPoiStateDTO
        {
            GuestSessionId = guestSessionId,
            PlaceId = place.PlaceId,
            IsInside = true,
            LastEnteredAt = DateTime.UtcNow,
            LastTriggeredAt = DateTime.UtcNow,
            CooldownUntil = DateTime.UtcNow.AddSeconds(place.CooldownSeconds),
            LastDistanceMeters = distance
        });
    }
}

public class ListeningHistoryBUS
{
    private readonly ListeningHistoryDAO _dao;
    public ListeningHistoryBUS(ListeningHistoryDAO dao) => _dao = dao;
    public long Create(ListeningHistoryDTO dto) => _dao.Insert(dto);
    public ListeningHistoryDTO? GetById(long id) => _dao.GetById(id);
    public List<ListeningHistoryDTO> GetAll() => _dao.GetAll();
    public List<ListeningHistoryDTO> GetByGuestSessionId(string guestSessionId) => _dao.GetByGuestSessionId(guestSessionId);
    public List<ListeningHistoryDTO> GetByNarrationId(long narrationId) => _dao.GetByNarrationId(narrationId);
    public List<ListeningHistoryDTO> GetByDateRange(DateTime from, DateTime to) => _dao.GetByDateRange(from, to);
    public bool UpdatePlaybackStatus(long id, string status) => _dao.UpdatePlaybackStatus(id, status);
    public bool UpdateListenDuration(long id, int seconds) => _dao.UpdateListenDuration(id, seconds);

    public PagedResultDTO<ListeningHistoryDTO> GetPaged(int page, int pageSize)
{
    var normalized = PaginationHelper.Normalize(page, pageSize);

    return new PagedResultDTO<ListeningHistoryDTO>
    {
        Items = _dao.GetPaged(normalized.Page, normalized.PageSize),
        Page = normalized.Page,
        PageSize = normalized.PageSize,
        TotalItems = _dao.CountAll()
    };
}
}

public class FeedbackBUS
{
    private readonly FeedbackDAO _dao;
    public FeedbackBUS(FeedbackDAO dao) => _dao = dao;
    public long CreateFeedback(FeedbackDTO dto) { ValidateFeedbackTarget(dto); ValidateRating(dto.Rating); return _dao.Insert(dto); }
    public FeedbackDTO? GetById(long id) => _dao.GetById(id);
    public List<FeedbackDTO> GetAll() => _dao.GetAll();
    public List<FeedbackDTO> GetApproved() => _dao.GetApproved();
    public List<FeedbackDTO> GetPending() => _dao.GetPending();
    public bool Approve(long id) => _dao.Approve(id);
    public bool Reject(long id) => _dao.Reject(id);
    public List<FeedbackDTO> GetByPlaceId(long placeId) => _dao.GetByPlaceId(placeId);
    public List<FeedbackDTO> GetByDishId(long dishId) => _dao.GetByDishId(dishId);
    public List<FeedbackDTO> GetByNarrationId(long narrationId) => _dao.GetByNarrationId(narrationId);
    private static void ValidateRating(int rating) { if (!ValidationHelper.IsValidRating(rating)) throw new ArgumentException("Rating must be between 1 and 5."); }
    private static void ValidateFeedbackTarget(FeedbackDTO dto)
    {
        var count = new[] { dto.PlaceId, dto.DishId, dto.NarrationId }.Count(x => x != null);
        if (count != 1) throw new ArgumentException("Feedback must target exactly one object: Place, Dish, or Narration.");
    }

    public PagedResultDTO<FeedbackDTO> GetPaged(int page, int pageSize)
{
    var normalized = PaginationHelper.Normalize(page, pageSize);

    return new PagedResultDTO<FeedbackDTO>
    {
        Items = _dao.GetPaged(normalized.Page, normalized.PageSize),
        Page = normalized.Page,
        PageSize = normalized.PageSize,
        TotalItems = _dao.CountAll()
    };
}
}
