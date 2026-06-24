using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.BUS;

public class AdminDashboardBUS
{
    private readonly AdminDashboardDAO _dao;

    public AdminDashboardBUS(AdminDashboardDAO dao) => _dao = dao;

    public AdminDashboardStatisticsDTO GetStatistics(string? requestedPeriod)
    {
        var period = NormalizePeriod(requestedPeriod);
        var now = DateTime.UtcNow;
        var to = now.AddMilliseconds(1);
        DateTime from;
        string granularity;

        switch (period)
        {
            case "quarter":
                var quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;
                from = new DateTime(now.Year, quarterStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                granularity = "month";
                break;
            case "halfYear":
                from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);
                granularity = "month";
                break;
            case "year":
                from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);
                granularity = "month";
                break;
            case "all":
                from = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                granularity = "year";
                break;
            default:
                from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                granularity = "day";
                break;
        }

        return _dao.GetStatistics(period, from, to, granularity);
    }

    private static string NormalizePeriod(string? value) => value switch
    {
        "quarter" => "quarter",
        "halfYear" => "halfYear",
        "year" => "year",
        "all" => "all",
        _ => "month"
    };
}
