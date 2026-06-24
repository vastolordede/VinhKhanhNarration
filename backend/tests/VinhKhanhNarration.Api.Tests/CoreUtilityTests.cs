using Xunit;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.Tests;

public class CoreUtilityTests
{
    [Fact]
    public void Distance_FromPointToItself_IsZero()
    {
        var calculator = new GeoDistanceCalculator();

        var distance = calculator.CalculateDistanceMeters(
            10.755m, 106.703m, 10.755m, 106.703m);

        Assert.Equal(0m, distance);
    }

    [Fact]
    public void Distance_OneLatitudeDegree_IsAbout111Kilometers()
    {
        var calculator = new GeoDistanceCalculator();

        var distance = calculator.CalculateDistanceMeters(0m, 0m, 1m, 0m);

        Assert.InRange(distance, 111_000m, 111_300m);
    }

    [Fact]
    public void PasswordHasher_RoundTripsAndRejectsWrongPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.HashPassword("CorrectHorseBatteryStaple!");

        Assert.True(hasher.VerifyPassword("CorrectHorseBatteryStaple!", hash));
        Assert.False(hasher.VerifyPassword("wrong-password", hash));
    }
}
