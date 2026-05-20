namespace VinhKhanhNarration.Api.Utils;

public class GeoDistanceCalculator
{
    public decimal CalculateDistanceMeters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double earthRadius = 6371000d;
        double dLat = ToRadians((double)(lat2 - lat1));
        double dLon = ToRadians((double)(lon2 - lon1));
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(earthRadius * c);
    }

    private static double ToRadians(double degree) => degree * Math.PI / 180d;
}
