using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Service for handling time zone operations and automatic detection
/// </summary>
public interface ITimeZoneService
{
    /// <summary>
    /// Automatically detects the time zone for given geographic coordinates
    /// </summary>
    /// <param name="coordinate">Geographic coordinate</param>
    /// <returns>TimeZoneInfo for the location</returns>
    TimeZoneInfo DetectTimeZone(GeographicCoordinate coordinate);

    /// <summary>
    /// Converts UTC time to local time for the specified coordinate
    /// </summary>
    /// <param name="utcTime">UTC DateTime</param>
    /// <param name="coordinate">Geographic coordinate</param>
    /// <returns>Local DateTime for the coordinate location</returns>
    DateTime ConvertUtcToLocal(DateTime utcTime, GeographicCoordinate coordinate);

    /// <summary>
    /// Converts local time to UTC for the specified coordinate
    /// </summary>
    /// <param name="localTime">Local DateTime</param>
    /// <param name="coordinate">Geographic coordinate</param>
    /// <returns>UTC DateTime</returns>
    DateTime ConvertLocalToUtc(DateTime localTime, GeographicCoordinate coordinate);

    /// <summary>
    /// Gets all available time zones
    /// </summary>
    /// <returns>Collection of available time zones</returns>
    IEnumerable<TimeZoneInfo> GetAvailableTimeZones();

    /// <summary>
    /// Checks if daylight saving time is in effect for the given time and location
    /// </summary>
    /// <param name="dateTime">DateTime to check</param>
    /// <param name="coordinate">Geographic coordinate</param>
    /// <returns>True if DST is in effect</returns>
    bool IsDaylightSavingTime(DateTime dateTime, GeographicCoordinate coordinate);
}