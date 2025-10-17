namespace SolarPositionCalculator.Models;

/// <summary>
/// Represents a geographic coordinate with latitude and longitude
/// </summary>
public record GeographicCoordinate(double Latitude, double Longitude)
{
    /// <summary>
    /// Validates if the coordinate values are within valid ranges
    /// </summary>
    public bool IsValid => Latitude >= -90 && Latitude <= 90 && 
                          Longitude >= -180 && Longitude <= 180;

    /// <summary>
    /// Returns a string representation of the coordinate in decimal degrees
    /// </summary>
    public override string ToString()
    {
        return $"Lat: {Latitude:F6}°, Lon: {Longitude:F6}°";
    }
}