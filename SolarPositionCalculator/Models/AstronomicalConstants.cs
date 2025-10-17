namespace SolarPositionCalculator.Models;

/// <summary>
/// Contains astronomical constants used in solar position calculations
/// </summary>
public static class AstronomicalConstants
{
    /// <summary>
    /// Earth's obliquity (axial tilt) in degrees
    /// </summary>
    public const double EarthObliquity = 23.4397;

    /// <summary>
    /// Earth's orbital eccentricity factor
    /// </summary>
    public const double EccentricityFactor = 0.0167;

    /// <summary>
    /// Solar constant in W/m²
    /// </summary>
    public const double SolarConstant = 1361;

    /// <summary>
    /// Degrees to radians conversion factor
    /// </summary>
    public const double DegreesToRadians = Math.PI / 180.0;

    /// <summary>
    /// Radians to degrees conversion factor
    /// </summary>
    public const double RadiansToDegrees = 180.0 / Math.PI;

    /// <summary>
    /// Julian day number for January 1, 2000, 12:00 UTC (J2000.0 epoch)
    /// </summary>
    public const double J2000 = 2451545.0;

    /// <summary>
    /// Number of days in a Julian year
    /// </summary>
    public const double DaysPerJulianYear = 365.25;

    /// <summary>
    /// Atmospheric refraction correction at horizon in degrees
    /// </summary>
    public const double AtmosphericRefraction = 0.833;
}