using SolarPositionCalculator.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of input validation services
/// </summary>
public class ValidationService : IValidationService
{
    private static readonly DateTime MinValidDate = new(1900, 1, 1);
    private static readonly DateTime MaxValidDate = new(2100, 12, 31);

    /// <summary>
    /// Validates geographic coordinates
    /// </summary>
    public ValidationResult ValidateCoordinates(double latitude, double longitude)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate latitude range
        if (latitude < -90 || latitude > 90)
        {
            errors.Add($"Latitude must be between -90° and +90°. Current value: {latitude:F6}°");
        }

        // Validate longitude range
        if (longitude < -180 || longitude > 180)
        {
            errors.Add($"Longitude must be between -180° and +180°. Current value: {longitude:F6}°");
        }

        // Check for extreme latitudes that might cause calculation issues
        if (Math.Abs(latitude) > 89.9)
        {
            warnings.Add("Extremely high latitude may cause calculation precision issues near the poles.");
        }

        // Check for polar regions
        if (Math.Abs(latitude) >= 66.5)
        {
            warnings.Add("Location is in polar region. Expect midnight sun or polar night conditions during certain periods.");
        }

        // Check for equatorial regions
        if (Math.Abs(latitude) < 1)
        {
            warnings.Add("Location is near the equator. Sun will pass nearly overhead during certain times of year.");
        }

        if (errors.Count > 0)
        {
            return new ValidationResult(false, errors.ToArray(), warnings.ToArray());
        }

        return warnings.Count > 0 
            ? ValidationResult.Warning(warnings.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates a date for astronomical calculations
    /// </summary>
    public ValidationResult ValidateDate(DateTime date)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Check date range for calculation accuracy
        if (date < MinValidDate)
        {
            errors.Add($"Date is too far in the past. Minimum supported date: {MinValidDate:yyyy-MM-dd}");
        }
        else if (date > MaxValidDate)
        {
            errors.Add($"Date is too far in the future. Maximum supported date: {MaxValidDate:yyyy-MM-dd}");
        }

        // Warn about dates far from current time
        var daysDifference = Math.Abs((date - DateTime.Now).TotalDays);
        if (daysDifference > 36525) // ~100 years
        {
            warnings.Add("Date is more than 100 years from present. Calculation accuracy may be reduced.");
        }

        // Check for leap year edge cases
        if (date.Month == 2 && date.Day == 29 && !DateTime.IsLeapYear(date.Year))
        {
            errors.Add($"February 29 does not exist in {date.Year} (not a leap year).");
        }

        if (errors.Count > 0)
        {
            return new ValidationResult(false, errors.ToArray(), warnings.ToArray());
        }

        return warnings.Count > 0 
            ? ValidationResult.Warning(warnings.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates coordinate string input and attempts to parse it
    /// </summary>
    public CoordinateValidationResult ValidateCoordinateInput(string input, CoordinateType coordinateType)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return CoordinateValidationResult.Error($"{coordinateType} cannot be empty.");
        }

        input = input.Trim();

        // Try to parse as decimal degrees first
        if (TryParseDecimalDegrees(input, out double decimalValue))
        {
            var coordinateValidation = coordinateType == CoordinateType.Latitude
                ? ValidateLatitude(decimalValue)
                : ValidateLongitude(decimalValue);

            if (coordinateValidation.IsValid)
            {
                return coordinateValidation.HasWarnings
                    ? CoordinateValidationResult.Warning(decimalValue, coordinateValidation.WarningMessages!)
                    : CoordinateValidationResult.Success(decimalValue);
            }
            else
            {
                return CoordinateValidationResult.Error(coordinateValidation.ErrorMessages);
            }
        }

        // Try to parse as degrees, minutes, seconds (DMS)
        if (TryParseDMS(input, out double dmsValue))
        {
            var coordinateValidation = coordinateType == CoordinateType.Latitude
                ? ValidateLatitude(dmsValue)
                : ValidateLongitude(dmsValue);

            if (coordinateValidation.IsValid)
            {
                return coordinateValidation.HasWarnings
                    ? CoordinateValidationResult.Warning(dmsValue, coordinateValidation.WarningMessages!)
                    : CoordinateValidationResult.Success(dmsValue);
            }
            else
            {
                return CoordinateValidationResult.Error(coordinateValidation.ErrorMessages);
            }
        }

        // If neither format worked, provide helpful error message
        var expectedFormat = coordinateType == CoordinateType.Latitude
            ? "Expected format: decimal degrees (-90 to +90) or DMS (e.g., 51°28'38\"N)"
            : "Expected format: decimal degrees (-180 to +180) or DMS (e.g., 0°0'5\"W)";

        return CoordinateValidationResult.Error(
            $"Unable to parse {coordinateType.ToString().ToLower()} value: '{input}'",
            expectedFormat);
    }

