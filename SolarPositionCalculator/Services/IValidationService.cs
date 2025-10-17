using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for input validation services
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates geographic coordinates
    /// </summary>
    /// <param name="latitude">Latitude value</param>
    /// <param name="longitude">Longitude value</param>
    /// <returns>Validation result with error messages if invalid</returns>
    ValidationResult ValidateCoordinates(double latitude, double longitude);

    /// <summary>
    /// Validates a date for astronomical calculations
    /// </summary>
    /// <param name="date">Date to validate</param>
    /// <returns>Validation result with error messages if invalid</returns>
    ValidationResult ValidateDate(DateTime date);

    /// <summary>
    /// Validates coordinate string input and attempts to parse it
    /// </summary>
    /// <param name="input">Coordinate string input</param>
    /// <param name="coordinateType">Type of coordinate (latitude or longitude)</param>
    /// <returns>Validation result with parsed value if valid</returns>
    CoordinateValidationResult ValidateCoordinateInput(string input, CoordinateType coordinateType);

    /// <summary>
    /// Validates if a location is suitable for calculations
    /// </summary>
    /// <param name="location">Geographic coordinate to validate</param>
    /// <returns>Validation result with warnings for edge cases</returns>
    ValidationResult ValidateLocation(GeographicCoordinate location);

    /// <summary>
    /// Gets user-friendly error message for common validation failures
    /// </summary>
    /// <param name="validationResult">Validation result</param>
    /// <returns>User-friendly error message</returns>
    string GetUserFriendlyErrorMessage(ValidationResult validationResult);
}

/// <summary>
/// Represents the type of coordinate being validated
/// </summary>
public enum CoordinateType
{
    Latitude,
    Longitude
}

/// <summary>
/// Result of input validation
/// </summary>
public record ValidationResult(
    bool IsValid,
    string[] ErrorMessages,
    string[]? WarningMessages = null)
{
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates a failed validation result with error messages
    /// </summary>
    public static ValidationResult Error(params string[] errorMessages) => new(false, errorMessages);

    /// <summary>
    /// Creates a successful validation result with warnings
    /// </summary>
    public static ValidationResult Warning(params string[] warningMessages) => new(true, Array.Empty<string>(), warningMessages);

    /// <summary>
    /// Gets the primary error message
    /// </summary>
    public string PrimaryError => ErrorMessages.FirstOrDefault() ?? "";

    /// <summary>
    /// Gets all messages (errors and warnings) combined
    /// </summary>
    public string[] AllMessages => ErrorMessages.Concat(WarningMessages ?? Array.Empty<string>()).ToArray();

    /// <summary>
    /// Indicates if there are any warnings
    /// </summary>
    public bool HasWarnings => WarningMessages?.Length > 0;
}

/// <summary>
/// Result of coordinate validation with parsed value
/// </summary>
public record CoordinateValidationResult(
    bool IsValid,
    string[] ErrorMessages,
    double ParsedValue = 0,
    string[]? WarningMessages = null) : ValidationResult(IsValid, ErrorMessages, WarningMessages)
{
    /// <summary>
    /// Creates a successful coordinate validation result
    /// </summary>
    public static CoordinateValidationResult Success(double value) => new(true, Array.Empty<string>(), value);

    /// <summary>
    /// Creates a failed coordinate validation result
    /// </summary>
    public static new CoordinateValidationResult Error(params string[] errorMessages) => new(false, errorMessages);

    /// <summary>
    /// Creates a successful coordinate validation result with warnings
    /// </summary>
    public static CoordinateValidationResult Warning(double value, params string[] warningMessages) => 
        new(true, Array.Empty<string>(), value, warningMessages);
}