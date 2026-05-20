using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.BUS;

public class QRCodeBUS : ICrudBUS<QRCodeDTO, long>
{
    private readonly QRCodeDAO _dao;
    private readonly TargetTypeDAO _targetTypeDAO;
    private readonly NarrationContentDAO _narrationDAO;
    private readonly NarrationTranslationDAO _translationDAO;
    private readonly AudioFileDAO _audioDAO;
    private readonly ListeningHistoryDAO _historyDAO;

    public QRCodeBUS(QRCodeDAO dao, TargetTypeDAO targetTypeDAO, NarrationContentDAO narrationDAO, NarrationTranslationDAO translationDAO, AudioFileDAO audioDAO, ListeningHistoryDAO historyDAO)
    {
        _dao = dao; _targetTypeDAO = targetTypeDAO; _narrationDAO = narrationDAO; _translationDAO = translationDAO; _audioDAO = audioDAO; _historyDAO = historyDAO;
    }

    public long Create(QRCodeDTO dto) { ValidateQRCodeTarget(dto); if (_dao.IsQRCodeValueExists(dto.QRCodeValue)) throw new InvalidOperationException("QR code value already exists."); return _dao.Insert(dto); }
    public bool Update(QRCodeDTO dto) { ValidateQRCodeTarget(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public QRCodeDTO? GetById(long id) => _dao.GetById(id);
    public QRCodeDTO? GetByValue(string value) => _dao.GetByValue(value);
    public List<QRCodeDTO> GetAll() => _dao.GetAll();
    public List<QRCodeDTO> GetActive() => _dao.GetActive();

    public QRScanResultDTO ResolveQRCode(string qrCodeValue, long languageId, string guestSessionId)
    {
        var qr = _dao.GetByValue(qrCodeValue) ?? throw new InvalidOperationException("QR code not found.");
        if (!qr.IsActive) throw new InvalidOperationException("QR code is inactive.");

        var targetType = _targetTypeDAO.GetById(qr.TargetTypeId)?.Code;
        NarrationContentDTO? narration = targetType switch
        {
            "Place" => qr.PlaceId == null ? null : _narrationDAO.GetMainNarrationByPlaceId(qr.PlaceId.Value),
            "Dish" => qr.DishId == null ? null : _narrationDAO.GetMainNarrationByDishId(qr.DishId.Value),
            "Narration" => qr.NarrationId == null ? null : _narrationDAO.GetById(qr.NarrationId.Value),
            _ => null
        };

        if (narration == null) throw new InvalidOperationException("Narration not found for QR target.");
        var translation = _translationDAO.GetByNarrationAndLanguage(narration.NarrationId, languageId) ?? throw new InvalidOperationException("Translation not found for selected language.");
        var audio = _audioDAO.GetActiveAudioByTranslationId(translation.TranslationId) ?? throw new InvalidOperationException("Playable audio not found.");

        _historyDAO.Insert(new ListeningHistoryDTO
        {
            GuestSessionId = guestSessionId,
            NarrationId = narration.NarrationId,
            LanguageId = languageId,
            AudioId = audio.AudioId,
            QRCodeId = qr.QRCodeId,
            TriggerSource = "QR",
            PlaybackStatus = "Played"
        });

        return new QRScanResultDTO { PlaceId = qr.PlaceId, DishId = qr.DishId, NarrationId = narration.NarrationId, TranslationId = translation.TranslationId, AudioId = audio.AudioId, Title = translation.TranslatedTitle, Text = translation.TranslatedText, AudioUrl = audio.AudioUrl };
    }

    public string GenerateQRCodeValue(string targetPrefix, long targetId) => $"{targetPrefix}-{targetId}-{Guid.NewGuid():N}";

    private void ValidateQRCodeTarget(QRCodeDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.QRCodeValue)) throw new ArgumentException("QRCodeValue is required.");
        var type = _targetTypeDAO.GetById(dto.TargetTypeId)?.Code;
        if (type == "Place" && (dto.PlaceId == null || dto.DishId != null || dto.NarrationId != null)) throw new ArgumentException("Place QR requires PlaceId only.");
        if (type == "Dish" && (dto.DishId == null || dto.PlaceId != null || dto.NarrationId != null)) throw new ArgumentException("Dish QR requires DishId only.");
        if (type == "Narration" && (dto.NarrationId == null || dto.PlaceId != null || dto.DishId != null)) throw new ArgumentException("Narration QR requires NarrationId only.");
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
    private readonly NarrationContentDAO _narrationDAO;
    private readonly NarrationTranslationDAO _translationDAO;
    private readonly AudioFileDAO _audioDAO;
    private readonly ListeningHistoryDAO _historyDAO;
    private readonly GeoDistanceCalculator _distanceCalculator;

    public GeofenceBUS(PlaceDAO placeDAO, GuestPoiStateDAO stateDAO, GeofenceEventDAO eventDAO, GeofenceEventTypeDAO eventTypeDAO, GeofenceEventStatusDAO eventStatusDAO, NarrationContentDAO narrationDAO, NarrationTranslationDAO translationDAO, AudioFileDAO audioDAO, ListeningHistoryDAO historyDAO, GeoDistanceCalculator distanceCalculator)
    {
        _placeDAO = placeDAO; _stateDAO = stateDAO; _eventDAO = eventDAO; _eventTypeDAO = eventTypeDAO; _eventStatusDAO = eventStatusDAO; _narrationDAO = narrationDAO; _translationDAO = translationDAO; _audioDAO = audioDAO; _historyDAO = historyDAO; _distanceCalculator = distanceCalculator;
    }

    public GeofenceCheckResultDTO CheckLocation(string guestSessionId, decimal latitude, decimal longitude, long languageId)
    {
        var place = FindBestNearbyPlace(latitude, longitude);
        if (place == null) return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "No nearby POI." };

        var distance = _distanceCalculator.CalculateDistanceMeters(latitude, longitude, place.Latitude!.Value, place.Longitude!.Value);
        var state = _stateDAO.GetState(guestSessionId, place.PlaceId);
        _stateDAO.UpdateInsideState(guestSessionId, place.PlaceId, true, distance);

        if (IsDebounced(state, place))
        {
            CreateEvent(guestSessionId, place.PlaceId, null, "Near", "IgnoredDebounce", latitude, longitude, distance, "Ignored by debounce.");
            return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "IgnoredDebounce", PlaceId = place.PlaceId, DistanceMeters = distance };
        }

