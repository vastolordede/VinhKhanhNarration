using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class NarrationContentDAO : GenericCrudDAO<NarrationContentDTO>
{
    public NarrationContentDAO(DbConnectionFactory factory) : base(factory) { }

    public List<NarrationContentDTO> GetByPlaceId(long placeId)
    {
        return QueryList("SELECT * FROM narration_contents WHERE place_id = @id ORDER BY narration_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", placeId));
    }

    public List<NarrationContentDTO> GetByDishId(long dishId)
    {
        return QueryList("SELECT * FROM narration_contents WHERE dish_id = @id ORDER BY narration_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", dishId));
    }

    public List<NarrationContentDTO> GetByContentTypeId(long contentTypeId)
    {
        return QueryList("SELECT * FROM narration_contents WHERE content_type_id = @id ORDER BY narration_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", contentTypeId));
    }

    public NarrationContentDTO? GetMainNarrationByPlaceId(long placeId)
    {
        return QuerySingle("SELECT * FROM narration_contents WHERE is_active = TRUE AND place_id = @id ORDER BY narration_id DESC LIMIT 1;", cmd => cmd.Parameters.AddWithValue("@id", placeId));
    }

    public NarrationContentDTO? GetMainNarrationByDishId(long dishId)
    {
        return QuerySingle("SELECT * FROM narration_contents WHERE is_active = TRUE AND dish_id = @id ORDER BY narration_id DESC LIMIT 1;", cmd => cmd.Parameters.AddWithValue("@id", dishId));
    }
}

public class NarrationTranslationDAO : GenericCrudDAO<NarrationTranslationDTO>
{
    public NarrationTranslationDAO(DbConnectionFactory factory) : base(factory) { }

    public override List<NarrationTranslationDTO> GetActive() => GetAll();
    public override bool Restore(long id) => false;

    public List<NarrationTranslationDTO> GetByNarrationId(long narrationId)
    {
        return QueryList("SELECT * FROM narration_translations WHERE narration_id = @id ORDER BY translation_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", narrationId));
    }

    public NarrationTranslationDTO? GetByNarrationAndLanguage(long narrationId, long languageId)
    {
        return QuerySingle("SELECT * FROM narration_translations WHERE narration_id = @narrationId AND language_id = @languageId LIMIT 1;", cmd =>
        {
            cmd.Parameters.AddWithValue("@narrationId", narrationId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
        });
    }

    public bool IsTranslationExists(long narrationId, long languageId)
    {
        return Exists("SELECT COUNT(1) FROM narration_translations WHERE narration_id = @narrationId AND language_id = @languageId;", cmd =>
        {
            cmd.Parameters.AddWithValue("@narrationId", narrationId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
        });
    }

    public bool MarkAsReviewed(long translationId, long reviewedBy)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE narration_translations SET is_reviewed = TRUE, reviewed_by = @reviewedBy WHERE translation_id = @id;", conn);
        cmd.Parameters.AddWithValue("@reviewedBy", reviewedBy);
        cmd.Parameters.AddWithValue("@id", translationId);
        return cmd.ExecuteNonQuery() > 0;
    }
}

public class AudioFileDAO : GenericCrudDAO<AudioFileDTO>
{
    public AudioFileDAO(DbConnectionFactory factory) : base(factory) { }

    public List<AudioFileDTO> GetByTranslationId(long translationId)
    {
        return QueryList("SELECT * FROM audio_files WHERE translation_id = @id ORDER BY audio_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", translationId));
    }

    public AudioFileDTO? GetActiveAudioByTranslationId(long translationId)
    {
        return QuerySingle("SELECT * FROM audio_files WHERE translation_id = @id AND is_active = TRUE ORDER BY audio_id DESC LIMIT 1;", cmd => cmd.Parameters.AddWithValue("@id", translationId));
    }
}
