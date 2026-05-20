using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class QRCodeDAO : GenericCrudDAO<QRCodeDTO>
{
    public QRCodeDAO(DbConnectionFactory factory) : base(factory) { }

    public QRCodeDTO? GetByValue(string qrCodeValue)
    {
        return QuerySingle("SELECT * FROM qr_codes WHERE qr_code_value = @value LIMIT 1;", cmd => cmd.Parameters.AddWithValue("@value", qrCodeValue));
    }

    public List<QRCodeDTO> GetByPlaceId(long placeId) => QueryList("SELECT * FROM qr_codes WHERE place_id = @id ORDER BY qr_code_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", placeId));
    public List<QRCodeDTO> GetByDishId(long dishId) => QueryList("SELECT * FROM qr_codes WHERE dish_id = @id ORDER BY qr_code_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", dishId));
    public List<QRCodeDTO> GetByNarrationId(long narrationId) => QueryList("SELECT * FROM qr_codes WHERE narration_id = @id ORDER BY qr_code_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", narrationId));

    public bool IsQRCodeValueExists(string qrCodeValue)
    {
        return Exists("SELECT COUNT(1) FROM qr_codes WHERE qr_code_value = @value;", cmd => cmd.Parameters.AddWithValue("@value", qrCodeValue));
    }
}

public class GuestSessionDAO : BaseDAO
{
    public GuestSessionDAO(DbConnectionFactory factory) : base(factory) { }

