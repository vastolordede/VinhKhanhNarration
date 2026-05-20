using FluentAssertions;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.Tests.Utils;

public class GeoDistanceCalculatorTests
{
    [Fact]
    public void CalculateDistanceMeters_ShouldReturnZero_WhenTwoPointsAreSame()
    {
        var calculator = new GeoDistanceCalculator();

        var distance = calculator.CalculateDistanceMeters(
            10.7569m,
            106.7057m,
            10.7569m,
            106.7057m
        );

        Convert.ToDouble(distance).Should().BeApproximately(0, 0.5);
    }

    [Fact]
    public void CalculateDistanceMeters_ShouldReturnPositiveNumber_WhenTwoPointsAreDifferent()
    {
        var calculator = new GeoDistanceCalculator();

        var distance = calculator.CalculateDistanceMeters(
            10.7569m,
            106.7057m,
            10.7572m,
            106.7061m
        );

        Convert.ToDouble(distance).Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateDistanceMeters_ShouldReturnReasonableDistance_ForNearbyVinhKhanhPoints()
    {
        var calculator = new GeoDistanceCalculator();

        var distance = calculator.CalculateDistanceMeters(
            10.7569m,
            106.7057m,
            10.7572m,
            106.7061m
        );

        Convert.ToDouble(distance).Should().BeLessThan(100);
    }
}
