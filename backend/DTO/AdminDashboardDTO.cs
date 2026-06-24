namespace VinhKhanhNarration.Api.DTO;

public class AdminDashboardStatisticsDTO
{
    public string Period { get; set; } = "month";
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public long Places { get; set; }
    public long Dishes { get; set; }
    public long Narrations { get; set; }
    public long Vendors { get; set; }

    public long Feedbacks { get; set; }
    public long Listening { get; set; }
    public long Geofence { get; set; }
    public long GuestSessions { get; set; }
    public long VendorPayments { get; set; }

    public decimal GuestRevenue { get; set; }
    public decimal VendorRevenue { get; set; }
    public decimal TotalRevenue => GuestRevenue + VendorRevenue;

    public List<AdminDashboardActivityDTO> Activity { get; set; } = new();
}

public class AdminDashboardActivityDTO
{
    public DateTime PeriodStart { get; set; }
    public long Listening { get; set; }
    public long Geofence { get; set; }
    public long Feedbacks { get; set; }
    public long GuestSessions { get; set; }
    public decimal GuestRevenue { get; set; }
    public decimal VendorRevenue { get; set; }
}
