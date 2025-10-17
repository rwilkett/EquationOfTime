namespace SolarPositionCalculator.Models;

/// <summary>
/// Represents the type of polar condition at a location
/// </summary>
public enum PolarConditionType
{
    /// <summary>
    /// Normal day/night cycle
    /// </summary>
    Normal,
    
    /// <summary>
    /// Midnight sun - sun never sets
    /// </summary>
    MidnightSun,
    
    /// <summary>
    /// Polar night - sun never rises
    /// </summary>
    PolarNight,
    
    /// <summary>
    /// Civil twilight conditions in polar regions
    /// </summary>
    CivilTwilight,
    
    /// <summary>
    /// Nautical twilight conditions in polar regions
    /// </summary>
    NauticalTwilight,
    
    /// <summary>
    /// Astronomical twilight conditions in polar regions
    /// </summary>
    AstronomicalTwilight
}

/// <summary>
/// Contains information about polar conditions at a specific location and date
/// </summary>
public record PolarCondition(
    PolarConditionType Type,
    string Description,
    bool IsPolarRegion,
    double MaxElevation,
    double MinElevation,
    TimeSpan? DaylightDuration)
{
    /// <summary>
    /// Gets a user-friendly message describing the polar condition
    /// </summary>
    public string GetUserMessage()
    {
        return Type switch
        {
            PolarConditionType.MidnightSun => 
                $"Midnight Sun: The sun remains above the horizon for the entire day. Maximum elevation: {MaxElevation:F1}°",
            
            PolarConditionType.PolarNight => 
                $"Polar Night: The sun remains below the horizon for the entire day. Maximum elevation: {MaxElevation:F1}°",
            
            PolarConditionType.CivilTwilight => 
                $"Civil Twilight: The sun stays between 0° and -6° below the horizon. Continuous twilight conditions.",
            
            PolarConditionType.NauticalTwilight => 
                $"Nautical Twilight: The sun stays between -6° and -12° below the horizon. Navigation by stars possible.",
            
            PolarConditionType.AstronomicalTwilight => 
                $"Astronomical Twilight: The sun stays between -12° and -18° below the horizon. Dark sky conditions for astronomy.",
            
            PolarConditionType.Normal => 
                DaylightDuration.HasValue 
                    ? $"Normal day/night cycle. Daylight duration: {DaylightDuration.Value:hh\\:mm}"
                    : "Normal day/night cycle",
            
            _ => Description
        };
    }

    /// <summary>
    /// Indicates if special polar visualization is needed
    /// </summary>
    public bool RequiresSpecialVisualization => Type != PolarConditionType.Normal;
}