    /// <summary>
    /// Validates if a location is suitable for calculations
    /// </summary>
    public ValidationResult ValidateLocation(GeographicCoordinate location)
    {
        if (!location.IsValid)
        {
            return ValidationResult.Error("Invalid geographic coordinates provided.");
        }

        return ValidateCoordinates(location.Latitude, location.Longitude);
    }

    /// <summary>
    /// Gets user-friendly error message for common validation failures
    /// </summary>
    public string GetUserFriendlyErrorMessage(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return validationResult.HasWarnings 
                ? $"Warning: {string.Join(" ", validationResult.WarningMessages!)}"
                : "Input is valid.";
        }

        var primaryError = validationResult.PrimaryError;

        // Provide more user-friendly versions of common errors
        if (primaryError.Contains("Latitude must be between"))
        {
            return "Latitude must be between -90° (South Pole) and +90° (North Pole). Please check your input.";
        }

        if (primaryError.Contains("Longitude must be between"))
        {
            return "Longitude must be between -180° (International Date Line West) and +180° (International Date Line East). Please check your input.";
        }

        if (primaryError.Contains("Date is too far"))
        {
            return "The selected date is outside the supported range for accurate calculations. Please choose a date between 1900 and 2100.";
        }

        if (primaryError.Contains("Unable to parse"))
        {
            return "Unable to understand the coordinate format. Please use decimal degrees (e.g., 51.4769) or degrees/minutes/seconds (e.g., 51°28'38\"N).";
        }

        // Return the original error message if no specific user-friendly version exists
        return primaryError;
    }

    /// <summary>
    /// Validates latitude value
    /// </summary>
    private ValidationResult ValidateLatitude(double latitude)
    {
        var warnings = new List<string>();

        if (latitude < -90 || latitude > 90)
        {
            return ValidationResult.Error($"Latitude must be between -90° and +90°. Current value: {latitude:F6}°");
        }

        if (Math.Abs(latitude) > 89.9)
        {
            warnings.Add("Extremely high latitude may cause calculation precision issues near the poles.");
        }

        if (Math.Abs(latitude) >= 66.5)
        {
            warnings.Add("Location is in polar region. Expect midnight sun or polar night conditions.");
        }

        return warnings.Count > 0 ? ValidationResult.Warning(warnings.ToArray()) : ValidationResult.Success();
    }

    /// <summary>
    /// Validates longitude value
    /// </summary>
    private ValidationResult ValidateLongitude(double longitude)
    {
        if (longitude < -180 || longitude > 180)
        {
            return ValidationResult.Error($"Longitude must be between -180° and +180°. Current value: {longitude:F6}°");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Attempts to parse decimal degrees format
    /// </summary>
    private bool TryParseDecimalDegrees(string input, out double value)
    {
        // Remove degree symbol if present
        input = input.Replace("°", "").Trim();

        return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Attempts to parse degrees, minutes, seconds (DMS) format
    /// </summary>
    private bool TryParseDMS(string input, out double value)
    {
        value = 0;

        // Regex pattern for DMS format: 51°28'38"N or 51d28m38sN or 51 28 38 N
        var dmsPattern = @"^(\d+)[°d]\s*(\d+)[''m]\s*(\d+(?:\.\d+)?)[""s]?\s*([NSEW]?)$";
        var match = Regex.Match(input.ToUpper(), dmsPattern, RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            // Try simpler pattern: 51 28 38 N
            var simplePattern = @"^(\d+)\s+(\d+)\s+(\d+(?:\.\d+)?)\s*([NSEW]?)$";
            match = Regex.Match(input.ToUpper(), simplePattern, RegexOptions.IgnoreCase);
        }

        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int degrees) &&
                int.TryParse(match.Groups[2].Value, out int minutes) &&
                double.TryParse(match.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
            {
                // Validate ranges
                if (minutes >= 60 || seconds >= 60)
                {
                    return false;
                }

                value = degrees + minutes / 60.0 + seconds / 3600.0;

                // Apply hemisphere
                var hemisphere = match.Groups[4].Value;
                if (hemisphere == "S" || hemisphere == "W")
                {
                    value = -value;
                }

                return true;
            }
        }

        return false;
    }
}