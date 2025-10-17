using System.Globalization;
using System.Text.RegularExpressions;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of coordinate conversion and formatting services
/// </summary>
public class CoordinateConverter : ICoordinateConverter
{
    private readonly ITimeZoneService _timeZoneService;

    public CoordinateConverter(ITimeZoneService timeZoneService)
    {
        _timeZoneService = timeZoneService ?? throw new ArgumentNullException(nameof(timeZoneService));
    }

    private static readonly Regex DecimalDegreesRegex = new(
        @"^(?<lat>-?\d+(?:\.\d+)?)\s*,?\s*(?<lon>-?\d+(?:\.\d+)?)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DmsRegex = new(
        @"^(?<latDeg>\d+)[°\s]+(?<latMin>\d+)['\s]+(?<latSec>\d+(?:\.\d+)?)[""'\s]*(?<latDir>[NS])\s*,?\s*(?<lonDeg>\d+)[°\s]+(?<lonMin>\d+)['\s]+(?<lonSec>\d+(?:\.\d+)?)[""'\s]*(?<lonDir>[EW])$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses coordinate string in the specified format
    /// </summary>
    public GeographicCoordinate ParseCoordinates(string input, CoordinateFormat format)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        return format switch
        {
            CoordinateFormat.DecimalDegrees => ParseDecimalDegrees(input),
            CoordinateFormat.DegreesMinutesSeconds => ParseDegreesMinutesSeconds(input),
            _ => throw new ArgumentException($"Unsupported coordinate format: {format}")
        };
    }

    /// <summary>
    /// Formats coordinates for display in the specified format
    /// </summary>
    public string FormatCoordinates(GeographicCoordinate coordinate, CoordinateFormat format)
    {
        if (coordinate == null)
            throw new ArgumentNullException(nameof(coordinate));

        if (!coordinate.IsValid)
            throw new ArgumentException("Invalid coordinate values", nameof(coordinate));

        return format switch
        {
            CoordinateFormat.DecimalDegrees => FormatDecimalDegrees(coordinate),
            CoordinateFormat.DegreesMinutesSeconds => FormatDegreesMinutesSeconds(coordinate),
            _ => throw new ArgumentException($"Unsupported coordinate format: {format}")
        };
    }

    /// <summary>
    /// Converts UTC time to specified time zone
    /// </summary>
    public DateTime ConvertToTimeZone(DateTime utc, TimeZoneInfo timeZone)
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));

        // Ensure the input DateTime is treated as UTC
        var utcDateTime = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
    }

    /// <summary>
    /// Detects the appropriate time zone for given coordinates
    /// </summary>
    public TimeZoneInfo DetectTimeZone(GeographicCoordinate coordinate)
    {
        return _timeZoneService.DetectTimeZone(coordinate);
    }

    private GeographicCoordinate ParseDecimalDegrees(string input)
    {
        var match = DecimalDegreesRegex.Match(input.Trim());
        if (!match.Success)
            throw new FormatException("Invalid decimal degrees format. Expected: 'latitude, longitude' (e.g., '40.7128, -74.0060')");

        if (!double.TryParse(match.Groups["lat"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
            throw new FormatException("Invalid latitude value");

        if (!double.TryParse(match.Groups["lon"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            throw new FormatException("Invalid longitude value");

        var coordinate = new GeographicCoordinate(latitude, longitude);
        if (!coordinate.IsValid)
            throw new ArgumentOutOfRangeException(nameof(input), 
                "Coordinates out of valid range. Latitude: -90 to +90, Longitude: -180 to +180");

        return coordinate;
    }

    private GeographicCoordinate ParseDegreesMinutesSeconds(string input)
    {
        var match = DmsRegex.Match(input.Trim());
        if (!match.Success)
            throw new FormatException("Invalid DMS format. Expected: 'DD° MM' SS\"N/S, DD° MM' SS\"E/W' (e.g., '40° 42' 46\"N, 74° 0' 21\"W')");

        // Parse latitude components
        if (!int.TryParse(match.Groups["latDeg"].Value, out var latDeg) ||
            !int.TryParse(match.Groups["latMin"].Value, out var latMin) ||
            !double.TryParse(match.Groups["latSec"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latSec))
            throw new FormatException("Invalid latitude DMS values");

        // Parse longitude components
        if (!int.TryParse(match.Groups["lonDeg"].Value, out var lonDeg) ||
            !int.TryParse(match.Groups["lonMin"].Value, out var lonMin) ||
            !double.TryParse(match.Groups["lonSec"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var lonSec))
            throw new FormatException("Invalid longitude DMS values");

        // Validate DMS component ranges
        if (latMin >= 60 || latSec >= 60 || lonMin >= 60 || lonSec >= 60)
            throw new ArgumentOutOfRangeException(nameof(input), "Minutes and seconds must be less than 60");

        // Convert to decimal degrees
        var latitude = latDeg + (latMin / 60.0) + (latSec / 3600.0);
        var longitude = lonDeg + (lonMin / 60.0) + (lonSec / 3600.0);

        // Apply direction (N/S for latitude, E/W for longitude)
        var latDir = match.Groups["latDir"].Value.ToUpperInvariant();
        var lonDir = match.Groups["lonDir"].Value.ToUpperInvariant();

        if (latDir == "S") latitude = -latitude;
        if (lonDir == "W") longitude = -longitude;

        var coordinate = new GeographicCoordinate(latitude, longitude);
        if (!coordinate.IsValid)
            throw new ArgumentOutOfRangeException(nameof(input), 
                "Coordinates out of valid range. Latitude: -90 to +90, Longitude: -180 to +180");

        return coordinate;
    }

    private string FormatDecimalDegrees(GeographicCoordinate coordinate)
    {
        return $"{coordinate.Latitude:F6}, {coordinate.Longitude:F6}";
    }

    private string FormatDegreesMinutesSeconds(GeographicCoordinate coordinate)
    {
        var (latDeg, latMin, latSec, latDir) = ConvertToDegreesMinutesSeconds(coordinate.Latitude, true);
        var (lonDeg, lonMin, lonSec, lonDir) = ConvertToDegreesMinutesSeconds(coordinate.Longitude, false);

        return $"{latDeg:D2}° {latMin:D2}' {latSec:F2}\"{latDir}, {lonDeg:D3}° {lonMin:D2}' {lonSec:F2}\"{lonDir}";
    }

    private (int degrees, int minutes, double seconds, string direction) ConvertToDegreesMinutesSeconds(double decimalDegrees, bool isLatitude)
    {
        var isNegative = decimalDegrees < 0;
        var absoluteValue = Math.Abs(decimalDegrees);

        var degrees = (int)absoluteValue;
        var minutesDecimal = (absoluteValue - degrees) * 60;
        var minutes = (int)minutesDecimal;
        var seconds = (minutesDecimal - minutes) * 60;

        string direction;
        if (isLatitude)
            direction = isNegative ? "S" : "N";
        else
            direction = isNegative ? "W" : "E";

        return (degrees, minutes, seconds, direction);
    }
}