    public bool Insert(GuestSessionDTO dto)
    {
        const string sql = @"INSERT INTO guest_sessions (guest_session_id, preferred_language_id, device_info, ip_address, is_active)
                             VALUES (@id, @lang, @device, @ip, @active);";
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", dto.GuestSessionId);
        cmd.Parameters.AddWithValue("@lang", DbValue(dto.PreferredLanguageId));
        cmd.Parameters.AddWithValue("@device", DbValue(dto.DeviceInfo));
        cmd.Parameters.AddWithValue("@ip", DbValue(dto.IPAddress));
        cmd.Parameters.AddWithValue("@active", dto.IsActive);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdatePreferredLanguage(string guestSessionId, long languageId)
    {
        return Execute("UPDATE guest_sessions SET preferred_language_id = @languageId, last_seen_at = CURRENT_TIMESTAMP WHERE guest_session_id = @id;", cmd =>
        {
            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@id", guestSessionId);
        });
    }

    public bool UpdateLastSeen(string guestSessionId)
    {
        return Execute("UPDATE guest_sessions SET last_seen_at = CURRENT_TIMESTAMP WHERE guest_session_id = @id;", cmd => cmd.Parameters.AddWithValue("@id", guestSessionId));
    }

    public bool Deactivate(string guestSessionId)
    {
        return Execute("UPDATE guest_sessions SET is_active = FALSE WHERE guest_session_id = @id;", cmd => cmd.Parameters.AddWithValue("@id", guestSessionId));
    }

    public GuestSessionDTO? GetById(string guestSessionId)
    {
        return QuerySingle("SELECT * FROM guest_sessions WHERE guest_session_id = @id;", cmd => cmd.Parameters.AddWithValue("@id", guestSessionId));
    }

    public List<GuestSessionDTO> GetActiveSessions() => QueryList("SELECT * FROM guest_sessions WHERE is_active = TRUE ORDER BY last_seen_at DESC;");
    public List<GuestSessionDTO> GetExpiredSessions(DateTime beforeTime) => QueryList("SELECT * FROM guest_sessions WHERE last_seen_at < @before ORDER BY last_seen_at;", cmd => cmd.Parameters.AddWithValue("@before", beforeTime));

    private bool Execute(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        return cmd.ExecuteNonQuery() > 0;
    }

    private GuestSessionDTO? QuerySingle(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private List<GuestSessionDTO> QueryList(string sql, Action<NpgsqlCommand>? bind = null)
    {
        var list = new List<GuestSessionDTO>();
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind?.Invoke(cmd);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    private static GuestSessionDTO Map(NpgsqlDataReader reader) => new()
    {
        GuestSessionId = Convert.ToString(reader["guest_session_id"]) ?? string.Empty,
        PreferredLanguageId = reader["preferred_language_id"] == DBNull.Value ? null : Convert.ToInt64(reader["preferred_language_id"]),
        DeviceInfo = reader["device_info"] == DBNull.Value ? null : Convert.ToString(reader["device_info"]),
        IPAddress = reader["ip_address"] == DBNull.Value ? null : Convert.ToString(reader["ip_address"]),
        CreatedAt = Convert.ToDateTime(reader["created_at"]),
        LastSeenAt = Convert.ToDateTime(reader["last_seen_at"]),
        IsActive = Convert.ToBoolean(reader["is_active"])
    };
}


public class GuestPoiStateDAO : BaseDAO
{
    public GuestPoiStateDAO(DbConnectionFactory factory) : base(factory) { }

    public bool Upsert(GuestPoiStateDTO dto)
    {
        const string sql = @"
            INSERT INTO guest_poi_states
            (guest_session_id, place_id, is_inside, last_entered_at, last_exited_at, last_triggered_at, cooldown_until, last_distance_meters)
            VALUES (@guest, @place, @inside, @entered, @exited, @triggered, @cooldown, @distance)
            ON CONFLICT (guest_session_id, place_id)
            DO UPDATE SET
                is_inside = EXCLUDED.is_inside,
                last_entered_at = EXCLUDED.last_entered_at,
                last_exited_at = EXCLUDED.last_exited_at,
                last_triggered_at = EXCLUDED.last_triggered_at,
                cooldown_until = EXCLUDED.cooldown_until,
                last_distance_meters = EXCLUDED.last_distance_meters;";
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        Bind(cmd, dto);
        return cmd.ExecuteNonQuery() > 0;
    }

    public GuestPoiStateDTO? GetState(string guestSessionId, long placeId)
    {
        return QuerySingle("SELECT * FROM guest_poi_states WHERE guest_session_id = @guest AND place_id = @place;", cmd =>
        {
            cmd.Parameters.AddWithValue("@guest", guestSessionId);
            cmd.Parameters.AddWithValue("@place", placeId);
        });
    }

    public List<GuestPoiStateDTO> GetByGuestSessionId(string guestSessionId)
    {
        return QueryList("SELECT * FROM guest_poi_states WHERE guest_session_id = @guest ORDER BY updated_at DESC;", cmd => cmd.Parameters.AddWithValue("@guest", guestSessionId));
    }

    public bool UpdateInsideState(string guestSessionId, long placeId, bool isInside, decimal distanceMeters)
    {
        var now = DateTime.UtcNow;
        const string sql = @"
            INSERT INTO guest_poi_states (guest_session_id, place_id, is_inside, last_entered_at, last_exited_at, last_distance_meters)
            VALUES (@guest, @place, @inside, @entered, @exited, @distance)
            ON CONFLICT (guest_session_id, place_id)
            DO UPDATE SET is_inside = @inside,
                          last_entered_at = COALESCE(@entered, guest_poi_states.last_entered_at),
                          last_exited_at = COALESCE(@exited, guest_poi_states.last_exited_at),
                          last_distance_meters = @distance;";
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@guest", guestSessionId);
        cmd.Parameters.AddWithValue("@place", placeId);
        cmd.Parameters.AddWithValue("@inside", isInside);
        cmd.Parameters.AddWithValue("@entered", isInside ? (object)now : DBNull.Value);
        cmd.Parameters.AddWithValue("@exited", isInside ? DBNull.Value : (object)now);
        cmd.Parameters.AddWithValue("@distance", distanceMeters);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateCooldown(string guestSessionId, long placeId, DateTime cooldownUntil)
    {
        const string sql = @"
            UPDATE guest_poi_states
            SET last_triggered_at = CURRENT_TIMESTAMP, cooldown_until = @cooldown
            WHERE guest_session_id = @guest AND place_id = @place;";
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cooldown", cooldownUntil);
        cmd.Parameters.AddWithValue("@guest", guestSessionId);
        cmd.Parameters.AddWithValue("@place", placeId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteByGuestSessionId(string guestSessionId)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM guest_poi_states WHERE guest_session_id = @guest;", conn);
        cmd.Parameters.AddWithValue("@guest", guestSessionId);
        return cmd.ExecuteNonQuery() > 0;
    }

    private static void Bind(NpgsqlCommand cmd, GuestPoiStateDTO dto)
    {
        cmd.Parameters.AddWithValue("@guest", dto.GuestSessionId);
        cmd.Parameters.AddWithValue("@place", dto.PlaceId);
        cmd.Parameters.AddWithValue("@inside", dto.IsInside);
        cmd.Parameters.AddWithValue("@entered", DbValue(dto.LastEnteredAt));
        cmd.Parameters.AddWithValue("@exited", DbValue(dto.LastExitedAt));
        cmd.Parameters.AddWithValue("@triggered", DbValue(dto.LastTriggeredAt));
        cmd.Parameters.AddWithValue("@cooldown", DbValue(dto.CooldownUntil));
        cmd.Parameters.AddWithValue("@distance", DbValue(dto.LastDistanceMeters));
    }

    private GuestPoiStateDTO? QuerySingle(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private List<GuestPoiStateDTO> QueryList(string sql, Action<NpgsqlCommand> bind)
    {
        var list = new List<GuestPoiStateDTO>();
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    private static GuestPoiStateDTO Map(NpgsqlDataReader reader) => new()
    {
        GuestSessionId = Convert.ToString(reader["guest_session_id"]) ?? string.Empty,
        PlaceId = Convert.ToInt64(reader["place_id"]),
        IsInside = Convert.ToBoolean(reader["is_inside"]),
        LastEnteredAt = reader["last_entered_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["last_entered_at"]),
        LastExitedAt = reader["last_exited_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["last_exited_at"]),
        LastTriggeredAt = reader["last_triggered_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["last_triggered_at"]),
        CooldownUntil = reader["cooldown_until"] == DBNull.Value ? null : Convert.ToDateTime(reader["cooldown_until"]),
        LastDistanceMeters = reader["last_distance_meters"] == DBNull.Value ? null : Convert.ToDecimal(reader["last_distance_meters"]),
        UpdatedAt = Convert.ToDateTime(reader["updated_at"])
    };
}

public class GeofenceEventDAO : BaseDAO
{
    public GeofenceEventDAO(DbConnectionFactory factory) : base(factory) { }

    public long Insert(GeofenceEventDTO dto)
    {
        const string sql = @"
            INSERT INTO geofence_events
            (guest_session_id, place_id, narration_id, event_type_id, event_status_id, user_latitude, user_longitude, distance_meters, processed_at, note)
            VALUES (@guest, @place, @narration, @type, @status, @lat, @lng, @distance, @processed, @note)
            RETURNING event_id;";
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        Bind(cmd, dto);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public bool UpdateStatus(long eventId, long eventStatusId)
    {
        return Execute("UPDATE geofence_events SET event_status_id = @status WHERE event_id = @id;", cmd =>
        {
            cmd.Parameters.AddWithValue("@id", eventId);
            cmd.Parameters.AddWithValue("@status", eventStatusId);
        });
    }

    public bool MarkProcessed(long eventId)
    {
        return Execute("UPDATE geofence_events SET processed_at = CURRENT_TIMESTAMP WHERE event_id = @id;", cmd => cmd.Parameters.AddWithValue("@id", eventId));
    }

    public GeofenceEventDTO? GetById(long eventId) => QuerySingle("SELECT * FROM geofence_events WHERE event_id = @id;", cmd => cmd.Parameters.AddWithValue("@id", eventId));
    public List<GeofenceEventDTO> GetAll() => QueryList("SELECT * FROM geofence_events ORDER BY event_id DESC;");
    public List<GeofenceEventDTO> GetByGuestSessionId(string guestSessionId) => QueryList("SELECT * FROM geofence_events WHERE guest_session_id = @guest ORDER BY event_id DESC;", cmd => cmd.Parameters.AddWithValue("@guest", guestSessionId));
    public List<GeofenceEventDTO> GetByPlaceId(long placeId) => QueryList("SELECT * FROM geofence_events WHERE place_id = @place ORDER BY event_id DESC;", cmd => cmd.Parameters.AddWithValue("@place", placeId));
    public List<GeofenceEventDTO> GetByDateRange(DateTime from, DateTime to) => QueryList("SELECT * FROM geofence_events WHERE detected_at BETWEEN @from AND @to ORDER BY event_id DESC;", cmd => { cmd.Parameters.AddWithValue("@from", from); cmd.Parameters.AddWithValue("@to", to); });

    private bool Execute(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        return cmd.ExecuteNonQuery() > 0;
    }

    private GeofenceEventDTO? QuerySingle(string sql, Action<NpgsqlCommand> bind)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind(cmd);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private List<GeofenceEventDTO> QueryList(string sql, Action<NpgsqlCommand>? bind = null)
    {
        var list = new List<GeofenceEventDTO>();
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn); bind?.Invoke(cmd);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    private static void Bind(NpgsqlCommand cmd, GeofenceEventDTO dto)
    {
        cmd.Parameters.AddWithValue("@guest", dto.GuestSessionId);
        cmd.Parameters.AddWithValue("@place", dto.PlaceId);
        cmd.Parameters.AddWithValue("@narration", DbValue(dto.NarrationId));
        cmd.Parameters.AddWithValue("@type", dto.EventTypeId);
        cmd.Parameters.AddWithValue("@status", dto.EventStatusId);
        cmd.Parameters.AddWithValue("@lat", DbValue(dto.UserLatitude));
        cmd.Parameters.AddWithValue("@lng", DbValue(dto.UserLongitude));
        cmd.Parameters.AddWithValue("@distance", DbValue(dto.DistanceMeters));
        cmd.Parameters.AddWithValue("@processed", DbValue(dto.ProcessedAt));
        cmd.Parameters.AddWithValue("@note", DbValue(dto.Note));
    }

    private static GeofenceEventDTO Map(NpgsqlDataReader reader) => new()
    {
        EventId = Convert.ToInt64(reader["event_id"]),
        GuestSessionId = Convert.ToString(reader["guest_session_id"]) ?? string.Empty,
        PlaceId = Convert.ToInt64(reader["place_id"]),
        NarrationId = reader["narration_id"] == DBNull.Value ? null : Convert.ToInt64(reader["narration_id"]),
        EventTypeId = Convert.ToInt64(reader["event_type_id"]),
        EventStatusId = Convert.ToInt64(reader["event_status_id"]),
        UserLatitude = reader["user_latitude"] == DBNull.Value ? null : Convert.ToDecimal(reader["user_latitude"]),
        UserLongitude = reader["user_longitude"] == DBNull.Value ? null : Convert.ToDecimal(reader["user_longitude"]),
        DistanceMeters = reader["distance_meters"] == DBNull.Value ? null : Convert.ToDecimal(reader["distance_meters"]),
        DetectedAt = Convert.ToDateTime(reader["detected_at"]),
        ProcessedAt = reader["processed_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["processed_at"]),
        Note = reader["note"] == DBNull.Value ? null : Convert.ToString(reader["note"])
    };
}


public class ListeningHistoryDAO : GenericCrudDAO<ListeningHistoryDTO>
{
    public ListeningHistoryDAO(DbConnectionFactory factory) : base(factory) { }

    public bool UpdatePlaybackStatus(long historyId, string status)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE listening_histories SET playback_status = @status WHERE history_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", historyId);
        cmd.Parameters.AddWithValue("@status", status);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateListenDuration(long historyId, int seconds)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE listening_histories SET listen_duration_seconds = @seconds WHERE history_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", historyId);
        cmd.Parameters.AddWithValue("@seconds", seconds);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<ListeningHistoryDTO> GetByGuestSessionId(string guestSessionId) => QueryList("SELECT * FROM listening_histories WHERE guest_session_id = @guest ORDER BY history_id DESC;", cmd => cmd.Parameters.AddWithValue("@guest", guestSessionId));
    public List<ListeningHistoryDTO> GetByNarrationId(long narrationId) => QueryList("SELECT * FROM listening_histories WHERE narration_id = @id ORDER BY history_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", narrationId));
    public List<ListeningHistoryDTO> GetByDateRange(DateTime from, DateTime to) => QueryList("SELECT * FROM listening_histories WHERE listened_at BETWEEN @from AND @to ORDER BY history_id DESC;", cmd => { cmd.Parameters.AddWithValue("@from", from); cmd.Parameters.AddWithValue("@to", to); });
}

public class FeedbackDAO : GenericCrudDAO<FeedbackDTO>
{
    public FeedbackDAO(DbConnectionFactory factory) : base(factory) { }

    public bool Approve(long feedbackId)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE feedbacks SET is_approved = TRUE WHERE feedback_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", feedbackId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Reject(long feedbackId)
    {
        using var conn = CreateConnection(); conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE feedbacks SET is_approved = FALSE WHERE feedback_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", feedbackId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<FeedbackDTO> GetApproved() => QueryList("SELECT * FROM feedbacks WHERE is_approved = TRUE ORDER BY feedback_id DESC;");
    public List<FeedbackDTO> GetPending() => QueryList("SELECT * FROM feedbacks WHERE is_approved = FALSE ORDER BY feedback_id DESC;");
    public List<FeedbackDTO> GetByPlaceId(long placeId) => QueryList("SELECT * FROM feedbacks WHERE place_id = @id ORDER BY feedback_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", placeId));
    public List<FeedbackDTO> GetByDishId(long dishId) => QueryList("SELECT * FROM feedbacks WHERE dish_id = @id ORDER BY feedback_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", dishId));
    public List<FeedbackDTO> GetByNarrationId(long narrationId) => QueryList("SELECT * FROM feedbacks WHERE narration_id = @id ORDER BY feedback_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", narrationId));
}
