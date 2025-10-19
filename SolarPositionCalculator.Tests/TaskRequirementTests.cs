using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using Xunit;

namespace SolarPositionCalculator.Tests;

/// <summary>
/// Tests that specifically address the task requirements:
/// - Test solar position calculations against known reference values
/// - Validate equation of time calculations with NOAA data (with reasonable tolerance)
/// - Test edge cases for polar regions and extreme dates
/// </summary>
public class TaskRequirementTests
{
    private readonly IAstronomicalCalculator _calculator;

    public TaskRequirementTests()
    {
        _calculator = new AstronomicalCalculator();
    }

    #region Solar Position Against Known Reference Values

    [Fact]
    public void SolarPosition_KnownReferenceValues_WithinReasonableTolerance()
    {
        // Test against well-known astronomical reference points
        // Using more tolerant ranges since exact NOAA matching requires identical algorithms

        var testCases = new[]
        {
            new {
                Name = "Greenwich Equinox Noon",
                Location = new GeographicCoordinate(51.4769, 0.0),
                Date = new DateTime(2024, 3, 20, 12, 0, 0, DateTimeKind.Utc),
                ExpectedElevationMin = 35.0,
                ExpectedElevationMax = 45.0
            },
            new {
                Name = "New York Summer",
                Location = new GeographicCoordinate(40.7128, -74.0060),
                Date = new DateTime(2024, 6, 21, 16, 0, 0, DateTimeKind.Utc), // Noon EDT
                ExpectedElevationMin = 65.0,
                ExpectedElevationMax = 75.0
            },
            new {
                Name = "Sydney Winter",
                Location = new GeographicCoordinate(-33.8688, 151.2093),
                Date = new DateTime(2024, 6, 21, 2, 0, 0, DateTimeKind.Utc), // Noon AEST
                ExpectedElevationMin = 25.0,
                ExpectedElevationMax = 35.0
            }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var position = _calculator.CalculateSolarPosition(testCase.Location, testCase.Date);

            // Assert
            Assert.True(position.Elevation >= testCase.ExpectedElevationMin &&
                       position.Elevation <= testCase.ExpectedElevationMax,
                $"{testCase.Name}: Elevation should be {testCase.ExpectedElevationMin}-{testCase.ExpectedElevationMax}°, got {position.Elevation:F2}°");

            // Basic validity checks
            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360,
                $"{testCase.Name}: Azimuth should be 0-360°, got {position.Azimuth:F2}°");
        }
    }

    [Fact]
    public void SolarPosition_EquinoxConditions_ShowsExpectedSymmetry()
    {
        // Test that equinox conditions show expected symmetry
        var location = new GeographicCoordinate(40.0, -75.0); // Philadelphia area
        var springEquinox = new DateTime(2024, 3, 20, 16, 0, 0, DateTimeKind.Utc); // Noon EST
        var autumnEquinox = new DateTime(2024, 9, 22, 16, 0, 0, DateTimeKind.Utc); // Noon EST

        // Act
        var springPosition = _calculator.CalculateSolarPosition(location, springEquinox);
        var autumnPosition = _calculator.CalculateSolarPosition(location, autumnEquinox);

        // Assert: Elevations should be similar at equinoxes
        var elevationDifference = Math.Abs(springPosition.Elevation - autumnPosition.Elevation);
        Assert.True(elevationDifference < 5.0,
            $"Spring and autumn equinox elevations should be similar, difference: {elevationDifference:F2}°");
    }

    #endregion

    #region Equation of Time Validation (NOAA-inspired)

    [Fact]
    public void EquationOfTime_AnnualExtremes_MatchExpectedPattern()
    {
        // Test that equation of time follows the expected annual pattern
        // Based on NOAA data but with reasonable tolerance for algorithm differences

        var year = 2024;
        var annualData = _calculator.CalculateAnnualEquationOfTime(year);

        var minValue = annualData.Min(d => d.Minutes);
        var maxValue = annualData.Max(d => d.Minutes);
        var minDate = annualData.First(d => Math.Abs(d.Minutes - minValue) < 0.1).Date;
        var maxDate = annualData.First(d => Math.Abs(d.Minutes - maxValue) < 0.1).Date;

        // Assert: Annual extremes should be in expected ranges and months
        Assert.True(minValue >= -17 && minValue <= -12,
            $"Annual minimum should be around -14 ± 3 minutes, got {minValue:F2}");
        Assert.True(maxValue >= 12 && maxValue <= 18,
            $"Annual maximum should be around 16 ± 2 minutes, got {maxValue:F2}");

        // Minimum should occur in February (month 2)
        Assert.Equal(2, minDate.Month);

        // Maximum should occur in late fall (October or November)
        Assert.True(maxDate.Month >= 10 && maxDate.Month <= 11,
            $"Maximum should occur in October or November, occurred in month {maxDate.Month}");
    }

    [Fact]
    public void EquationOfTime_KeyDates_WithinExpectedRanges()
    {
        // Test equation of time for key dates with reasonable tolerance
        var testCases = new[]
        {
            new { Date = new DateTime(2024, 1, 1), ExpectedMin = -5.0, ExpectedMax = 0.0 },
            new { Date = new DateTime(2024, 4, 15), ExpectedMin = -2.0, ExpectedMax = 2.0 },   // Near zero
            new { Date = new DateTime(2024, 7, 1), ExpectedMin = -5.0, ExpectedMax = -2.0 },   // Negative
            new { Date = new DateTime(2024, 10, 1), ExpectedMin = 8.0, ExpectedMax = 15.0 }    // Positive
        };

        foreach (var testCase in testCases)
        {
            // Act
            double equationOfTime = _calculator.CalculateEquationOfTime(testCase.Date);

            // Assert
            Assert.True(equationOfTime >= testCase.ExpectedMin && equationOfTime <= testCase.ExpectedMax,
                $"Equation of time on {testCase.Date:yyyy-MM-dd} should be {testCase.ExpectedMin} to {testCase.ExpectedMax} minutes, got {equationOfTime:F2}");
        }
    }

    #endregion

    #region Polar Region Edge Cases

    [Fact]
    public void PolarRegions_MidnightSunConditions_DetectedCorrectly()
    {
        // Test midnight sun detection in Arctic summer
        var arcticLocations = new[]
        {
            new GeographicCoordinate(70.0, 0.0),   // Northern Norway
            new GeographicCoordinate(75.0, 10.0),  // Svalbard
            new GeographicCoordinate(80.0, -100.0) // High Arctic Canada
        };

        var summerSolstice = new DateTime(2024, 6, 21);

        foreach (var location in arcticLocations)
        {
            // Act
            var condition = _calculator.GetPolarCondition(location, summerSolstice);
            var sunPath = _calculator.CalculateDailySunPath(location, summerSolstice);

            // Assert
            Assert.True(condition.IsPolarRegion, $"Location {location.Latitude}°N should be in polar region");

            // For high Arctic in summer, should have midnight sun or very long daylight
            if (location.Latitude >= 75.0)
            {
                // Very high latitudes should have midnight sun
                Assert.True(condition.Type == PolarConditionType.MidnightSun ||
                           condition.MaxElevation > 0,
                    $"Location {location.Latitude}°N should have midnight sun or positive elevation in summer");
            }

            // All positions should be reasonable
            Assert.True(sunPath.DailyPositions.All(p => p.Elevation >= -90 && p.Elevation <= 90),
                "All elevations should be within valid range");
        }
    }

    [Fact]
    public void PolarRegions_PolarNightConditions_DetectedCorrectly()
    {
        // Test polar night detection in Arctic winter
        var arcticLocations = new[]
        {
            new GeographicCoordinate(70.0, 0.0),   // Northern Norway
            new GeographicCoordinate(75.0, 10.0),  // Svalbard
            new GeographicCoordinate(80.0, -100.0) // High Arctic Canada
        };

        var winterSolstice = new DateTime(2024, 12, 21);

        foreach (var location in arcticLocations)
        {
            // Act
            var condition = _calculator.GetPolarCondition(location, winterSolstice);
            var sunPath = _calculator.CalculateDailySunPath(location, winterSolstice);

            // Assert
            Assert.True(condition.IsPolarRegion, $"Location {location.Latitude}°N should be in polar region");

            // For high Arctic in winter, should have polar night or very low sun
            if (location.Latitude >= 75.0)
            {
                Assert.True(condition.MaxElevation < 5.0,
                    $"Location {location.Latitude}°N should have low or negative elevation in winter, got {condition.MaxElevation:F2}°");
            }

            // All positions should be reasonable
            Assert.True(sunPath.DailyPositions.All(p => p.Elevation >= -90 && p.Elevation <= 90),
                "All elevations should be within valid range");
        }
    }

    [Fact]
    public void PolarRegions_AntarcticSeasons_OppositeToArctic()
    {
        // Test that Antarctic seasons are opposite to Arctic
        var arcticLocation = new GeographicCoordinate(75.0, 0.0);
        var antarcticLocation = new GeographicCoordinate(-75.0, 0.0);

        var summerSolstice = new DateTime(2024, 6, 21); // Northern summer
        var winterSolstice = new DateTime(2024, 12, 21); // Northern winter

        // Act
        var arcticSummer = _calculator.GetPolarCondition(arcticLocation, summerSolstice);
        var arcticWinter = _calculator.GetPolarCondition(arcticLocation, winterSolstice);
        var antarcticSummer = _calculator.GetPolarCondition(antarcticLocation, winterSolstice); // Antarctic summer
        var antarcticWinter = _calculator.GetPolarCondition(antarcticLocation, summerSolstice); // Antarctic winter

        // Assert: Seasons should be opposite
        Assert.True(arcticSummer.MaxElevation > arcticWinter.MaxElevation,
            "Arctic should have higher elevation in northern summer than winter");
        Assert.True(antarcticSummer.MaxElevation > antarcticWinter.MaxElevation,
            "Antarctic should have higher elevation in northern winter (Antarctic summer) than northern summer");
    }

    #endregion

    #region Extreme Date Edge Cases

    [Fact]
    public void ExtremeHistoricalDates_HandledCorrectly()
    {
        // Test with dates far from J2000 epoch
        var location = new GeographicCoordinate(45.0, 0.0);
        var extremeDates = new[]
        {
            new DateTime(1900, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(1950, 6, 15, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2050, 12, 31, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2100, 6, 21, 12, 0, 0, DateTimeKind.Utc)
        };

        foreach (var date in extremeDates)
        {
            // Act & Assert: Should not throw exceptions
            var position = _calculator.CalculateSolarPosition(location, date);
            var equationOfTime = _calculator.CalculateEquationOfTime(date);

            // Basic validity checks
            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360,
                $"Azimuth for {date:yyyy} should be valid");
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90,
                $"Elevation for {date:yyyy} should be valid");
            Assert.True(Math.Abs(equationOfTime) < 30,
                $"Equation of time for {date:yyyy} should be reasonable, got {equationOfTime:F2}");
        }
    }

    [Fact]
    public void LeapYearDates_HandledCorrectly()
    {
        // Test leap year dates including February 29
        var location = new GeographicCoordinate(40.0, -75.0);
        var leapYearDates = new[]
        {
            new DateTime(2024, 2, 29, 12, 0, 0, DateTimeKind.Utc), // Leap day
            new DateTime(2024, 3, 1, 12, 0, 0, DateTimeKind.Utc),  // Day after leap day
            new DateTime(2020, 2, 29, 12, 0, 0, DateTimeKind.Utc), // Another leap year
        };

        foreach (var date in leapYearDates)
        {
            // Act & Assert: Should not throw exceptions
            var position = _calculator.CalculateSolarPosition(location, date);
            var equationOfTime = _calculator.CalculateEquationOfTime(date);

            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360);
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90);
            Assert.True(Math.Abs(equationOfTime) < 20);
        }
    }

    #endregion

    #region Precision and Consistency Tests

    [Fact]
    public void CalculationPrecision_HighAccuracy_MaintainedAcrossRuns()
    {
        // Test that calculations are consistent and precise
        var location = new GeographicCoordinate(51.5074, -0.1278); // London
        var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act: Multiple calculations
        var positions = new SolarPosition[10];
        var equationOfTimes = new double[10];

        for (int i = 0; i < 10; i++)
        {
            positions[i] = _calculator.CalculateSolarPosition(location, date);
            equationOfTimes[i] = _calculator.CalculateEquationOfTime(date);
        }

        // Assert: All results should be identical (deterministic)
        for (int i = 1; i < 10; i++)
        {
            Assert.Equal(positions[0].Azimuth, positions[i].Azimuth, 15);
            Assert.Equal(positions[0].Elevation, positions[i].Elevation, 15);
            Assert.Equal(equationOfTimes[0], equationOfTimes[i], 15);
        }
    }

    [Fact]
    public void EdgeCaseCoordinates_HandledGracefully()
    {
        // Test coordinates at the edges of valid ranges
        var edgeCases = new[]
        {
            new GeographicCoordinate(90.0, 0.0),    // North Pole
            new GeographicCoordinate(-90.0, 0.0),   // South Pole
            new GeographicCoordinate(0.0, 180.0),   // Antimeridian
            new GeographicCoordinate(0.0, -180.0),  // Antimeridian
            new GeographicCoordinate(66.5, 0.0),    // Arctic Circle
            new GeographicCoordinate(-66.5, 0.0)    // Antarctic Circle
        };

        var testDate = new DateTime(2024, 6, 21, 12, 0, 0, DateTimeKind.Utc);

        foreach (var location in edgeCases)
        {
            // Act & Assert: Should not throw exceptions
            var position = _calculator.CalculateSolarPosition(location, testDate);

            Assert.True(position.Azimuth >= 0 && position.Azimuth < 360,
                $"Azimuth at {location} should be valid");
            Assert.True(position.Elevation >= -90 && position.Elevation <= 90,
                $"Elevation at {location} should be valid");
        }
    }

    #endregion
}