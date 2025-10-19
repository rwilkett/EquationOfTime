using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using Xunit;

namespace SolarPositionCalculator.Tests;

/// <summary>
/// Basic tests for astronomical calculations focusing on core functionality
/// and reasonable bounds rather than exact NOAA reference matching
/// </summary>
public class BasicAstronomicalTests
{
    private readonly IAstronomicalCalculator _calculator;

    public BasicAstronomicalTests()
    {
        _calculator = new AstronomicalCalculator();
    }

    #region Core Functionality Tests

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
    public void CalculateSolarPosition_ValidInputs_ReturnsValidResults()
    {
        // Arrange
        var location = new GeographicCoordinate(40.7128, -74.0060); // New York
        var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: Basic validity checks
        Assert.True(position.Azimuth >= 0 && position.Azimuth < 360,
            $"Azimuth should be 0-360°, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation >= -90 && position.Elevation <= 90,
            $"Elevation should be -90 to +90°, got {position.Elevation:F2}°");
        Assert.Equal(location, position.Location);
        Assert.Equal(date, position.Timestamp);
    }

    [Fact]
    public void CalculateSolarPosition_MultipleLocations_ReturnsValidResults()
    {
        // Test multiple locations around the world
        var testLocations = new[]
        {
            new GeographicCoordinate(51.5074, -0.1278),  // London
            new GeographicCoordinate(-33.8688, 151.2093), // Sydney
            new GeographicCoordinate(35.6762, 139.6503),  // Tokyo
            new GeographicCoordinate(0.0, 0.0)            // Equator
        };

        var testDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        foreach (var location in testLocations)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(location, testDate);

            // Assert: Basic validity checks
            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360);
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90);
            Assert.Equal(location, position.Location);
            Assert.Equal(testDate, position.Timestamp);
        }
    }

    [Fact]
    public void CalculateEquationOfTime_AnnualRange_WithinExpectedBounds()
    {
        // Test that equation of time stays within expected annual range
        var year = 2024;
        var minValue = double.MaxValue;
        var maxValue = double.MinValue;

        // Sample every 30 days throughout the year
        for (int dayOfYear = 1; dayOfYear <= 365; dayOfYear += 30)
        {
            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            double equationOfTime = _calculator.CalculateEquationOfTime(date);

            minValue = Math.Min(minValue, equationOfTime);
            maxValue = Math.Max(maxValue, equationOfTime);
        }

        // Assert: Annual range should be approximately -16 to +16 minutes (with some tolerance)
        Assert.True(minValue >= -20 && minValue <= -10, $"Minimum equation of time should be reasonable, got {minValue:F2}");
        Assert.True(maxValue >= 10 && maxValue <= 20, $"Maximum equation of time should be reasonable, got {maxValue:F2}");
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
        Assert.True(Math.Abs(springDeclination) < 3.0, $"Spring equinox declination should be near 0°, got {springDeclination:F2}°");
        Assert.True(Math.Abs(autumnDeclination) < 3.0, $"Autumn equinox declination should be near 0°, got {autumnDeclination:F2}°");
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
        Assert.True(Math.Abs(summerDeclination - 23.4) < 2.0, $"Summer solstice declination should be near +23.4°, got {summerDeclination:F2}°");
        Assert.True(Math.Abs(winterDeclination + 23.4) < 2.0, $"Winter solstice declination should be near -23.4°, got {winterDeclination:F2}°");
    }

    #endregion

    #region Polar Region Tests

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

        // For mid-latitudes in summer, should have sunrise and sunset
        if (sunPath.HasSunrise && sunPath.HasSunset)
        {
            Assert.NotNull(sunPath.Sunrise);
            Assert.NotNull(sunPath.Sunset);
            Assert.True(sunPath.Sunrise.Timestamp < sunPath.Sunset.Timestamp);
        }
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

        // Check that all values are within reasonable range
        foreach (var data in annualData)
        {
            Assert.True(data.Minutes >= -20 && data.Minutes <= 20,
                $"Equation of time on {data.Date:yyyy-MM-dd} should be within ±20 minutes, got {data.Minutes:F2}");
        }
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void CalculateSolarPosition_RepeatedCalls_ConsistentResults()
    {
        // Arrange
        var location = new GeographicCoordinate(51.5074, -0.1278); // London
        var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act: Calculate same position multiple times
        var results = new SolarPosition[5];
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
        Assert.True(difference < 0.2, $"Equation of time should change slowly, difference was {difference:F4} minutes");
    }

    #endregion

    #region Seasonal Behavior Tests

    [Fact]
    public void CalculateSolarPosition_SeasonalVariation_ShowsExpectedPatterns()
    {
        // Test the same location at different seasons
        var location = new GeographicCoordinate(45.0, 0.0); // Mid-latitude
        var noonTime = new TimeSpan(12, 0, 0);

        var winterSolstice = new DateTime(2024, 12, 21).Add(noonTime);
        var springEquinox = new DateTime(2024, 3, 20).Add(noonTime);
        var summerSolstice = new DateTime(2024, 6, 21).Add(noonTime);
        var autumnEquinox = new DateTime(2024, 9, 22).Add(noonTime);

        // Act
        var winterPos = _calculator.CalculateSolarPosition(location, winterSolstice);
        var springPos = _calculator.CalculateSolarPosition(location, springEquinox);
        var summerPos = _calculator.CalculateSolarPosition(location, summerSolstice);
        var autumnPos = _calculator.CalculateSolarPosition(location, autumnEquinox);

        // Assert: Summer should have highest elevation, winter lowest
        Assert.True(summerPos.Elevation > springPos.Elevation, "Summer should be higher than spring");
        Assert.True(summerPos.Elevation > autumnPos.Elevation, "Summer should be higher than autumn");
        Assert.True(summerPos.Elevation > winterPos.Elevation, "Summer should be higher than winter");
        Assert.True(winterPos.Elevation < springPos.Elevation, "Winter should be lower than spring");
        Assert.True(winterPos.Elevation < autumnPos.Elevation, "Winter should be lower than autumn");
    }

    #endregion
}