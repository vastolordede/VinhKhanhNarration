using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class VendorModuleDAO : BaseDAO
{
    public VendorModuleDAO(DbConnectionFactory factory) : base(factory) { }

    public bool EmailExists(string email)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT COUNT(1) FROM vendor_users WHERE lower(email) = lower(@email);",
            conn);
        cmd.Parameters.AddWithValue("@email", email);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public long InsertVendor(
        string ownerName,
        string shopName,
        string email,
        string? phone,
        string passwordHash,
        long? placeId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO vendor_users
            (owner_name, shop_name, email, phone, password_hash, place_id,
             account_status, is_active)
            VALUES
            (@ownerName, @shopName, lower(@email), @phone, @passwordHash, @placeId,
             'PendingReview', FALSE)
            RETURNING vendor_user_id;", conn);
        cmd.Parameters.AddWithValue("@ownerName", ownerName);
        cmd.Parameters.AddWithValue("@shopName", shopName);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@phone", DbValue(phone));
        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
        cmd.Parameters.AddWithValue("@placeId", DbValue(placeId));
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public VendorAuthUserDTO? GetVendorByEmail(string email)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT vendor_user_id, owner_name, shop_name, email, phone, account_status, place_id
            FROM vendor_users
            WHERE lower(email) = lower(@email)
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@email", email);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapVendor(reader) : null;
    }

    public (VendorAuthUserDTO User, string PasswordHash)? GetVendorLoginByEmail(string email)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT vendor_user_id, owner_name, shop_name, email, phone, account_status,
                   place_id, password_hash
            FROM vendor_users
            WHERE lower(email) = lower(@email)
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@email", email);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var user = MapVendor(reader);
        return (user, reader.GetString(reader.GetOrdinal("password_hash")));
    }

    public VendorAuthUserDTO? GetVendorById(long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT vendor_user_id, owner_name, shop_name, email, phone, account_status, place_id
            FROM vendor_users
            WHERE vendor_user_id = @id
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@id", vendorUserId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapVendor(reader) : null;
    }

    public List<AdminVendorListItemDTO> GetAllVendors()
    {
        var result = new List<AdminVendorListItemDTO>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT vendor_user_id, owner_name, shop_name, email, phone, place_id,
                   account_status, review_reason, created_at
            FROM vendor_users
            ORDER BY created_at DESC, vendor_user_id DESC;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new AdminVendorListItemDTO
            {
                VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
                OwnerName = reader.GetString(reader.GetOrdinal("owner_name")),
                ShopName = reader.GetString(reader.GetOrdinal("shop_name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Phone = ReadNullableString(reader, "phone"),
                PlaceId = ReadNullableLong(reader, "place_id"),
                AccountStatus = reader.GetString(reader.GetOrdinal("account_status")),
                ReviewReason = ReadNullableString(reader, "review_reason"),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            });
        }
        return result;
    }

    public bool UpdateVendorReview(
        long vendorUserId,
        long adminId,
        bool approved,
        string? reason)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET account_status = @status,
                review_reason = @reason,
                reviewed_by = @adminId,
                reviewed_at = CURRENT_TIMESTAMP,
                is_active = FALSE,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @id
              AND account_status IN ('PendingReview','Rejected');", conn);
        cmd.Parameters.AddWithValue("@status", approved ? "PendingPayment" : "Rejected");
        cmd.Parameters.AddWithValue("@reason", DbValue(reason));
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@id", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool SetVendorAccountStatus(long vendorUserId, string status, bool isActive)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET account_status = @status,
                is_active = @active,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @id;", conn);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@active", isActive);
        cmd.Parameters.AddWithValue("@id", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public long InsertDocument(
        long vendorUserId,
        string documentType,
        string fileName,
        string fileUrl,
        DateTime? expiresAt)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO vendor_documents
            (vendor_user_id, document_type, file_name, file_url, expires_at,
             verification_status)
            VALUES (@vendorId, @type, @fileName, @fileUrl, @expiresAt, 'Pending')
            RETURNING document_id;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        cmd.Parameters.AddWithValue("@type", documentType);
        cmd.Parameters.AddWithValue("@fileName", fileName);
        cmd.Parameters.AddWithValue("@fileUrl", fileUrl);
        cmd.Parameters.AddWithValue("@expiresAt", DbValue(expiresAt?.Date));
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public VendorDocumentDTO? GetDocumentById(long documentId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT document_id, vendor_user_id, document_type, file_name, file_url,
                   expires_at, verification_status, review_reason, created_at
            FROM vendor_documents
            WHERE document_id = @documentId
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@documentId", documentId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapDocument(reader) : null;
    }

    public List<VendorDocumentDTO> GetDocuments(long vendorUserId)
    {
        var result = new List<VendorDocumentDTO>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT document_id, vendor_user_id, document_type, file_name, file_url,
                   expires_at, verification_status, review_reason, created_at
            FROM vendor_documents
            WHERE vendor_user_id = @vendorId
            ORDER BY created_at DESC, document_id DESC;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(MapDocument(reader));
        return result;
    }

    public bool ReviewPendingDocuments(long vendorUserId, long adminId, bool approved, string? reason)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_documents
            SET verification_status = @status,
                review_reason = @reason,
                reviewed_by = @adminId,
                reviewed_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorId
              AND verification_status = 'Pending';", conn);
        cmd.Parameters.AddWithValue("@status", approved ? "Approved" : "Rejected");
        cmd.Parameters.AddWithValue("@reason", DbValue(reason));
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public long InsertRenewalRequest(long vendorUserId, long foodSafetyDocumentId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO vendor_renewal_requests
            (vendor_user_id, food_safety_document_id, status)
            VALUES (@vendorId, @documentId, 'PendingReview')
            RETURNING renewal_request_id;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        cmd.Parameters.AddWithValue("@documentId", foodSafetyDocumentId);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public List<VendorRenewalRequestDTO> GetPendingRenewals()
    {
        var result = new List<VendorRenewalRequestDTO>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT renewal_request_id, vendor_user_id, food_safety_document_id,
                   status, review_reason, created_at
            FROM vendor_renewal_requests
            WHERE status = 'PendingReview'
            ORDER BY created_at;", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new VendorRenewalRequestDTO
            {
                RenewalRequestId = reader.GetInt64(reader.GetOrdinal("renewal_request_id")),
                VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
                FoodSafetyDocumentId = reader.GetInt64(reader.GetOrdinal("food_safety_document_id")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                ReviewReason = ReadNullableString(reader, "review_reason"),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            });
        }
        return result;
    }

    public VendorRenewalRequestDTO? GetRenewalRequest(long requestId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT renewal_request_id, vendor_user_id, food_safety_document_id,
                   status, review_reason, created_at
            FROM vendor_renewal_requests
            WHERE renewal_request_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", requestId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new VendorRenewalRequestDTO
        {
            RenewalRequestId = reader.GetInt64(reader.GetOrdinal("renewal_request_id")),
            VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
            FoodSafetyDocumentId = reader.GetInt64(reader.GetOrdinal("food_safety_document_id")),
            Status = reader.GetString(reader.GetOrdinal("status")),
            ReviewReason = ReadNullableString(reader, "review_reason"),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }

    public bool ReviewRenewal(long requestId, long adminId, bool approved, string? reason)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_renewal_requests
            SET status = @status,
                review_reason = @reason,
                reviewed_by = @adminId,
                reviewed_at = CURRENT_TIMESTAMP
            WHERE renewal_request_id = @id
              AND status = 'PendingReview';", conn);
        cmd.Parameters.AddWithValue("@status", approved ? "PendingPayment" : "Rejected");
        cmd.Parameters.AddWithValue("@reason", DbValue(reason));
        cmd.Parameters.AddWithValue("@adminId", adminId);
        cmd.Parameters.AddWithValue("@id", requestId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool MarkRenewalPaid(long requestId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_renewal_requests
            SET status = 'Paid'
            WHERE renewal_request_id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", requestId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public long InsertPaymentOrder(PaymentOrderDTO order)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO payment_orders
            (vendor_user_id, renewal_request_id, purpose, order_code, amount,
             status, provider, payment_url, qr_image_url, expires_at)
            VALUES
            (@vendorId, @renewalId, @purpose, @code, @amount,
             'Pending', @provider, @paymentUrl, @qrUrl, @expiresAt)
            RETURNING payment_order_id;", conn);
        cmd.Parameters.AddWithValue("@vendorId", order.VendorUserId);
        cmd.Parameters.AddWithValue("@renewalId", DbValue(order.RenewalRequestId));
        cmd.Parameters.AddWithValue("@purpose", order.Purpose);
        cmd.Parameters.AddWithValue("@code", order.OrderCode);
        cmd.Parameters.AddWithValue("@amount", order.Amount);
        cmd.Parameters.AddWithValue("@provider", order.Provider);
        cmd.Parameters.AddWithValue("@paymentUrl", order.PaymentUrl);
        cmd.Parameters.AddWithValue("@qrUrl", order.QrImageUrl);
        cmd.Parameters.AddWithValue("@expiresAt", order.ExpiresAt);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public PaymentOrderDTO? GetPaymentByOrderCode(string orderCode)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT * FROM payment_orders
            WHERE order_code = @code
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@code", orderCode);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPayment(reader) : null;
    }

    public PaymentOrderDTO? GetLatestPendingPayment(long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT * FROM payment_orders
            WHERE vendor_user_id = @vendorId
              AND status = 'Pending'
              AND expires_at > CURRENT_TIMESTAMP
            ORDER BY created_at DESC
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPayment(reader) : null;
    }

    public DateTime? ConfirmPaymentAndActivateSubscription(
        long paymentOrderId,
        long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        string status;
        DateTime orderExpiresAt;
        long? renewalRequestId;
        using (var select = new NpgsqlCommand(@"
            SELECT status, expires_at, renewal_request_id
            FROM payment_orders
            WHERE payment_order_id = @paymentId
              AND vendor_user_id = @vendorUserId
            FOR UPDATE;", conn, tx))
        {
            select.Parameters.AddWithValue("@paymentId", paymentOrderId);
            select.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            using var reader = select.ExecuteReader();
            if (!reader.Read()) return null;
            status = reader.GetString(0);
            orderExpiresAt = reader.GetDateTime(1);
            renewalRequestId = reader.IsDBNull(2) ? null : reader.GetInt64(2);
        }

        if (status == "Paid")
        {
            using var existing = new NpgsqlCommand(@"
                SELECT expires_at
                FROM vendor_subscriptions
                WHERE payment_order_id = @paymentId
                LIMIT 1;", conn, tx);
            existing.Parameters.AddWithValue("@paymentId", paymentOrderId);
            var existingExpiry = existing.ExecuteScalar();
            if (existingExpiry != null && existingExpiry != DBNull.Value)
            {
                tx.Commit();
                return Convert.ToDateTime(existingExpiry);
            }
        }
        else
        {
            if (status != "Pending" || orderExpiresAt <= DateTime.UtcNow)
                return null;

            using var pay = new NpgsqlCommand(@"
                UPDATE payment_orders
                SET status = 'Paid', paid_at = CURRENT_TIMESTAMP
                WHERE payment_order_id = @paymentId
                  AND status = 'Pending';", conn, tx);
            pay.Parameters.AddWithValue("@paymentId", paymentOrderId);
            if (pay.ExecuteNonQuery() == 0) return null;
        }

        DateTime baseDate;
        using (var current = new NpgsqlCommand(@"
            SELECT expires_at
            FROM vendor_subscriptions
            WHERE vendor_user_id = @vendorUserId
              AND status = 'Active'
              AND expires_at > CURRENT_TIMESTAMP
            ORDER BY expires_at DESC
            LIMIT 1
            FOR UPDATE;", conn, tx))
        {
            current.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            var currentExpiry = current.ExecuteScalar();
            baseDate = currentExpiry == null || currentExpiry == DBNull.Value
                ? DateTime.UtcNow
                : Convert.ToDateTime(currentExpiry);
        }

        using (var expire = new NpgsqlCommand(@"
            UPDATE vendor_subscriptions
            SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorUserId
              AND status = 'Active';", conn, tx))
        {
            expire.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            expire.ExecuteNonQuery();
        }

        var subscriptionExpiresAt = baseDate.AddMonths(6);
        using (var create = new NpgsqlCommand(@"
            INSERT INTO vendor_subscriptions
            (vendor_user_id, payment_order_id, starts_at, expires_at, status)
            VALUES
            (@vendorUserId, @paymentId, CURRENT_TIMESTAMP, @expiresAt, 'Active')
            ON CONFLICT (payment_order_id)
            DO UPDATE SET status = 'Active',
                          expires_at = EXCLUDED.expires_at,
                          updated_at = CURRENT_TIMESTAMP;", conn, tx))
        {
            create.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            create.Parameters.AddWithValue("@paymentId", paymentOrderId);
            create.Parameters.AddWithValue("@expiresAt", subscriptionExpiresAt);
            create.ExecuteNonQuery();
        }

        using (var activate = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET account_status = 'Active',
                is_active = TRUE,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorUserId;", conn, tx))
        {
            activate.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            if (activate.ExecuteNonQuery() == 0) return null;
        }

        using (var assign = new NpgsqlCommand(@"
            UPDATE places p
            SET owner_vendor_id = @vendorUserId,
                updated_at = CURRENT_TIMESTAMP
            FROM vendor_users vu
            WHERE vu.vendor_user_id = @vendorUserId
              AND vu.place_id = p.place_id
              AND (p.owner_vendor_id IS NULL OR p.owner_vendor_id = @vendorUserId);", conn, tx))
        {
            assign.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            assign.ExecuteNonQuery();
        }

        if (renewalRequestId.HasValue)
        {
            using var renewal = new NpgsqlCommand(@"
                UPDATE vendor_renewal_requests
                SET status = 'Paid', updated_at = CURRENT_TIMESTAMP
                WHERE renewal_request_id = @requestId;", conn, tx);
            renewal.Parameters.AddWithValue("@requestId", renewalRequestId.Value);
            renewal.ExecuteNonQuery();
        }

        tx.Commit();
        return subscriptionExpiresAt;
    }

    public bool MarkPaymentPaid(long paymentOrderId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE payment_orders
            SET status = 'Paid', paid_at = CURRENT_TIMESTAMP
            WHERE payment_order_id = @id
              AND status = 'Pending'
              AND expires_at > CURRENT_TIMESTAMP;", conn);
        cmd.Parameters.AddWithValue("@id", paymentOrderId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public VendorSubscriptionDTO? GetLatestSubscription(long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT subscription_id, vendor_user_id, payment_order_id,
                   starts_at, expires_at, status
            FROM vendor_subscriptions
            WHERE vendor_user_id = @vendorId
            ORDER BY expires_at DESC, subscription_id DESC
            LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapSubscription(reader) : null;
    }

    public long InsertSubscription(
        long vendorUserId,
        long paymentOrderId,
        DateTime startsAt,
        DateTime expiresAt)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        using (var expire = new NpgsqlCommand(@"
            UPDATE vendor_subscriptions
            SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorId AND status = 'Active';", conn, transaction))
        {
            expire.Parameters.AddWithValue("@vendorId", vendorUserId);
            expire.ExecuteNonQuery();
        }

        long subscriptionId;
        using (var insert = new NpgsqlCommand(@"
            INSERT INTO vendor_subscriptions
            (vendor_user_id, payment_order_id, starts_at, expires_at, status)
            VALUES (@vendorId, @paymentId, @startsAt, @expiresAt, 'Active')
            RETURNING subscription_id;", conn, transaction))
        {
            insert.Parameters.AddWithValue("@vendorId", vendorUserId);
            insert.Parameters.AddWithValue("@paymentId", paymentOrderId);
            insert.Parameters.AddWithValue("@startsAt", startsAt);
            insert.Parameters.AddWithValue("@expiresAt", expiresAt);
            subscriptionId = Convert.ToInt64(insert.ExecuteScalar());
        }

        transaction.Commit();
        return subscriptionId;
    }

    public bool ExpireSubscription(long subscriptionId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_subscriptions
            SET status = 'Expired', updated_at = CURRENT_TIMESTAMP
            WHERE subscription_id = @id AND status = 'Active';", conn);
        cmd.Parameters.AddWithValue("@id", subscriptionId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public long InsertNotification(
        long vendorUserId,
        string type,
        string title,
        string message,
        string? entityType = null,
        long? entityId = null)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO vendor_notifications
            (vendor_user_id, notification_type, title, message, entity_type, entity_id)
            VALUES (@vendorId, @type, @title, @message, @entityType, @entityId)
            RETURNING notification_id;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        cmd.Parameters.AddWithValue("@type", type);
        cmd.Parameters.AddWithValue("@title", title);
        cmd.Parameters.AddWithValue("@message", message);
        cmd.Parameters.AddWithValue("@entityType", DbValue(entityType));
        cmd.Parameters.AddWithValue("@entityId", DbValue(entityId));
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public bool NotificationExists(long vendorUserId, string type, string entityType, long entityId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM vendor_notifications
            WHERE vendor_user_id = @vendorId
              AND notification_type = @type
              AND entity_type = @entityType
              AND entity_id = @entityId;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        cmd.Parameters.AddWithValue("@type", type);
        cmd.Parameters.AddWithValue("@entityType", entityType);
        cmd.Parameters.AddWithValue("@entityId", entityId);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public List<VendorNotificationDTO> GetNotifications(long vendorUserId)
    {
        var result = new List<VendorNotificationDTO>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT notification_id, vendor_user_id, notification_type, title,
                   message, entity_type, entity_id, is_read, created_at, read_at
            FROM vendor_notifications
            WHERE vendor_user_id = @vendorId
            ORDER BY created_at DESC, notification_id DESC;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(MapNotification(reader));
        return result;
    }

    public int GetUnreadCount(long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM vendor_notifications
            WHERE vendor_user_id = @vendorId AND is_read = FALSE;", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool MarkNotificationRead(long notificationId, long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_notifications
            SET is_read = TRUE, read_at = CURRENT_TIMESTAMP
            WHERE notification_id = @id AND vendor_user_id = @vendorId;", conn);
        cmd.Parameters.AddWithValue("@id", notificationId);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AssignPlaceOwner(long? placeId, long vendorUserId)
    {
        if (!placeId.HasValue) return true;
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE places
            SET owner_vendor_id = @vendorId
            WHERE place_id = @placeId
              AND (owner_vendor_id IS NULL OR owner_vendor_id = @vendorId);", conn);
        cmd.Parameters.AddWithValue("@vendorId", vendorUserId);
        cmd.Parameters.AddWithValue("@placeId", placeId.Value);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateProfile(long vendorUserId, VendorProfileUpdateDTO profile)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET owner_name = @ownerName,
                shop_name = @shopName,
                phone = @phone,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorUserId;", conn);
        cmd.Parameters.AddWithValue("@ownerName", profile.OwnerName);
        cmd.Parameters.AddWithValue("@shopName", profile.ShopName);
        cmd.Parameters.AddWithValue("@phone", DbValue(profile.Phone));
        cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public string? GetPasswordHash(long vendorUserId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(
            "SELECT password_hash FROM vendor_users WHERE vendor_user_id = @id LIMIT 1;",
            conn);
        cmd.Parameters.AddWithValue("@id", vendorUserId);
        return cmd.ExecuteScalar() as string;
    }

    public bool UpdatePassword(long vendorUserId, string passwordHash)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET password_hash = @passwordHash,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @id;", conn);
        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
        cmd.Parameters.AddWithValue("@id", vendorUserId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<long> GetLifecycleVendorIds()
    {
        var result = new List<long>();
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            SELECT DISTINCT vendor_user_id
            FROM vendor_subscriptions
            WHERE status = 'Active'
            UNION
            SELECT vendor_user_id
            FROM vendor_users
            WHERE account_status IN ('Active','ExpiringSoon');", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(reader.GetInt64(0));
        return result;
    }

    public int ExpirePendingPayments()
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE payment_orders
            SET status = 'Expired'
            WHERE status = 'Pending'
              AND expires_at <= CURRENT_TIMESTAMP;", conn);
        return cmd.ExecuteNonQuery();
    }

    public bool AssignPlaceToVendor(long vendorUserId, long placeId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        using (var validate = new NpgsqlCommand(@"
            SELECT COUNT(1)
            FROM places
            WHERE place_id = @placeId
              AND (owner_vendor_id IS NULL OR owner_vendor_id = @vendorUserId);", conn, tx))
        {
            validate.Parameters.AddWithValue("@placeId", placeId);
            validate.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            if (Convert.ToInt64(validate.ExecuteScalar()) == 0)
                return false;
        }

        using (var place = new NpgsqlCommand(@"
            UPDATE places
            SET owner_vendor_id = @vendorUserId,
                updated_at = CURRENT_TIMESTAMP
            WHERE place_id = @placeId
              AND (owner_vendor_id IS NULL OR owner_vendor_id = @vendorUserId);", conn, tx))
        {
            place.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            place.Parameters.AddWithValue("@placeId", placeId);
            if (place.ExecuteNonQuery() == 0) return false;
        }

        using (var vendor = new NpgsqlCommand(@"
            UPDATE vendor_users
            SET place_id = @placeId,
                updated_at = CURRENT_TIMESTAMP
            WHERE vendor_user_id = @vendorUserId
              AND (place_id IS NULL OR place_id = @placeId);", conn, tx))
        {
            vendor.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            vendor.Parameters.AddWithValue("@placeId", placeId);
            if (vendor.ExecuteNonQuery() == 0) return false;
        }

        tx.Commit();
        return true;
    }

    private static VendorAuthUserDTO MapVendor(NpgsqlDataReader reader) => new()
    {
        VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
        OwnerName = reader.GetString(reader.GetOrdinal("owner_name")),
        ShopName = reader.GetString(reader.GetOrdinal("shop_name")),
        Email = reader.GetString(reader.GetOrdinal("email")),
        Phone = ReadNullableString(reader, "phone"),
        AccountStatus = reader.GetString(reader.GetOrdinal("account_status")),
        PlaceId = ReadNullableLong(reader, "place_id")
    };

    private static VendorDocumentDTO MapDocument(NpgsqlDataReader reader) => new()
    {
        DocumentId = reader.GetInt64(reader.GetOrdinal("document_id")),
        VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
        DocumentType = reader.GetString(reader.GetOrdinal("document_type")),
        FileName = reader.GetString(reader.GetOrdinal("file_name")),
        FileUrl = reader.GetString(reader.GetOrdinal("file_url")),
        ExpiresAt = ReadNullableDateTime(reader, "expires_at"),
        VerificationStatus = reader.GetString(reader.GetOrdinal("verification_status")),
        ReviewReason = ReadNullableString(reader, "review_reason"),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
    };

    private static PaymentOrderDTO MapPayment(NpgsqlDataReader reader) => new()
    {
        PaymentOrderId = reader.GetInt64(reader.GetOrdinal("payment_order_id")),
        VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
        RenewalRequestId = ReadNullableLong(reader, "renewal_request_id"),
        Purpose = reader.GetString(reader.GetOrdinal("purpose")),
        OrderCode = reader.GetString(reader.GetOrdinal("order_code")),
        Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
        Status = reader.GetString(reader.GetOrdinal("status")),
        Provider = reader.GetString(reader.GetOrdinal("provider")),
        PaymentUrl = reader.GetString(reader.GetOrdinal("payment_url")),
        QrImageUrl = reader.GetString(reader.GetOrdinal("qr_image_url")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
        ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at")),
        PaidAt = ReadNullableDateTime(reader, "paid_at")
    };

    private static VendorSubscriptionDTO MapSubscription(NpgsqlDataReader reader) => new()
    {
        SubscriptionId = reader.GetInt64(reader.GetOrdinal("subscription_id")),
        VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
        PaymentOrderId = reader.GetInt64(reader.GetOrdinal("payment_order_id")),
        StartsAt = reader.GetDateTime(reader.GetOrdinal("starts_at")),
        ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at")),
        Status = reader.GetString(reader.GetOrdinal("status"))
    };

    private static VendorNotificationDTO MapNotification(NpgsqlDataReader reader) => new()
    {
        NotificationId = reader.GetInt64(reader.GetOrdinal("notification_id")),
        VendorUserId = reader.GetInt64(reader.GetOrdinal("vendor_user_id")),
        NotificationType = reader.GetString(reader.GetOrdinal("notification_type")),
        Title = reader.GetString(reader.GetOrdinal("title")),
        Message = reader.GetString(reader.GetOrdinal("message")),
        EntityType = ReadNullableString(reader, "entity_type"),
        EntityId = ReadNullableLong(reader, "entity_id"),
        IsRead = reader.GetBoolean(reader.GetOrdinal("is_read")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
        ReadAt = ReadNullableDateTime(reader, "read_at")
    };

    private static string? ReadNullableString(NpgsqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static long? ReadNullableLong(NpgsqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static DateTime? ReadNullableDateTime(NpgsqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
