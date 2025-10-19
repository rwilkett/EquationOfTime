using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using Xunit;

namespace SolarPositionCalculator.Tests;

/// <summary>
/// Specialized tests for polar region calculations and edge cases
/// Tests midnight sun, polar night, and extreme latitude conditions
/// </summary>
public class PolarRegionTests
{
    private readonly IAstronomicalCalculator _calculator;

    public PolarRegionTests()
    {
        _calculator = new AstronomicalCalculator();
    }

    #region Arctic Region Tests

    [Theory]
    [InlineData(70.0, 0.0)]   // Svalbard
    [InlineData(78.0, 15.0)]  // Ny-Ålesund
    [InlineData(82.0, -82.0)] // Northern Greenland
    public void CalculateSolarPosition_HighArctic_SummerMidnightSun(double latitude, double longitude)
    {
        // Arrange: High Arctic locations during summer solstice
        var location = new GeographicCoordinate(latitude, longitude);
        var summerSolstice = new DateTime(2024, 6, 21);

        // Test multiple times throughout the day
        var testTimes = new[]
        {
            summerSolstice.AddHours(0),   // Midnight
            summerSolstice.AddHours(6),   // 6 AM
            summerSolstice.AddHours(12),  // Noon
            summerSolstice.AddHours(18)   // 6 PM
        };

        foreach (var testTime in testTimes)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location, testTime);

