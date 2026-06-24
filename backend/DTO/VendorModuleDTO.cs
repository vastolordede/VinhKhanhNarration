using Microsoft.AspNetCore.Http;

namespace VinhKhanhNarration.Api.DTO;

public static class VendorAccountStatuses
{
    public const string PendingReview = "PendingReview";
    public const string PendingPayment = "PendingPayment";
    public const string Active = "Active";
    public const string ExpiringSoon = "ExpiringSoon";
    public const string Expired = "Expired";
    public const string Rejected = "Rejected";
    public const string Suspended = "Suspended";
}

public class VendorRegistrationFormDTO
{
    public string OwnerName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public long? PlaceId { get; set; }
    public DateTime FoodSafetyExpiresAt { get; set; }
    public IFormFile? BusinessLicense { get; set; }
    public IFormFile? FoodSafetyCertificate { get; set; }
}

public class VendorRenewalFormDTO
{
    public DateTime FoodSafetyExpiresAt { get; set; }
    public IFormFile? FoodSafetyCertificate { get; set; }
}

public class VendorLoginRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class VendorAuthUserDTO
{
    public long VendorUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string AccountStatus { get; set; } = string.Empty;
    public long? PlaceId { get; set; }
}

public class VendorLoginResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public VendorAuthUserDTO Vendor { get; set; } = new();
}

public class VendorRefreshTokenRequestDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class VendorLogoutRequestDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class VendorChangePasswordRequestDTO
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class VendorProfileUpdateDTO
{
    public string OwnerName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class VendorDocumentDTO
{
    public long DocumentId { get; set; }
    public long VendorUserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? ReviewReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VendorSubscriptionDTO
{
    public long SubscriptionId { get; set; }
    public long VendorUserId { get; set; }
    public long PaymentOrderId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PaymentOrderDTO
{
    public long PaymentOrderId { get; set; }
    public long VendorUserId { get; set; }
    public long? RenewalRequestId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string QrImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class VendorNotificationDTO
{
    public long NotificationId { get; set; }
    public long VendorUserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class VendorDashboardDTO
{
    public VendorAuthUserDTO Vendor { get; set; } = new();
    public VendorSubscriptionDTO? Subscription { get; set; }
    public PaymentOrderDTO? PendingPayment { get; set; }
    public int UnreadNotifications { get; set; }
    public int DaysRemaining { get; set; }
    public bool CanManageContent { get; set; }
    public bool ShouldWarnExpiry { get; set; }
}

public class AdminVendorListItemDTO
{
    public long VendorUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public long? PlaceId { get; set; }
    public string AccountStatus { get; set; } = string.Empty;
    public string? ReviewReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VendorDocumentDTO> Documents { get; set; } = new();
    public VendorSubscriptionDTO? Subscription { get; set; }
    public PaymentOrderDTO? PendingPayment { get; set; }
}

public class ReviewVendorRequestDTO
{
    public bool Approved { get; set; }
    public string? Reason { get; set; }
    public decimal Amount { get; set; } = 500000;
}

public class ReviewRenewalRequestDTO
{
    public bool Approved { get; set; }
    public string? Reason { get; set; }
    public decimal Amount { get; set; } = 500000;
}

public class VendorRenewalRequestDTO
{
    public long RenewalRequestId { get; set; }
    public long VendorUserId { get; set; }
    public long FoodSafetyDocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NarrationModerationRequestDTO
{
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