        if (IsInCooldown(state))
        {
            CreateEvent(guestSessionId, place.PlaceId, null, "Near", "IgnoredCooldown", latitude, longitude, distance, "Ignored by cooldown.");
            return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "IgnoredCooldown", PlaceId = place.PlaceId, DistanceMeters = distance };
        }

        var narration = _narrationDAO.GetMainNarrationByPlaceId(place.PlaceId);
        if (narration == null) return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "Narration not found.", PlaceId = place.PlaceId, DistanceMeters = distance };
        var translation = _translationDAO.GetByNarrationAndLanguage(narration.NarrationId, languageId);
        if (translation == null) return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "Translation not found.", PlaceId = place.PlaceId, NarrationId = narration.NarrationId, DistanceMeters = distance };
        var audio = _audioDAO.GetActiveAudioByTranslationId(translation.TranslationId);
        if (audio == null) return new GeofenceCheckResultDTO { ShouldPlay = false, Reason = "Audio not found.", PlaceId = place.PlaceId, NarrationId = narration.NarrationId, TranslationId = translation.TranslationId, DistanceMeters = distance };

        var geofenceEvent = CreateEvent(guestSessionId, place.PlaceId, narration.NarrationId, "Near", "Played", latitude, longitude, distance, "Auto played by geofence.");
        UpdatePoiStateAfterTrigger(guestSessionId, place, distance);

        _historyDAO.Insert(new ListeningHistoryDTO
        {
            GuestSessionId = guestSessionId,
            NarrationId = narration.NarrationId,
            LanguageId = languageId,
            AudioId = audio.AudioId,
            GeofenceEventId = geofenceEvent.EventId,
            TriggerSource = "Geofence",
            PlaybackStatus = "Played"
        });

        return new GeofenceCheckResultDTO { ShouldPlay = true, Reason = "Played", PlaceId = place.PlaceId, NarrationId = narration.NarrationId, TranslationId = translation.TranslationId, AudioId = audio.AudioId, AudioUrl = audio.AudioUrl, DistanceMeters = distance };
    }

    public GeofenceEventDTO CreateEvent(string guestSessionId, long placeId, long? narrationId, string eventTypeCode, string eventStatusCode, decimal latitude, decimal longitude, decimal distance, string? note)
    {
        var typeId = _eventTypeDAO.GetByCode(eventTypeCode)?.Id ?? throw new InvalidOperationException($"Event type '{eventTypeCode}' not found.");
        var statusId = _eventStatusDAO.GetByCode(eventStatusCode)?.Id ?? throw new InvalidOperationException($"Event status '{eventStatusCode}' not found.");
        var dto = new GeofenceEventDTO { GuestSessionId = guestSessionId, PlaceId = placeId, NarrationId = narrationId, EventTypeId = typeId, EventStatusId = statusId, UserLatitude = latitude, UserLongitude = longitude, DistanceMeters = distance, Note = note };
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
        var candidates = _placeDAO.GetPoiEnabledPlaces()
            .Where(p => p.Latitude != null && p.Longitude != null)
            .Select(p => new { Place = p, Distance = _distanceCalculator.CalculateDistanceMeters(latitude, longitude, p.Latitude!.Value, p.Longitude!.Value) })
            .Where(x => x.Distance <= x.Place.TriggerRadiusMeters)
            .OrderByDescending(x => x.Place.Priority)
            .ThenBy(x => x.Distance)
            .FirstOrDefault();
        return candidates?.Place;
    }

    private static bool IsInCooldown(GuestPoiStateDTO? state)
    {
        return state?.CooldownUntil != null && state.CooldownUntil.Value > DateTime.UtcNow;
    }

    private static bool IsDebounced(GuestPoiStateDTO? state, PlaceDTO place)
    {
        if (state?.LastTriggeredAt == null) return false;
        return state.LastTriggeredAt.Value.AddSeconds(place.DebounceSeconds) > DateTime.UtcNow;
    }

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
}