            // Assert: Sun should be above horizon all day (midnight sun)
            Assert.True(position.IsSunVisible,
                $"Sun should be visible at {latitude}°N on summer solstice at {testTime.Hour}:00, elevation: {position.Elevation:F2}°");
            Assert.True(position.Elevation > 0,
                $"Elevation should be positive during midnight sun, got {position.Elevation:F2}°");
        }
    }

    [Theory]
    [InlineData(70.0, 0.0)]
    [InlineData(75.0, 10.0)]
    [InlineData(80.0, -15.0)]
    public void CalculateSolarPosition_HighArctic_WinterPolarNight(double latitude, double longitude)
    {
        // Arrange: High Arctic locations during winter solstice
        var location = new GeographicCoordinate(latitude, longitude);
        var winterSolstice = new DateTime(2024, 12, 21);

        // Test multiple times throughout the day
        var testTimes = new[]
        {
            winterSolstice.AddHours(0),   // Midnight
            winterSolstice.AddHours(6),   // 6 AM
            winterSolstice.AddHours(12),  // Noon
            winterSolstice.AddHours(18)   // 6 PM
        };

        foreach (var testTime in testTimes)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location, testTime);

            // Assert: Sun should be below horizon all day (polar night)
            Assert.False(position.IsSunVisible,
                $"Sun should not be visible at {latitude}°N on winter solstice at {testTime.Hour}:00, elevation: {position.Elevation:F2}°");
            Assert.True(position.Elevation < 0,
                $"Elevation should be negative during polar night, got {position.Elevation:F2}°");
        }
    }

    #endregion

    #region Antarctic Region Tests

    [Theory]
    [InlineData(-70.0, 0.0)]    // Antarctic coast
    [InlineData(-78.0, 166.0)]  // McMurdo Station area
    [InlineData(-85.0, 0.0)]    // Near South Pole
    public void CalculateSolarPosition_Antarctica_WinterMidnightSun(double latitude, double longitude)
    {
        // Arrange: Antarctic locations during their summer (Northern winter)
        var location = new GeographicCoordinate(latitude, longitude);
        var antarcticSummer = new DateTime(2024, 12, 21); // Winter solstice = Antarctic summer

        // Test multiple times throughout the day
        var testTimes = new[]
        {
            antarcticSummer.AddHours(0),   // Midnight
            antarcticSummer.AddHours(6),   // 6 AM
            antarcticSummer.AddHours(12),  // Noon
            antarcticSummer.AddHours(18)   // 6 PM
        };

        foreach (var testTime in testTimes)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location, testTime);

            // Assert: Sun should be above horizon all day during Antarctic summer
            Assert.True(position.IsSunVisible,
                $"Sun should be visible at {latitude}°S during Antarctic summer at {testTime.Hour}:00, elevation: {position.Elevation:F2}°");
        }
    }

    [Theory]
    [InlineData(-70.0, 0.0)]
    [InlineData(-75.0, 10.0)]
    [InlineData(-80.0, -15.0)]
    public void CalculateSolarPosition_Antarctica_SummerPolarNight(double latitude, double longitude)
    {
        // Arrange: Antarctic locations during their winter (Northern summer)
        var location = new GeographicCoordinate(latitude, longitude);
        var antarcticWinter = new DateTime(2024, 6, 21); // Summer solstice = Antarctic winter

        // Test multiple times throughout the day
        var testTimes = new[]
        {
            antarcticWinter.AddHours(0),   // Midnight
            antarcticWinter.AddHours(6),   // 6 AM
            antarcticWinter.AddHours(12),  // Noon
            antarcticWinter.AddHours(18)   // 6 PM
        };

        foreach (var testTime in testTimes)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location, testTime);

            // Assert: Sun should be below horizon all day during Antarctic winter
            Assert.False(position.IsSunVisible,
                $"Sun should not be visible at {latitude}°S during Antarctic winter at {testTime.Hour}:00, elevation: {position.Elevation:F2}°");
        }
    }

    #endregion

    #region Polar Condition Detection Tests

    [Fact]
    public void GetPolarCondition_ArcticMidnightSun_ReturnsCorrectCondition()
    {
        // Arrange
        var svalbard = new GeographicCoordinate(78.9, 11.9); // Longyearbyen
        var summerDate = new DateTime(2024, 6, 21);

        // Act
        var condition = _calculator.GetPolarCondition(svalbard, summerDate);

        // Assert
        Assert.True(condition.IsPolarRegion);
        Assert.Equal(PolarConditionType.MidnightSun, condition.Type);
        Assert.True(condition.MaxElevation > 0);
        Assert.True(condition.MinElevation > 0); // Sun never sets
        Assert.Equal(TimeSpan.FromHours(24), condition.DaylightDuration);
        Assert.True(condition.RequiresSpecialVisualization);
    }

    [Fact]
    public void GetPolarCondition_ArcticPolarNight_ReturnsCorrectCondition()
    {
        // Arrange
        var svalbard = new GeographicCoordinate(78.9, 11.9); // Longyearbyen
        var winterDate = new DateTime(2024, 12, 21);

        // Act
        var condition = _calculator.GetPolarCondition(svalbard, winterDate);

        // Assert
        Assert.True(condition.IsPolarRegion);
        Assert.True(condition.Type == PolarConditionType.PolarNight ||
                   condition.Type == PolarConditionType.CivilTwilight ||
                   condition.Type == PolarConditionType.NauticalTwilight ||
                   condition.Type == PolarConditionType.AstronomicalTwilight);
        Assert.True(condition.MaxElevation < 0);
        Assert.True(condition.MinElevation < 0);
        Assert.Equal(TimeSpan.Zero, condition.DaylightDuration);
        Assert.True(condition.RequiresSpecialVisualization);
    }

    [Fact]
    public void GetPolarCondition_TwilightConditions_ClassifiedCorrectly()
    {
        // Test different twilight conditions based on maximum elevation
        var testCases = new[]
        {
            new { Location = new GeographicCoordinate(70.0, 0.0), Date = new DateTime(2024, 11, 15) },
            new { Location = new GeographicCoordinate(72.0, 0.0), Date = new DateTime(2024, 12, 1) },
            new { Location = new GeographicCoordinate(75.0, 0.0), Date = new DateTime(2024, 1, 15) }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var condition = _calculator.GetPolarCondition(testCase.Location, testCase.Date);

            // Assert
            if (condition.MaxElevation > -6 && condition.MaxElevation < 0)
            {
                Assert.Equal(PolarConditionType.CivilTwilight, condition.Type);
            }
            else if (condition.MaxElevation > -12 && condition.MaxElevation <= -6)
            {
                Assert.Equal(PolarConditionType.NauticalTwilight, condition.Type);
            }
            else if (condition.MaxElevation > -18 && condition.MaxElevation <= -12)
            {
                Assert.Equal(PolarConditionType.AstronomicalTwilight, condition.Type);
            }
            else if (condition.MaxElevation <= -18)
            {
                Assert.Equal(PolarConditionType.PolarNight, condition.Type);
            }
        }
    }

    #endregion

    #region Sun Path Tests for Polar Regions

    [Fact]
    public void CalculateDailySunPath_PolarMidnightSun_NoSunriseOrSunset()
    {
        // Arrange: High Arctic during midnight sun period
        var location = new GeographicCoordinate(80.0, 0.0);
        var summerDate = new DateTime(2024, 6, 21);

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, summerDate);

        // Assert
        Assert.False(sunPath.HasSunrise, "Should not have sunrise during midnight sun");
        Assert.False(sunPath.HasSunset, "Should not have sunset during midnight sun");
        Assert.Null(sunPath.Sunrise);
        Assert.Null(sunPath.Sunset);
        Assert.True(sunPath.IsPolarDay, "Should be identified as polar day");

        // All positions should be above horizon
        Assert.True(sunPath.DailyPositions.All(p => p.IsSunVisible),
            "All positions should be above horizon during midnight sun");
    }

    [Fact]
    public void CalculateDailySunPath_PolarNight_NoSunriseOrSunset()
    {
        // Arrange: High Arctic during polar night period
        var location = new GeographicCoordinate(80.0, 0.0);
        var winterDate = new DateTime(2024, 12, 21);

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, winterDate);

        // Assert
        Assert.False(sunPath.HasSunrise, "Should not have sunrise during polar night");
        Assert.False(sunPath.HasSunset, "Should not have sunset during polar night");
        Assert.Null(sunPath.Sunrise);
        Assert.Null(sunPath.Sunset);
        Assert.True(sunPath.IsPolarNight, "Should be identified as polar night");

        // All positions should be below horizon
        Assert.True(sunPath.DailyPositions.All(p => !p.IsSunVisible),
            "All positions should be below horizon during polar night");
    }

    #endregion

    #region Extreme Date Tests

    [Fact]
    public void CalculateSolarPosition_ExtremeHistoricalDates_HandlesCorrectly()
    {
        // Test with historical dates
        var location = new GeographicCoordinate(60.0, 0.0);
        var extremeDates = new[]
        {
            new DateTime(1900, 1, 1),
            new DateTime(1950, 6, 15),
            new DateTime(2000, 1, 1), // J2000 epoch
            new DateTime(2050, 12, 31),
            new DateTime(2100, 6, 21)
        };

        foreach (var date in extremeDates)
        {
            // Act & Assert: Should not throw exceptions
            var position = _calculator.CalculateSolarPosition(location, date);

            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360);
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90);
            Assert.Equal(date, position.Timestamp);
        }
    }

    [Fact]
    public void CalculateEquationOfTime_ExtremeYears_RemainsWithinBounds()
    {
        // Test equation of time for extreme years
        var extremeYears = new[] { 1900, 1950, 2000, 2050, 2100 };

        foreach (var year in extremeYears)
        {
            // Act
            var annualData = _calculator.CalculateAnnualEquationOfTime(year);

            // Assert
            var minValue = annualData.Min(d => d.Minutes);
            var maxValue = annualData.Max(d => d.Minutes);

            Assert.True(minValue >= -20 && minValue <= -10,
                $"Minimum equation of time for year {year} should be reasonable, got {minValue:F2}");
            Assert.True(maxValue >= 10 && maxValue <= 20,
                $"Maximum equation of time for year {year} should be reasonable, got {maxValue:F2}");
        }
    }

    #endregion

    #region Precision Tests for Extreme Conditions

    [Fact]
    public void CalculateSolarPosition_NearPoles_MaintainsPrecision()
    {
        // Test very close to the poles
        var nearNorthPole = new GeographicCoordinate(89.9, 0.0);
        var nearSouthPole = new GeographicCoordinate(-89.9, 0.0);
        var testDate = new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var northPosition = _calculator.CalculateSolarPosition(nearNorthPole, testDate);
        var southPosition = _calculator.CalculateSolarPosition(nearSouthPole, testDate);

        // Assert: Should produce valid results even near poles
        Assert.True(northPosition.Azimuth >= 0 && northPosition.Azimuth < 360);
        Assert.True(southPosition.Azimuth >= 0 && southPosition.Azimuth < 360);
        Assert.True(northPosition.Elevation >= -90 && northPosition.Elevation <= 90);
        Assert.True(southPosition.Elevation >= -90 && southPosition.Elevation <= 90);

        // At summer solstice, North Pole should have high elevation, South Pole should be low
        Assert.True(northPosition.Elevation > southPosition.Elevation,
            "North Pole should have higher elevation than South Pole during northern summer");
    }

    [Fact]
    public void CalculateSolarPosition_DateLineCrossing_HandlesCorrectly()
    {
        // Test locations across the International Date Line
        var westOfDateLine = new GeographicCoordinate(60.0, 179.0);
        var eastOfDateLine = new GeographicCoordinate(60.0, -179.0);
        var testDate = new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var westPosition = _calculator.CalculateSolarPosition(westOfDateLine, testDate);
        var eastPosition = _calculator.CalculateSolarPosition(eastOfDateLine, testDate);

        // Assert: Results should be reasonable and similar (locations are close)
        Assert.True(Math.Abs(westPosition.Elevation - eastPosition.Elevation) < 5,
            "Elevations should be similar for nearby locations across date line");

        // Azimuths might differ more due to longitude difference, but should be valid
        Assert.True(westPosition.Azimuth >= 0 && westPosition.Azimuth < 360);
        Assert.True(eastPosition.Azimuth >= 0 && eastPosition.Azimuth < 360);
    }

    #endregion
}