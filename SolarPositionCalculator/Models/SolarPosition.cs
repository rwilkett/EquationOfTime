namespace SolarPositionCalculator.Models;

/// <summary>
/// Represents the position of the sun in the sky at a specific time and location
/// </summary>
public record SolarPosition(
    double Azimuth,      // Degrees from North (0-360)
    double Elevation,    // Degrees above horizon (-90 to +90)
    DateTime Timestamp,
    GeographicCoordinate Location)
{
    /// <summary>
    /// Indicates whether the sun is visible above the horizon
    /// </summary>
    public bool IsSunVisible => Elevation > 0;

    /// <summary>
    /// Returns a string representation of the solar position
    /// </summary>
    public override string ToString()
    {
        return $"Az: {Azimuth:F2}°, El: {Elevation:F2}° at {Timestamp:yyyy-MM-dd HH:mm:ss}";
    }
}