using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for error handling and user feedback services
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Handles calculation errors gracefully
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="context">Context information about where the error occurred</param>
    /// <returns>User-friendly error message</returns>
    string HandleCalculationError(Exception exception, string context);

    /// <summary>
    /// Handles input validation errors
    /// </summary>
    /// <param name="validationResult">Validation result with errors</param>
    /// <param name="fieldName">Name of the field that failed validation</param>
    /// <returns>User-friendly error message</returns>
    string HandleValidationError(ValidationResult validationResult, string fieldName);

    /// <summary>
    /// Handles edge case scenarios (like polar regions)
    /// </summary>
    /// <param name="scenario">Description of the edge case</param>
    /// <param name="location">Geographic location</param>
    /// <param name="date">Date of calculation</param>
    /// <returns>Informative message about the edge case</returns>
    string HandleEdgeCase(string scenario, GeographicCoordinate location, DateTime date);

    /// <summary>
    /// Logs error for debugging purposes
    /// </summary>
    /// <param name="exception">Exception to log</param>
    /// <param name="context">Context information</param>
    void LogError(Exception exception, string context);

    /// <summary>
    /// Gets recovery suggestions for common errors
    /// </summary>
    /// <param name="errorType">Type of error</param>
    /// <returns>Suggested actions to resolve the error</returns>
    string[] GetRecoverySuggestions(ErrorType errorType);

    /// <summary>
    /// Determines if an error is recoverable
    /// </summary>
    /// <param name="exception">Exception to analyze</param>
    /// <returns>True if the error can be recovered from</returns>
    bool IsRecoverableError(Exception exception);
}

/// <summary>
/// Types of errors that can occur in the application
/// </summary>
public enum ErrorType
{
    InvalidCoordinates,
    InvalidDate,
    CalculationFailure,
    PolarRegionEdgeCase,
    NumericPrecisionError,
    FileAccessError,
    NetworkError,
    UnknownError
}

/// <summary>
/// Contains information about an application error
/// </summary>
public record ErrorInfo(
    ErrorType Type,
    string Message,
    string Context,
    Exception? Exception = null,
    string[]? RecoverySuggestions = null)
{
    /// <summary>
    /// Gets a user-friendly description of the error
    /// </summary>
    public string UserFriendlyMessage => GetUserFriendlyMessage();

    /// <summary>
    /// Indicates if this error can be recovered from
    /// </summary>
    public bool IsRecoverable => Type != ErrorType.UnknownError && Exception is not OutOfMemoryException;

    private string GetUserFriendlyMessage()
    {
        return Type switch
        {
            ErrorType.InvalidCoordinates => "The coordinates you entered are not valid. Please check the latitude and longitude values.",
            ErrorType.InvalidDate => "The date you selected is not valid for calculations. Please choose a date between 1900 and 2100.",
            ErrorType.CalculationFailure => "Unable to complete the calculation. This may be due to extreme values or system limitations.",
            ErrorType.PolarRegionEdgeCase => "Special conditions apply to this polar region location. Some calculations may not be available.",
            ErrorType.NumericPrecisionError => "The calculation result may not be accurate due to precision limitations.",
            ErrorType.FileAccessError => "Unable to access the requested file. Please check file permissions and try again.",
            ErrorType.NetworkError => "Network connection error. Please check your internet connection and try again.",
            _ => Message
        };
    }
}