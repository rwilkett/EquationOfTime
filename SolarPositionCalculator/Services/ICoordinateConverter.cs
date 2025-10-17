using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Defines coordinate format types for parsing and formatting
/// </summary>
public enum CoordinateFormat
{
    DecimalDegrees,
    DegreesMinutesSeconds
}

/// <summary>
/// Service for converting and formatting geographic coordinates
/// </summary>
public interface ICoordinateConverter
{
    /// <summary>
    /// Parses coordinate string in the specified format
    /// </summary>
    /// <param name="input">Input string containing coordinates</param>
    /// <param name="format">Expected coordinate format</param>
    /// <returns>Parsed geographic coordinate</returns>
    GeographicCoordinate ParseCoordinates(string input, CoordinateFormat format);

    /// <summary>
    /// Formats coordinates for display in the specified format
    /// </summary>
    /// <param name="coordinate">Geographic coordinate to format</param>
    /// <param name="format">Desired output format</param>
    /// <returns>Formatted coordinate string</returns>
    string FormatCoordinates(GeographicCoordinate coordinate, CoordinateFormat format);

    /// <summary>
    /// Converts UTC time to specified time zone
    /// </summary>
    /// <param name="utc">UTC DateTime</param>
    /// <param name="timeZone">Target time zone</param>
    /// <returns>Converted DateTime in target time zone</returns>
    DateTime ConvertToTimeZone(DateTime utc, TimeZoneInfo timeZone);

    /// <summary>
    /// Detects the appropriate time zone for given coordinates
    /// </summary>
    /// <param name="coordinate">Geographic coordinate</param>
    /// <returns>TimeZoneInfo for the location</returns>
    TimeZoneInfo DetectTimeZone(GeographicCoordinate coordinate);
}