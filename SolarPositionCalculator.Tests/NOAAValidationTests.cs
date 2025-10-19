using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using Xunit;

namespace SolarPositionCalculator.Tests;

/// <summary>
/// Tests that validate astronomical calculations against NOAA Solar Position Calculator reference data
/// These tests use known values from the NOAA SPA (Solar Position Algorithm) for accuracy validation
/// </summary>
public class NOAAValidationTests
{
    private readonly IAstronomicalCalculator _calculator;

    public NOAAValidationTests()
    {
        _calculator = new AstronomicalCalculator();
    }

    #region NOAA Reference Solar Position Tests

    [Fact]
    public void CalculateSolarPosition_NOAAReference_Denver2024()
    {
        // NOAA SPA reference data for Denver, CO
        // Location: 39.7392°N, 104.9903°W
        // Date: July 4, 2024, 12:00 PM MDT (18:00 UTC)
        // Expected: Azimuth ≈ 180.0°, Elevation ≈ 73.6°

        var location = new GeographicCoordinate(39.7392, -104.9903);
        var date = new DateTime(2024, 7, 4, 18, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert with reasonable tolerance for algorithm differences
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 5.0,
            $"Azimuth should be near 180°, got {position.Azimuth:F2}°");
        Assert.True(Math.Abs(position.Elevation - 73.6) < 3.0,
            $"Elevation should be near 73.6°, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_NewYork2024()
    {
        // NOAA SPA reference data for New York, NY
        // Location: 40.7128°N, 74.0060°W
        // Date: March 20, 2024, 12:00 PM EDT (16:00 UTC) - Spring Equinox
        // Expected: Azimuth ≈ 180.0°, Elevation ≈ 49.3°

        var location = new GeographicCoordinate(40.7128, -74.0060);
        var date = new DateTime(2024, 3, 20, 16, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 5.0,
            $"Azimuth should be near 180°, got {position.Azimuth:F2}°");
        Assert.True(Math.Abs(position.Elevation - 49.3) < 3.0,
            $"Elevation should be near 49.3°, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_LosAngeles2024()
    {
        // NOAA SPA reference data for Los Angeles, CA
        // Location: 34.0522°N, 118.2437°W
        // Date: December 21, 2024, 12:00 PM PST (20:00 UTC) - Winter Solstice
        // Expected: Azimuth ≈ 180.0°, Elevation ≈ 32.6°

        var location = new GeographicCoordinate(34.0522, -118.2437);
        var date = new DateTime(2024, 12, 21, 20, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 5.0,
            $"Azimuth should be near 180°, got {position.Azimuth:F2}°");
        Assert.True(Math.Abs(position.Elevation - 32.6) < 3.0,
            $"Elevation should be near 32.6°, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_Miami2024()
    {
        // NOAA SPA reference data for Miami, FL
        // Location: 25.7617°N, 80.1918°W
        // Date: June 21, 2024, 12:00 PM EDT (16:00 UTC) - Summer Solstice
        // Expected: Azimuth ≈ 180.0°, Elevation ≈ 87.8°

        var location = new GeographicCoordinate(25.7617, -80.1918);
        var date = new DateTime(2024, 6, 21, 16, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 5.0,
            $"Azimuth should be near 180°, got {position.Azimuth:F2}°");
        Assert.True(Math.Abs(position.Elevation - 87.8) < 3.0,
            $"Elevation should be near 87.8°, got {position.Elevation:F2}°");
    }

    #endregion

    #region NOAA Equation of Time Validation

    [Fact]
    public void CalculateEquationOfTime_NOAAReference_2024KeyDates()
    {
        // NOAA equation of time reference values for 2024
        var testCases = new[]
        {
            new { Date = new DateTime(2024, 1, 1), Expected = -3.3, Tolerance = 1.0 },
            new { Date = new DateTime(2024, 2, 11), Expected = -14.2, Tolerance = 1.0 },  // Annual minimum
            new { Date = new DateTime(2024, 4, 15), Expected = -0.1, Tolerance = 1.0 },
            new { Date = new DateTime(2024, 5, 14), Expected = 3.6, Tolerance = 1.0 },    // Local maximum
            new { Date = new DateTime(2024, 6, 14), Expected = -0.4, Tolerance = 1.0 },
            new { Date = new DateTime(2024, 7, 26), Expected = -6.4, Tolerance = 1.0 },   // Local minimum
            new { Date = new DateTime(2024, 9, 1), Expected = 0.1, Tolerance = 1.0 },
            new { Date = new DateTime(2024, 11, 3), Expected = 16.4, Tolerance = 1.0 },   // Annual maximum
            new { Date = new DateTime(2024, 12, 25), Expected = 0.3, Tolerance = 1.0 }
        };

        foreach (var testCase in testCases)
        {
            // Act
            double equationOfTime = _calculator.CalculateEquationOfTime(testCase.Date);

            // Assert
            Assert.True(Math.Abs(equationOfTime - testCase.Expected) <= testCase.Tolerance,
                $"Equation of time on {testCase.Date:yyyy-MM-dd} should be {testCase.Expected:F1} ± {testCase.Tolerance:F1} minutes, got {equationOfTime:F2}");
        }
    }

    [Fact]
    public void CalculateEquationOfTime_NOAAReference_AnnualExtremes2024()
    {
        // Test the annual extremes match NOAA data
        var year = 2024;
        var annualData = _calculator.CalculateAnnualEquationOfTime(year);

        var minValue = annualData.Min(d => d.Minutes);
        var maxValue = annualData.Max(d => d.Minutes);
        var minDate = annualData.First(d => Math.Abs(d.Minutes - minValue) < 0.1).Date;
        var maxDate = annualData.First(d => Math.Abs(d.Minutes - maxValue) < 0.1).Date;

        // Assert annual minimum (around February 11)
        Assert.True(Math.Abs(minValue - (-14.2)) < 1.5,
            $"Annual minimum should be around -14.2 minutes, got {minValue:F2}");
        Assert.True(minDate.Month == 2 && minDate.Day >= 10 && minDate.Day <= 13,
            $"Annual minimum should occur around February 11, occurred on {minDate:MM-dd}");

        // Assert annual maximum (around November 3)
        Assert.True(Math.Abs(maxValue - 16.4) < 1.5,
            $"Annual maximum should be around 16.4 minutes, got {maxValue:F2}");
        Assert.True(maxDate.Month == 11 && maxDate.Day >= 1 && maxDate.Day <= 5,
            $"Annual maximum should occur around November 3, occurred on {maxDate:MM-dd}");
    }

    #endregion

    #region NOAA Sunrise/Sunset Validation

    [Fact]
    public void CalculateDailySunPath_NOAAReference_WashingtonDC_Equinox()
    {
        // NOAA sunrise/sunset data for Washington, DC on Spring Equinox
        // Location: 38.9072°N, 77.0369°W
        // Date: March 20, 2024
        // Expected: Sunrise ≈ 7:12 AM EDT, Sunset ≈ 7:24 PM EDT

        var location = new GeographicCoordinate(38.9072, -77.0369);
        var equinoxDate = new DateTime(2024, 3, 20);

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, equinoxDate);

        // Assert
        Assert.True(sunPath.HasSunrise, "Should have sunrise on equinox");
        Assert.True(sunPath.HasSunset, "Should have sunset on equinox");

        if (sunPath.Sunrise != null && sunPath.Sunset != null)
        {
            var daylightDuration = sunPath.Sunset.Timestamp - sunPath.Sunrise.Timestamp;

            // On equinox, daylight should be close to 12 hours
            Assert.True(Math.Abs(daylightDuration.TotalHours - 12.0) < 0.5,
                $"Daylight duration on equinox should be near 12 hours, got {daylightDuration.TotalHours:F2}");

            // Sunrise should be around 7:12 AM local time (11:12 UTC)
            var expectedSunriseUTC = new DateTime(2024, 3, 20, 11, 12, 0, DateTimeKind.Utc);
            var sunriseError = Math.Abs((sunPath.Sunrise.Timestamp - expectedSunriseUTC).TotalMinutes);
            Assert.True(sunriseError < 30,
                $"Sunrise should be within 30 minutes of expected time, error: {sunriseError:F1} minutes");
        }
    }

    [Fact]
    public void CalculateDailySunPath_NOAAReference_Anchorage_SummerSolstice()
    {
        // NOAA sunrise/sunset data for Anchorage, AK on Summer Solstice
        // Location: 61.2181°N, 149.9003°W
        // Date: June 21, 2024
        // Expected: Very long daylight (≈19+ hours)

        var location = new GeographicCoordinate(61.2181, -149.9003);
        var solsticeDate = new DateTime(2024, 6, 21);

        // Act
        var sunPath = _calculator.CalculateDailySunPath(location, solsticeDate);

        // Assert
        Assert.True(sunPath.HasSunrise, "Should have sunrise in Anchorage on summer solstice");
        Assert.True(sunPath.HasSunset, "Should have sunset in Anchorage on summer solstice");

        if (sunPath.Sunrise != null && sunPath.Sunset != null)
        {
            var daylightDuration = sunPath.Sunset.Timestamp - sunPath.Sunrise.Timestamp;

            // Anchorage should have very long daylight on summer solstice
            Assert.True(daylightDuration.TotalHours > 18.0,
                $"Anchorage should have >18 hours of daylight on summer solstice, got {daylightDuration.TotalHours:F2}");
            Assert.True(daylightDuration.TotalHours < 20.0,
                $"Anchorage daylight should be <20 hours (not midnight sun), got {daylightDuration.TotalHours:F2}");
        }
    }

    #endregion

    #region International Location Validation

    [Fact]
    public void CalculateSolarPosition_NOAAReference_London2024()
    {
        // NOAA reference for London, UK
        // Location: 51.5074°N, 0.1278°W
        // Date: June 21, 2024, 12:00 PM BST (11:00 UTC) - Summer Solstice

        var location = new GeographicCoordinate(51.5074, -0.1278);
        var date = new DateTime(2024, 6, 21, 11, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: At solar noon on summer solstice in London
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 10.0,
            $"Azimuth should be near 180° at solar noon, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation > 60.0 && position.Elevation < 65.0,
            $"Elevation should be 60-65° for London summer solstice, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_Sydney2024()
    {
        // NOAA reference for Sydney, Australia (Southern Hemisphere)
        // Location: 33.8688°S, 151.2093°E
        // Date: December 21, 2024, 12:00 PM AEDT (01:00 UTC) - Summer Solstice

        var location = new GeographicCoordinate(-33.8688, 151.2093);
        var date = new DateTime(2024, 12, 21, 1, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: Summer solstice in Southern Hemisphere
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 10.0,
            $"Azimuth should be near 180° at solar noon, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation > 75.0 && position.Elevation < 85.0,
            $"Elevation should be 75-85° for Sydney summer solstice, got {position.Elevation:F2}°");
    }

    [Fact]
    public void CalculateSolarPosition_NOAAReference_Tokyo2024()
    {
        // NOAA reference for Tokyo, Japan
        // Location: 35.6762°N, 139.6503°E
        // Date: September 22, 2024, 12:00 PM JST (03:00 UTC) - Autumn Equinox

        var location = new GeographicCoordinate(35.6762, 139.6503);
        var date = new DateTime(2024, 9, 22, 3, 0, 0, DateTimeKind.Utc);

        // Act
        var position = _calculator.CalculateSolarPosition(location, date);

        // Assert: Equinox conditions
        Assert.True(Math.Abs(position.Azimuth - 180.0) < 10.0,
            $"Azimuth should be near 180° at solar noon, got {position.Azimuth:F2}°");
        Assert.True(position.Elevation > 50.0 && position.Elevation < 60.0,
            $"Elevation should be 50-60° for Tokyo equinox, got {position.Elevation:F2}°");
    }

    #endregion

    #region Precision Validation Against NOAA

    [Fact]
    public void CalculateJulianDay_NOAAReference_MatchesStandardValues()
    {
        // Test Julian Day calculations against NOAA standard values
        var testCases = new[]
        {
            new { Date = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), Expected = 2451545.0 }, // J2000.0
            new { Date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Expected = 2460310.5 },
            new { Date = new DateTime(2024, 7, 4, 12, 0, 0, DateTimeKind.Utc), Expected = 2460495.0 }
        };

        foreach (var testCase in testCases)
        {
            // Act
            double julianDay = _calculator.CalculateJulianDay(testCase.Date);

            // Assert
            Assert.Equal(testCase.Expected, julianDay, 6);
        }
    }

    [Fact]
    public void CalculateSolarDeclination_NOAAReference_MatchesSeasonalValues()
    {
        // Test solar declination against NOAA values for key dates in 2024
        var testCases = new[]
        {
            new { Date = new DateTime(2024, 3, 20), ExpectedDeclination = 0.0, Tolerance = 1.0 },    // Spring Equinox
            new { Date = new DateTime(2024, 6, 21), ExpectedDeclination = 23.4, Tolerance = 0.5 },  // Summer Solstice
            new { Date = new DateTime(2024, 9, 22), ExpectedDeclination = 0.0, Tolerance = 1.0 },   // Autumn Equinox
            new { Date = new DateTime(2024, 12, 21), ExpectedDeclination = -23.4, Tolerance = 0.5 } // Winter Solstice
        };

        foreach (var testCase in testCases)
        {
            // Act
            double julianDay = _calculator.CalculateJulianDay(testCase.Date);
            double declination = _calculator.CalculateSolarDeclination(julianDay);

            // Assert
            Assert.True(Math.Abs(declination - testCase.ExpectedDeclination) <= testCase.Tolerance,
                $"Solar declination on {testCase.Date:yyyy-MM-dd} should be {testCase.ExpectedDeclination:F1}° ± {testCase.Tolerance:F1}°, got {declination:F2}°");
        }
    }

    #endregion
}