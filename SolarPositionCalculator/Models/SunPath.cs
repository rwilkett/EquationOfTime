namespace SolarPositionCalculator.Models;

/// <summary>
/// Represents the sun's path across the sky for a specific date and location
/// </summary>
public record SunPath(
    GeographicCoordinate Location,
    DateTime Date,
    SolarPosition[] DailyPositions,
    SolarPosition? Sunrise,
    SolarPosition? Sunset)
{
    /// <summary>
    /// Indicates whether the sun rises on this date at this location
    /// </summary>
    public bool HasSunrise => Sunrise != null;

    /// <summary>
    /// Indicates whether the sun sets on this date at this location
    /// </summary>
    public bool HasSunset => Sunset != null;

    /// <summary>
    /// Indicates if this is a polar day (midnight sun)
    /// </summary>
    public bool IsPolarDay => DailyPositions.All(p => p.IsSunVisible);

    /// <summary>
    /// Indicates if this is a polar night
    /// </summary>
    public bool IsPolarNight => DailyPositions.All(p => !p.IsSunVisible);

    /// <summary>
    /// Gets the maximum elevation angle for the day
    /// </summary>
    public double MaxElevation => DailyPositions.Max(p => p.Elevation);

    /// <summary>
    /// Gets the minimum elevation angle for the day
    /// </summary>
    public double MinElevation => DailyPositions.Min(p => p.Elevation);

    /// <summary>
    /// Indicates if this location requires special polar visualization
    /// </summary>
    public bool RequiresSpecialVisualization => IsPolarDay || IsPolarNight || Math.Abs(Location.Latitude) >= 66.5;

    /// <summary>
    /// Gets a descriptive message for polar conditions
    /// </summary>
    public string GetPolarConditionMessage()
    {
        if (IsPolarDay)
            return $"Midnight Sun: The sun remains above the horizon all day. Max elevation: {MaxElevation:F1}°";
        
        if (IsPolarNight)
        {
            if (MaxElevation > -6)
                return $"Civil Twilight: Continuous twilight conditions. Max elevation: {MaxElevation:F1}°";
            else if (MaxElevation > -12)
                return $"Nautical Twilight: Navigation by stars possible. Max elevation: {MaxElevation:F1}°";
            else if (MaxElevation > -18)
                return $"Astronomical Twilight: Dark sky conditions. Max elevation: {MaxElevation:F1}°";
            else
                return $"Polar Night: Complete darkness. Max elevation: {MaxElevation:F1}°";
        }
        
        return HasSunrise && HasSunset 
            ? $"Normal day/night cycle. Daylight: {(Sunset!.Timestamp - Sunrise!.Timestamp):hh\\:mm}"
            : "Normal conditions";
    }

    /// <summary>
    /// Returns a string representation of the sun path
    /// </summary>
    public override string ToString()
    {
        return $"Sun path for {Date:yyyy-MM-dd} at {Location}";
    }
}