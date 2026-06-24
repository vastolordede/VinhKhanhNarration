using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class NarrationContentDAO : GenericCrudDAO<NarrationContentDTO>
{
    public NarrationContentDAO(DbConnectionFactory factory) : base(factory) { }

    public List<NarrationContentDTO> GetByPlaceId(long placeId) =>
        QueryList(
            "SELECT * FROM narration_contents WHERE place_id = @id ORDER BY narration_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", placeId));

    public List<NarrationContentDTO> GetByDishId(long dishId) =>
        QueryList(
            "SELECT * FROM narration_contents WHERE dish_id = @id ORDER BY narration_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", dishId));

    public List<NarrationContentDTO> GetByVendorId(long vendorUserId) =>
        QueryList(
            "SELECT * FROM narration_contents WHERE submitted_by_vendor_id = @id ORDER BY narration_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", vendorUserId));

    public List<NarrationContentDTO> GetByWorkflowStatus(string status) =>
        QueryList(
            "SELECT * FROM narration_contents WHERE workflow_status = @status ORDER BY narration_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@status", status));

    public NarrationContentDTO? GetPublishedByIdForPublic(long narrationId) =>
        QuerySingle(@"
            SELECT *
            FROM narration_contents
            WHERE narration_id = @id
              AND is_active = TRUE
              AND workflow_status = 'Published'
              AND (
                    submitted_by_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = narration_contents.submitted_by_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", narrationId));

    public NarrationContentDTO? GetMainPublishedByPlaceId(long placeId) =>
        QuerySingle(@"
            SELECT *
            FROM narration_contents
            WHERE is_active = TRUE
              AND workflow_status = 'Published'
              AND place_id = @id
              AND (
                    submitted_by_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = narration_contents.submitted_by_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
            ORDER BY published_at DESC NULLS LAST, narration_id DESC
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", placeId));

    public NarrationContentDTO? GetMainPublishedByDishId(long dishId) =>
        QuerySingle(@"
            SELECT *
            FROM narration_contents
            WHERE is_active = TRUE
              AND workflow_status = 'Published'
              AND dish_id = @id
              AND (
                    submitted_by_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = narration_contents.submitted_by_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
            ORDER BY published_at DESC NULLS LAST, narration_id DESC
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", dishId));

    public bool SubmitForReview(long narrationId, long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET workflow_status = 'PendingReview',
                submitted_at = CURRENT_TIMESTAMP,
                reviewed_by = NULL,
                reviewed_at = NULL,
                rejection_reason = NULL
            WHERE narration_id = @id
              AND submitted_by_vendor_id = @vendorId
              AND workflow_status IN ('Draft', 'Rejected');", conn);
        cmd.Parameters.AddWithValue("@id", narrationId);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Review(long narrationId, long adminId, bool approved, string? reason)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET workflow_status = @status,
                reviewed_by = @adminId,
                reviewed_at = CURRENT_TIMESTAMP,
                rejection_reason = @reason
            WHERE narration_id = @id
              AND workflow_status = 'PendingReview';", conn);
        cmd.Parameters.AddWithValue("@status", approved
            ? NarrationWorkflowStatuses.Approved
            : NarrationWorkflowStatuses.Rejected);
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", narrationId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Publish(long narrationId, long adminId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET workflow_status = 'Published',
                reviewed_by = COALESCE(reviewed_by, @adminId),
                reviewed_at = COALESCE(reviewed_at, CURRENT_TIMESTAMP),
                published_at = CURRENT_TIMESTAMP,
                is_active = TRUE
            WHERE narration_id = @id
              AND workflow_status IN ('Approved', 'Published');", conn);
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@id", narrationId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool SetPendingReview(long narrationId, long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET workflow_status = 'PendingReview',
                is_active = FALSE,
                submitted_at = CURRENT_TIMESTAMP,
                reviewed_by = NULL,
                reviewed_at = NULL,
                rejection_reason = NULL,
                moderation_reason = NULL,
                moderation_by_admin_id = NULL,
                hidden_at = NULL,
                deleted_at = NULL,
                updated_at = CURRENT_TIMESTAMP
            WHERE narration_id = @id
              AND submitted_by_vendor_id = @vendorId;", conn);
        cmd.Parameters.AddWithValue("@id", narrationId);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool VendorModerate(long narrationId, long vendorUserId, string action)
    {
        var targetStatus = action switch
        {
            "hide" => NarrationWorkflowStatuses.HiddenByVendor,
            "delete" => NarrationWorkflowStatuses.DeletedByVendor,
            "restore" => null,
            _ => throw new ArgumentException("Unsupported vendor moderation action.")
        };

        using var conn = CreateConnection();
        conn.Open();

        if (action == "restore")
        {
            using var restore = new NpgsqlCommand(@"
                UPDATE narration_contents
                SET workflow_status = CASE
                        WHEN previous_workflow_status = 'Published' THEN 'Published'
                        WHEN previous_workflow_status IN ('Draft','Rejected','PendingReview')
                            THEN previous_workflow_status
                        ELSE 'PendingReview'
                    END,
                    is_active = CASE WHEN previous_workflow_status = 'Published' THEN TRUE ELSE FALSE END,
                    hidden_at = NULL,
                    deleted_at = NULL,
                    moderation_reason = NULL,
                    updated_at = CURRENT_TIMESTAMP
                WHERE narration_id = @id
                  AND submitted_by_vendor_id = @vendorId
                  AND workflow_status IN ('HiddenByVendor','DeletedByVendor');", conn);
            restore.Parameters.AddWithValue("@id", narrationId);
            restore.Parameters.AddWithValue("@vendorId", vendorUserId);
            return restore.ExecuteNonQuery() > 0;
        }

        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET previous_workflow_status = workflow_status,
                workflow_status = @status,
                is_active = FALSE,
                hidden_at = CASE WHEN @status = 'HiddenByVendor' THEN CURRENT_TIMESTAMP ELSE hidden_at END,
                deleted_at = CASE WHEN @status = 'DeletedByVendor' THEN CURRENT_TIMESTAMP ELSE deleted_at END,
                updated_at = CURRENT_TIMESTAMP
            WHERE narration_id = @id
              AND submitted_by_vendor_id = @vendorId
              AND workflow_status NOT IN ('DeletedByAdmin','HiddenByAdmin');", conn);
        cmd.Parameters.AddWithValue("@status", targetStatus!);
        cmd.Parameters.AddWithValue("@id", narrationId);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AdminModerate(
        long narrationId,
        long adminId,
        string action,
        string? reason)
    {
        var targetStatus = action switch
        {
            "hide" => NarrationWorkflowStatuses.HiddenByAdmin,
            "delete" => NarrationWorkflowStatuses.DeletedByAdmin,
            "restore" => null,
            _ => throw new ArgumentException("Unsupported admin moderation action.")
        };

        using var conn = CreateConnection();
        conn.Open();

        if (action == "restore")
        {
            using var restore = new NpgsqlCommand(@"
                UPDATE narration_contents
                SET workflow_status = CASE
                        WHEN previous_workflow_status = 'Published' THEN 'Published'
                        ELSE 'PendingReview'
                    END,
                    is_active = CASE WHEN previous_workflow_status = 'Published' THEN TRUE ELSE FALSE END,
                    moderation_reason = @reason,
                    moderation_by_admin_id = @adminId,
                    hidden_at = NULL,
                    deleted_at = NULL,
                    updated_at = CURRENT_TIMESTAMP
                WHERE narration_id = @id
                  AND workflow_status IN ('HiddenByAdmin','DeletedByAdmin');", conn);
            restore.Parameters.AddWithValue("@reason", DbValue(reason));
            restore.Parameters.AddWithValue("@adminId", adminId);
            restore.Parameters.AddWithValue("@id", narrationId);
            return restore.ExecuteNonQuery() > 0;
        }

        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_contents
            SET previous_workflow_status = workflow_status,
                workflow_status = @status,
                is_active = FALSE,
                moderation_reason = @reason,
                moderation_by_admin_id = @adminId,
                hidden_at = CASE WHEN @status = 'HiddenByAdmin' THEN CURRENT_TIMESTAMP ELSE hidden_at END,
                deleted_at = CASE WHEN @status = 'DeletedByAdmin' THEN CURRENT_TIMESTAMP ELSE deleted_at END,
                updated_at = CURRENT_TIMESTAMP
            WHERE narration_id = @id;", conn);
        cmd.Parameters.AddWithValue("@status", targetStatus!);
        cmd.Parameters.AddWithValue("@reason", DbValue(reason));
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@id", narrationId);
        return cmd.ExecuteNonQuery() > 0;
    }

}

public class NarrationTranslationDAO : GenericCrudDAO<NarrationTranslationDTO>
{
    public NarrationTranslationDAO(DbConnectionFactory factory) : base(factory) { }

    public override List<NarrationTranslationDTO> GetActive() => GetAll();
    public override bool Restore(long id) => false;

    public List<NarrationTranslationDTO> GetByNarrationId(long narrationId) =>
        QueryList(@"
            SELECT *
            FROM narration_translations
            WHERE narration_id = @id
            ORDER BY language_id;",
            cmd => cmd.Parameters.AddWithValue("@id", narrationId));

    public NarrationTranslationDTO? GetByNarrationAndLanguage(long narrationId, long languageId) =>
        QuerySingle(@"
            SELECT *
            FROM narration_translations
            WHERE narration_id = @narrationId
              AND language_id = @languageId
            LIMIT 1;", cmd =>
        {
            cmd.Parameters.AddWithValue("@narrationId", narrationId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
        });

    public NarrationTranslationDTO? GetApprovedByNarrationAndLanguage(long narrationId, long languageId) =>
        QuerySingle(@"
            SELECT *
            FROM narration_translations
            WHERE narration_id = @narrationId
              AND language_id = @languageId
              AND status = 'Approved'
              AND is_reviewed = TRUE
            LIMIT 1;", cmd =>
        {
            cmd.Parameters.AddWithValue("@narrationId", narrationId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
        });

    public bool IsTranslationExists(long narrationId, long languageId) =>
        Exists(@"
            SELECT COUNT(1)
            FROM narration_translations
            WHERE narration_id = @narrationId
              AND language_id = @languageId;", cmd =>
        {
            cmd.Parameters.AddWithValue("@narrationId", narrationId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
        });

    public long UpsertGenerated(NarrationTranslationDTO dto)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO narration_translations
            (
                narration_id,
                language_id,
                translated_title,
                translated_text,
                translation_source_id,
                provider,
                status,
                error_message,
                reviewed_by,
                reviewed_at,
                is_reviewed
            )
            VALUES
            (
                @narrationId,
                @languageId,
                @title,
                @text,
                @sourceId,
                @provider,
                @status,
                NULL,
                @reviewedBy,
                @reviewedAt,
                @isReviewed
            )
            ON CONFLICT (narration_id, language_id)
            DO UPDATE SET
                translated_title = EXCLUDED.translated_title,
                translated_text = EXCLUDED.translated_text,
                translation_source_id = EXCLUDED.translation_source_id,
                provider = EXCLUDED.provider,
                status = EXCLUDED.status,
                error_message = NULL,
                reviewed_by = EXCLUDED.reviewed_by,
                reviewed_at = EXCLUDED.reviewed_at,
                is_reviewed = EXCLUDED.is_reviewed,
                updated_at = CURRENT_TIMESTAMP
            RETURNING translation_id;", conn);

        cmd.Parameters.AddWithValue("@narrationId", dto.NarrationId);
        cmd.Parameters.AddWithValue("@languageId", dto.LanguageId);
        cmd.Parameters.AddWithValue("@title", dto.TranslatedTitle);
        cmd.Parameters.AddWithValue("@text", dto.TranslatedText);
        cmd.Parameters.AddWithValue("@sourceId", dto.TranslationSourceId);
        cmd.Parameters.AddWithValue("@provider", (object?)dto.Provider ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", dto.Status);
        cmd.Parameters.AddWithValue("@reviewedBy", (object?)dto.ReviewedBy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@reviewedAt", (object?)dto.ReviewedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@isReviewed", dto.IsReviewed);

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public bool Review(long translationId, long adminId, bool approved, string? reason)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_translations
            SET status = @status,
                is_reviewed = @approved,
                reviewed_by = @adminId,
                reviewed_at = CURRENT_TIMESTAMP,
                error_message = @reason,
                updated_at = CURRENT_TIMESTAMP
            WHERE translation_id = @id
              AND status IN ('PendingReview', 'Rejected', 'Outdated');", conn);
        cmd.Parameters.AddWithValue("@status", approved
            ? TranslationStatuses.Approved
            : TranslationStatuses.Rejected);
        cmd.Parameters.AddWithValue("@approved", approved);
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", translationId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool MarkOutdatedByNarration(long narrationId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE narration_translations
            SET status = 'Outdated',
                is_reviewed = FALSE,
                reviewed_by = NULL,
                reviewed_at = NULL,
                updated_at = CURRENT_TIMESTAMP
            WHERE narration_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", narrationId);
        cmd.ExecuteNonQuery();
        return true;
    }
}

public class AudioFileDAO : GenericCrudDAO<AudioFileDTO>
{
    public AudioFileDAO(DbConnectionFactory factory) : base(factory) { }

    public List<AudioFileDTO> GetByTranslationId(long translationId) =>
        QueryList(@"
            SELECT *
            FROM audio_files
            WHERE translation_id = @id
            ORDER BY audio_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", translationId));

    public AudioFileDTO? GetLatestByTranslationId(long translationId) =>
        QuerySingle(@"
            SELECT *
            FROM audio_files
            WHERE translation_id = @id
            ORDER BY audio_id DESC
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", translationId));

    public AudioFileDTO? GetReadyByTranslationId(long translationId) =>
        QuerySingle(@"
            SELECT *
            FROM audio_files
            WHERE translation_id = @id
              AND is_active = TRUE
              AND status = 'Ready'
              AND audio_url IS NOT NULL
              AND btrim(audio_url) <> ''
            ORDER BY audio_id DESC
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", translationId));

    public AudioFileDTO? GetPublishedPlayableByTranslationId(long translationId) =>
        QuerySingle(@"
            SELECT *
            FROM audio_files
            WHERE translation_id = @id
              AND is_active = TRUE
              AND status = 'Ready'
              AND published_at IS NOT NULL
              AND audio_url IS NOT NULL
              AND btrim(audio_url) <> ''
            ORDER BY published_at DESC, audio_id DESC
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@id", translationId));

    public bool CanVendorAccess(long audioId, long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM audio_files a
            JOIN narration_translations t
              ON t.translation_id = a.translation_id
            JOIN narration_contents n
              ON n.narration_id = t.narration_id
            WHERE a.audio_id = @audioId
              AND n.submitted_by_vendor_id = @vendorUserId;", conn);
        cmd.Parameters.AddWithValue("@audioId", audioId);
        cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public void DeactivateByTranslationId(long translationId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE audio_files
            SET is_active = FALSE,
                updated_at = CURRENT_TIMESTAMP
            WHERE translation_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", translationId);
        cmd.ExecuteNonQuery();
    }

    public bool MarkOutdatedByTranslationId(long translationId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE audio_files
            SET status = 'Outdated',
                published_at = NULL,
                updated_at = CURRENT_TIMESTAMP
            WHERE translation_id = @id
              AND status = 'Ready';", conn);
        cmd.Parameters.AddWithValue("@id", translationId);
        cmd.ExecuteNonQuery();
        return true;
    }

    public bool PublishByNarration(long narrationId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE audio_files a
            SET published_at = CURRENT_TIMESTAMP,
                updated_at = CURRENT_TIMESTAMP
            FROM narration_translations t
            WHERE a.translation_id = t.translation_id
              AND t.narration_id = @narrationId
              AND t.status = 'Approved'
              AND t.is_reviewed = TRUE
              AND a.is_active = TRUE
              AND a.status = 'Ready';", conn);
        cmd.Parameters.AddWithValue("@narrationId", narrationId);
        return cmd.ExecuteNonQuery() > 0;
    }
}
