using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of time zone handling services
/// </summary>
public class TimeZoneService : ITimeZoneService
{
    private static readonly Dictionary<string, (double MinLon, double MaxLon, double MinLat, double MaxLat)> TimeZoneRegions = new()
    {
        // Major time zones with approximate geographic boundaries
        { "Pacific Standard Time", (-180, -120, 25, 72) },
        { "Mountain Standard Time", (-120, -105, 25, 72) },
        { "Central Standard Time", (-105, -90, 25, 72) },
        { "Eastern Standard Time", (-90, -60, 25, 72) },
        { "GMT Standard Time", (-15, 15, 35, 70) },
        { "Central European Standard Time", (5, 25, 35, 70) },
        { "Russian Standard Time", (25, 45, 40, 70) },
        { "China Standard Time", (70, 140, 15, 55) },
        { "Tokyo Standard Time", (125, 150, 25, 50) },
        { "AUS Eastern Standard Time", (140, 160, -45, -10) },
        // Add more regions as needed
    };

    /// <summary>
    /// Automatically detects the time zone for given geographic coordinates
    /// </summary>
    public TimeZoneInfo DetectTimeZone(GeographicCoordinate coordinate)
    {
        if (coordinate == null)
            throw new ArgumentNullException(nameof(coordinate));

        if (!coordinate.IsValid)
            throw new ArgumentException("Invalid coordinate values", nameof(coordinate));

        // First try to find a matching region
        foreach (var (timeZoneId, (minLon, maxLon, minLat, maxLat)) in TimeZoneRegions)
        {
            if (coordinate.Longitude >= minLon && coordinate.Longitude <= maxLon &&
                coordinate.Latitude >= minLat && coordinate.Latitude <= maxLat)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Continue to fallback method
                }
            }
        }

        // Fallback: Estimate time zone based on longitude
        return EstimateTimeZoneFromLongitude(coordinate.Longitude);
    }

    /// <summary>
    /// Converts UTC time to local time for the specified coordinate
    /// </summary>
    public DateTime ConvertUtcToLocal(DateTime utcTime, GeographicCoordinate coordinate)
    {
        if (coordinate == null)
            throw new ArgumentNullException(nameof(coordinate));

        var timeZone = DetectTimeZone(coordinate);
        var utcDateTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }

    /// <summary>
    /// Converts local time to UTC for the specified coordinate
    /// </summary>
    public DateTime ConvertLocalToUtc(DateTime localTime, GeographicCoordinate coordinate)
    {
        if (coordinate == null)
            throw new ArgumentNullException(nameof(coordinate));

        var timeZone = DetectTimeZone(coordinate);
        return TimeZoneInfo.ConvertTimeToUtc(localTime, timeZone);
    }

    /// <summary>
    /// Gets all available time zones
    /// </summary>
    public IEnumerable<TimeZoneInfo> GetAvailableTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones().OrderBy(tz => tz.BaseUtcOffset);
    }

    /// <summary>
    /// Checks if daylight saving time is in effect for the given time and location
    /// </summary>
    public bool IsDaylightSavingTime(DateTime dateTime, GeographicCoordinate coordinate)
    {
        if (coordinate == null)
            throw new ArgumentNullException(nameof(coordinate));

        var timeZone = DetectTimeZone(coordinate);
        
        // If the dateTime is unspecified, treat it as local time for the coordinate's time zone
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            return timeZone.IsDaylightSavingTime(dateTime);
        }
        
        // Convert to the coordinate's local time if needed
        var localTime = dateTime.Kind == DateTimeKind.Utc 
            ? ConvertUtcToLocal(dateTime, coordinate)
            : dateTime;
            
        return timeZone.IsDaylightSavingTime(localTime);
    }

    private TimeZoneInfo EstimateTimeZoneFromLongitude(double longitude)
    {
        // Rough estimation: each 15 degrees of longitude represents 1 hour
        var estimatedOffsetHours = Math.Round(longitude / 15.0);
        
        // Clamp to reasonable range (-12 to +14 hours)
        estimatedOffsetHours = Math.Max(-12, Math.Min(14, estimatedOffsetHours));
        
        // Try to find a system time zone that matches this offset
        var targetOffset = TimeSpan.FromHours(estimatedOffsetHours);
        
        var matchingTimeZone = TimeZoneInfo.GetSystemTimeZones()
            .Where(tz => tz.BaseUtcOffset == targetOffset)
            .FirstOrDefault();
            
        // If no exact match, find the closest one
        if (matchingTimeZone == null)
        {
            matchingTimeZone = TimeZoneInfo.GetSystemTimeZones()
                .OrderBy(tz => Math.Abs((tz.BaseUtcOffset - targetOffset).TotalMinutes))
                .First();
        }
        
        return matchingTimeZone;
    }
}