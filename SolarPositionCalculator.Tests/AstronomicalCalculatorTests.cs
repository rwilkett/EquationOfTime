using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using Xunit;

namespace SolarPositionCalculator.Tests;

/// <summary>
/// Unit tests for the AstronomicalCalculator class
/// Tests solar position calculations against known reference values and validates
/// equation of time calculations with NOAA data
/// </summary>
public class AstronomicalCalculatorTests
{
    private readonly IAstronomicalCalculator _calculator;

    public AstronomicalCalculatorTests()
    {
        _calculator = new AstronomicalCalculator();
    }

    #region Julian Day Tests

    [Fact]
    public void CalculateJulianDay_J2000Epoch_ReturnsCorrectValue()
    {
        // Arrange: January 1, 2000, 12:00 UTC (J2000.0 epoch)
        var j2000Date = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        double julianDay = _calculator.CalculateJulianDay(j2000Date);

        // Assert: Should equal the J2000 constant
        Assert.Equal(AstronomicalConstants.J2000, julianDay, 6);
    }

    [Fact]
    public void CalculateJulianDay_KnownDates_ReturnsCorrectValues()
    {
        // Test cases from astronomical references
        var testCases = new[]
        {
            new { Date = new DateTime(1985, 2, 17, 6, 0, 0, DateTimeKind.Utc), Expected = 2446113.75 },
            new { Date = new DateTime(1987, 1, 27, 0, 0, 0, DateTimeKind.Utc), Expected = 2446822.5 },
            new { Date = new DateTime(1988, 1, 27, 0, 0, 0, DateTimeKind.Utc), Expected = 2447187.5 }
        };

        foreach (var testCase in testCases)
        {
            // Act
            double julianDay = _calculator.CalculateJulianDay(testCase.Date);

            // Assert
            Assert.Equal(testCase.Expected, julianDay, 6);
        }
    }

    #endregion

    #region Solar Declination Tests

    [Fact]
    public void CalculateSolarDeclination_Equinoxes_ReturnsNearZero()
    {
        // Arrange: Spring and autumn equinoxes (approximately March 20 and September 22)
        var springEquinox = new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        var autumnEquinox = new DateTime(2024, 9, 22, 12, 0, 0, DateTimeKind.Utc);

        // Act
        double springDeclination = _calculator.CalculateSolarDeclination(_calculator.CalculateJulianDay(springEquinox));
        double autumnDeclination = _calculator.CalculateSolarDeclination(_calculator.CalculateJulianDay(autumnEquinox));

        // Assert: Declination should be close to 0° at equinoxes
        Assert.True(Math.Abs(springDeclination) < 2.0, $"Spring equinox declination should be near 0°, got {springDeclination:F2}°");
        Assert.True(Math.Abs(autumnDeclination) < 2.0, $"Autumn equinox declination should be near 0°, got {autumnDeclination:F2}°");
    }

    [Fact]
    public void CalculateSolarDeclination_Solstices_ReturnsMaximumValues()
    {
        // Arrange: Summer and winter solstices (approximately June 21 and December 21)
        var summerSolstice = new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc);
        var winterSolstice = new DateTime(2024, 12, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act
        double summerDeclination = _calculator.CalculateSolarDeclination(_calculator.CalculateJulianDay(summerSolstice));
        double winterDeclination = _calculator.CalculateSolarDeclination(_calculator.CalculateJulianDay(winterSolstice));

        // Assert: Declination should be close to ±23.4° at solstices
        Assert.True(Math.Abs(summerDeclination - 23.4) < 1.0, $"Summer solstice declination should be near +23.4°, got {summerDeclination:F2}°");
        Assert.True(Math.Abs(winterDeclination + 23.4) < 1.0, $"Winter solstice declination should be near -23.4°, got {winterDeclination:F2}°");
    }

    #endregion

    #region Solar Position Tests - Known Reference Values

