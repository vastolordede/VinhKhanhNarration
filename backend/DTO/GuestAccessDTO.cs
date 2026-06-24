using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

public static class GuestPaymentStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}

public static class GuestAccessPassStatuses
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Revoked = "Revoked";
}

[DbTable("guest_payment_orders")]
public class GuestPaymentOrderDTO
{
    [DbColumn("guest_payment_order_id", IsKey = true, IsIdentity = true)]
    public long GuestPaymentOrderId { get; set; }

    [DbColumn("guest_session_id")]
    public string? GuestSessionId { get; set; }

    [DbColumn("order_code")]
    public string OrderCode { get; set; } = string.Empty;

    [DbColumn("amount")]
    public decimal Amount { get; set; }

    [DbColumn("status")]
    public string Status { get; set; } = GuestPaymentStatuses.Pending;

    [DbColumn("provider")]
    public string Provider { get; set; } = "Mock";

    [DbColumn("payment_url")]
    public string PaymentUrl { get; set; } = string.Empty;

    [DbColumn("preferred_language_id")]
    public long? PreferredLanguageId { get; set; }

    [DbColumn("device_info")]
    public string? DeviceInfo { get; set; }

    [DbColumn("ip_address")]
    public string? IPAddress { get; set; }

    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime CreatedAt { get; set; }

    [DbColumn("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [DbColumn("paid_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime? PaidAt { get; set; }
}

[DbTable("guest_access_passes")]
public class GuestAccessPassDTO
{
    [DbColumn("access_pass_id", IsKey = true, IsIdentity = true)]
    public long AccessPassId { get; set; }

    [DbColumn("guest_session_id")]
    public string GuestSessionId { get; set; } = string.Empty;

    [DbColumn("guest_payment_order_id")]
    public long GuestPaymentOrderId { get; set; }

    [DbColumn("starts_at")]
    public DateTime StartsAt { get; set; }

    [DbColumn("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [DbColumn("status")]
    public string Status { get; set; } = GuestAccessPassStatuses.Active;

    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime CreatedAt { get; set; }

    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)]
    public DateTime UpdatedAt { get; set; }
}

public class GuestAccessStatusDTO
{
    public bool HasActivePass { get; set; }
    public GuestSessionDTO? GuestSession { get; set; }
    public GuestAccessPassDTO? AccessPass { get; set; }
    public GuestPaymentOrderDTO? PendingPayment { get; set; }
    public int HoursRemaining { get; set; }
}

public class CreateGuestPaymentRequestDTO
{
    public long? PreferredLanguageId { get; set; }
    public string? DeviceInfo { get; set; }
}

