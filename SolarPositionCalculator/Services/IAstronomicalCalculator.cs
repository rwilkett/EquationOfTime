using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for astronomical calculations related to solar positioning
/// </summary>
public interface IAstronomicalCalculator
{
    /// <summary>
    /// Calculates the solar position (azimuth and elevation) for a given location and time
    /// </summary>
    /// <param name="location">Geographic coordinates</param>
    /// <param name="dateTime">Date and time for calculation</param>
    /// <returns>Solar position with azimuth and elevation angles</returns>
    SolarPosition CalculateSolarPosition(GeographicCoordinate location, DateTime dateTime);

    /// <summary>
    /// Calculates the equation of time for a specific date
    /// </summary>
    /// <param name="date">Date for calculation</param>
    /// <returns>Equation of time in minutes</returns>
    double CalculateEquationOfTime(DateTime date);

    /// <summary>
    /// Calculates the complete sun path for a given location and date
    /// </summary>
    /// <param name="location">Geographic coordinates</param>
    /// <param name="date">Date for calculation</param>
    /// <returns>Sun path with daily positions and sunrise/sunset times</returns>
    SunPath CalculateDailySunPath(GeographicCoordinate location, DateTime date);

    /// <summary>
    /// Calculates equation of time data for an entire year
    /// </summary>
    /// <param name="year">Year for calculation</param>
    /// <returns>Array of equation of time data points throughout the year</returns>
    EquationOfTimeData[] CalculateAnnualEquationOfTime(int year);

    /// <summary>
    /// Calculates the Julian day number for a given date and time
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <returns>Julian day number</returns>
    double CalculateJulianDay(DateTime dateTime);

    /// <summary>
    /// Calculates the solar declination angle for a given Julian day
    /// </summary>
    /// <param name="julianDay">Julian day number</param>
    /// <returns>Solar declination in degrees</returns>
    double CalculateSolarDeclination(double julianDay);

    /// <summary>
    /// Determines if a location experiences polar conditions (midnight sun or polar night)
    /// </summary>
    /// <param name="location">Geographic coordinates</param>
    /// <param name="date">Date for calculation</param>
    /// <returns>Polar condition information</returns>
    PolarCondition GetPolarCondition(GeographicCoordinate location, DateTime date);

    /// <summary>
    /// Checks if a location is in a polar region (above Arctic or below Antarctic Circle)
    /// </summary>
    /// <param name="location">Geographic coordinates</param>
    /// <returns>True if location is in polar region</returns>
    bool IsPolarRegion(GeographicCoordinate location);
}