    [Fact]
    public void CalculateSolarPosition_GreenwichNoon_ReturnsCorrectValues()
    {
        // Arrange: Greenwich Observatory at solar noon on spring equinox
        var location = new GeographicCoordinate(51.4769, 0.0); // Greenwich
        var date = new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: At solar noon, azimuth should be close to 180° (south)
        // Elevation should be approximately 90° - latitude + declination
        Assert.True(Math.Abs(position.Azimuth - 180) < 5, $"Azimuth should be near 180° at solar noon, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation > 35 && position.Elevation < 45, $"Elevation should be between 35-45° at Greenwich equinox noon, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_MatchesExpectedValues()
    {
        // Test case based on NOAA Solar Position Calculator
        // Location: Denver, CO (39.7392°N, 104.9903°W)
        // Date: July 4, 2024, 12:00 PM MDT (18:00 UTC)
        var location = new GeographicCoordinate(39.7392, -104.9903);
        var date = new DateTime(2024, 7, 4, 18, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: Expected values from NOAA calculator (with tolerance for algorithm differences)
        // Expected: Azimuth ≈ 180°, Elevation ≈ 73°
        Assert.True(Math.Abs(position.Azimuth - 180) < 10, $"Azimuth should be near 180°, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation > 65 && position.Elevation < 80, $"Elevation should be between 65-80°, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_MultipleLocations_ReturnsValidResults()
    {
        // Test multiple locations around the world
        var testLocations = new[]
        {
            new { Name = "London", Coord = new GeographicCoordinate(51.5074, -0.1278) },
            new { Name = "Sydney", Coord = new GeographicCoordinate(-33.8688, 151.2093) },
            new { Name = "New York", Coord = new GeographicCoordinate(40.7128, -74.0060) },
            new { Name = "Tokyo", Coord = new GeographicCoordinate(35.6762, 139.6503) }
        };

        var testDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        foreach (var location in testLocations)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location.Coord, testDate);

            // Assert: Basic validity checks
            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360,
                $"{location.Name}: Azimuth should be 0-360°, got {position.Azimuth:F2}°");
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90,
                $"{location.Name}: Elevation should be -90 to +90°, got {position.Elevation:F2}°");
            Assert.Equal(location.Coord, position.Location);
            Assert.Equal(testDate, position.Timestamp);
        }
    }

    #endregion

    #region Equation of Time Tests

    [Fact]
    public void CalculateEquationOfTime_KnownDates_ReturnsExpectedValues()
    {
        // Test cases based on NOAA data for 2024
        var testCases = new[]
        {
            new { Date = new DateTime(2024, 2, 11), ExpectedMin = -14.5, ExpectedMax = -13.5 }, // Minimum around Feb 11
            new { Date = new DateTime(2024, 5, 14), ExpectedMin = 3.5, ExpectedMax = 4.5 },     // Maximum around May 14
            new { Date = new DateTime(2024, 7, 26), ExpectedMin = -6.5, ExpectedMax = -5.5 },   // Minimum around July 26
            new { Date = new DateTime(2024, 11, 3), ExpectedMin = 16.0, ExpectedMax = 17.0 }    // Maximum around Nov 3
        };

        foreach (var testCase in testCases)
        {
            // Act
            double equationOfTime = _calculator.CalculateEquationOfTime(testCase.Date);

            // Assert
            Assert.True(equationOfTime >= testCase.ExpectedMin && equationOfTime <= testCase.ExpectedMax,
                $"Equation of time on {testCase.Date:yyyy-MM-dd} should be between {testCase.ExpectedMin} and {testCase.ExpectedMax} minutes, got {equationOfTime:F2}");
        }
    }

    [Fact]
    public void CalculateEquationOfTime_AnnualRange_WithinExpectedBounds()
    {
        // Test that equation of time stays within expected annual range
        var year = 2024;
        var minValue = double.MaxValue;
        var maxValue = double.MinValue;

        // Sample every 10 days throughout the year
        for (int dayOfYear = 1; dayOfYear <= 365; dayOfYear += 10)
        {
            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            double equationOfTime = _calculator.CalculateEquationOfTime(date);

            minValue = Math.Min(minValue, equationOfTime);
            maxValue = Math.Max(maxValue, equationOfTime);
        }

        // Assert: Annual range should be approximately -16 to +14 minutes
        Assert.True(minValue >= -17 && minValue <= -13, $"Minimum equation of time should be around -16 minutes, got {minValue:F2}");
        Assert.True(maxValue >= 13 && maxValue <= 17, $"Maximum equation of time should be around +14 minutes, got {maxValue:F2}");
    }

    [Fact]
    public void CalculateAnnualEquationOfTime_ReturnsCorrectDataPoints()
    {
        // Act
        var annualData = _calculator.CalculateAnnualEquationOfTime(2024);

        // Assert
        Assert.Equal(366, annualData.Length); // 2024 is a leap year
        Assert.Equal(new DateTime(2024, 1, 1), annualData[0].Date);
        Assert.Equal(new DateTime(2024, 12, 31), annualData[^1].Date);

        // Check that all values are within expected range
        foreach (var data in annualData)
        {
            Assert.True(data.Minutes >= -17 && data.Minutes <= 17,
                $"Equation of time on {data.Date:yyyy-MM-dd} should be within ±17 minutes, got {data.Minutes:F2}");
        }
    }

    #endregion

    #region Edge Cases and Polar Region Tests

    [Fact]
    public void CalculateSolarPosition_PolarRegions_HandlesExtremeLatitudes()
    {
        // Test locations in polar regions
        var arcticLocation = new GeographicCoordinate(80.0, 0.0);  // High Arctic
        var antarcticLocation = new GeographicCoordinate(-80.0, 0.0); // Antarctica

        var summerDate = new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc);
        var winterDate = new DateTime(2024, 12, 21, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert: Should not throw exceptions
        var arcticSummer = _calculator.CalculateSolarPosition(arcticLocation, summerDate);
        var arcticWinter = _calculator.CalculateSolarPosition(arcticLocation, winterDate);
        var antarcticSummer = _calculator.CalculateSolarPosition(antarcticLocation, summerDate);
        var antarcticWinter = _calculator.CalculateSolarPosition(antarcticLocation, winterDate);

        // Validate results are within expected ranges
        Assert.True(arcticSummer.Elevation >= -90 && arcticSummer.Elevation <= 90);
        Assert.True(arcticWinter.Elevation >= -90 && arcticWinter.Elevation <= 90);
        Assert.True(antarcticSummer.Elevation >= -90 && antarcticSummer.Elevation <= 90);
        Assert.True(antarcticWinter.Elevation >= -90 && antarcticWinter.Elevation <= 90);
    }

    [Fact]
    public void IsPolarRegion_VariousLatitudes_ReturnsCorrectResults()
    {
        var testCases = new[]
        {
            new { Coord = new GeographicCoordinate(70.0, 0.0), Expected = true },   // Arctic
            new { Coord = new GeographicCoordinate(-70.0, 0.0), Expected = true },  // Antarctic
            new { Coord = new GeographicCoordinate(60.0, 0.0), Expected = false },  // Below Arctic Circle
            new { Coord = new GeographicCoordinate(-60.0, 0.0), Expected = false }, // Above Antarctic Circle
            new { Coord = new GeographicCoordinate(0.0, 0.0), Expected = false },   // Equator
            new { Coord = new GeographicCoordinate(66.5, 0.0), Expected = true },   // Exactly on Arctic Circle
            new { Coord = new GeographicCoordinate(-66.5, 0.0), Expected = true }   // Exactly on Antarctic Circle
        };

        foreach (var testCase in testCases)
        {
            // Act
            bool result = _calculator.IsPolarRegion(testCase.Coord);

            // Assert
            Assert.Equal(testCase.Expected, result);
        }
    }

    [Fact]
    public void GetPolarCondition_MidnightSun_DetectedCorrectly()
    {
        // Arrange: High Arctic location during summer
        var arcticLocation = new GeographicCoordinate(75.0, 0.0);
        var summerDate = new DateTime(2024, 6, 21); // Summer solstice

        // Act
        var condition = _calculator.GetPolarCondition(arcticLocation, summerDate);

        // Assert
        Assert.True(condition.IsPolarRegion);
        Assert.True(condition.MaxElevation > 0, "Should have positive elevation during midnight sun");
        Assert.True(condition.RequiresSpecialVisualization);
    }

    [Fact]
    public void GetPolarCondition_PolarNight_DetectedCorrectly()
    {
        // Arrange: High Arctic location during winter
        var arcticLocation = new GeographicCoordinate(75.0, 0.0);
        var winterDate = new DateTime(2024, 12, 21); // Winter solstice

        // Act
        var condition = _calculator.GetPolarCondition(arcticLocation, winterDate);

        // Assert
        Assert.True(condition.IsPolarRegion);
        Assert.True(condition.MaxElevation < 0, "Should have negative elevation during polar night");
        Assert.True(condition.RequiresSpecialVisualization);
    }

    #endregion

    #region Input Validation Tests

    [Fact]
    public void CalculateSolarPosition_InvalidCoordinates_ThrowsArgumentException()
    {
        // Arrange
        var invalidCoordinates = new[]
        {
            new GeographicCoordinate(91.0, 0.0),    // Latitude too high
            new GeographicCoordinate(-91.0, 0.0),   // Latitude too low
            new GeographicCoordinate(0.0, 181.0),   // Longitude too high
            new GeographicCoordinate(0.0, -181.0)   // Longitude too low
        };

        var testDate = DateTime.UtcNow;

        foreach (var coord in invalidCoordinates)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _calculator.CalculateSolarPosition(coord, testDate));
        }
    }

    [Fact]
    public void CalculateDailySunPath_InvalidCoordinates_ThrowsArgumentException()
    {
        // Arrange
        var invalidCoordinate = new GeographicCoordinate(100.0, 0.0);
        var testDate = DateTime.Today;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _calculator.CalculateDailySunPath(invalidCoordinate, testDate));
    }

    [Fact]
    public void GetPolarCondition_InvalidCoordinates_ThrowsArgumentException()
    {
        // Arrange
        var invalidCoordinate = new GeographicCoordinate(0.0, 200.0);
        var testDate = DateTime.Today;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _calculator.GetPolarCondition(invalidCoordinate, testDate));
    }

    #endregion

    #region Sun Path Tests

    [Fact]
    public void CalculateDailySunPath_NormalLocation_ReturnsValidPath()
    {
        // Arrange: Mid-latitude location
        var location = new GeographicCoordinate(40.0, -74.0); // New York area
        var date = new DateTime(2024, 6, 15); // Summer

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, date);

        // Assert
        Assert.Equal(location, sunPath.Location);
        Assert.Equal(date, sunPath.Date);
        Assert.True(sunPath.DailyPositions.Length > 0);
        Assert.True(sunPath.HasSunrise);
        Assert.True(sunPath.HasSunset);
        Assert.NotNull(sunPath.Sunrise);
        Assert.NotNull(sunPath.Sunset);

        // Sunrise should be before sunset
        Assert.True(sunPath.Sunrise.Timestamp < sunPath.Sunset.Timestamp);
    }

    [Fact]
    public void CalculateDailySunPath_EquatorLocation_HasNearEqualDayNight()
    {
        // Arrange: Equatorial location
        var location = new GeographicCoordinate(0.0, 0.0);
        var equinoxDate = new DateTime(2024, 3, 20); // Spring equinox

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, equinoxDate);

        // Assert
        Assert.True(sunPath.HasSunrise);
        Assert.True(sunPath.HasSunset);

        if (sunPath.Sunrise != null && sunPath.Sunset != null)
        {
            var daylightDuration = sunPath.Sunset.Timestamp - sunPath.Sunrise.Timestamp;
            // At equator on equinox, daylight should be close to 12 hours
            Assert.True(Math.Abs(daylightDuration.TotalHours - 12) < 1.0,
                $"Daylight duration at equator should be near 12 hours, got {daylightDuration.TotalHours:F2}");
        }
    }

    #endregion

    #region Performance and Precision Tests

    [Fact]
    public void CalculateSolarPosition_RepeatedCalls_ConsistentResults()
    {
        // Arrange
        var location = new GeographicCoordinate(51.5074, -0.1278); // London
        var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act: Calculate same position multiple times
        var results = new SolarPosition[10];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = _calculator.CalculateSolarPosition(location, date);
        }

        // Assert: All results should be identical
        for (int i = 1; i < results.Length; i++)
        {
            Assert.Equal(results[0].Azimuth, results[i].Azimuth, 10);
            Assert.Equal(results[0].Elevation, results[i].Elevation, 10);
        }
    }

    [Fact]
    public void CalculateEquationOfTime_HighPrecision_MaintainsAccuracy()
    {
        // Test precision with dates very close together
        var baseDate = new DateTime(2024, 6, 15);
        var nextDate = baseDate.AddHours(1);

        var eot1 = _calculator.CalculateEquationOfTime(baseDate);
        var eot2 = _calculator.CalculateEquationOfTime(nextDate);

        // The difference should be very small for dates 1 hour apart
        var difference = Math.Abs(eot2 - eot1);
        Assert.True(difference < 0.1, $"Equation of time should change slowly, difference was {difference:F4} minutes");
    }

    #endregion
}