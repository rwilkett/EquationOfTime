using SolarPositionCalculator.Models;
using System.Diagnostics;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of error handling and user feedback services
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    /// <summary>
    /// Handles calculation errors gracefully
    /// </summary>
    public string HandleCalculationError(Exception exception, string context)
    {
        LogError(exception, context);

        return exception switch
        {
            ArgumentException argEx when argEx.Message.Contains("coordinate") =>
                "Invalid coordinates provided. Please check that latitude is between -90° and +90°, and longitude is between -180° and +180°.",

            ArgumentOutOfRangeException rangeEx =>
                "One or more input values are outside the valid range. Please check your coordinates and date selection.",

            OverflowException =>
                "Calculation resulted in values too large to process. This may occur with extreme coordinates or dates.",

            DivideByZeroException =>
                "Mathematical error in calculation. This may occur at extreme latitudes near the poles.",

            InvalidOperationException invOpEx when invOpEx.Message.Contains("polar") =>
                "Special polar region conditions detected. Some calculations may not be available during midnight sun or polar night periods.",

            NotSupportedException =>
                "This calculation is not supported for the current location or date combination.",

            _ => "An unexpected error occurred during calculation. Please try again with different values."
        };
    }

    /// <summary>
    /// Handles input validation errors
    /// </summary>
    public string HandleValidationError(ValidationResult validationResult, string fieldName)
    {
        if (validationResult.IsValid)
        {
            return validationResult.HasWarnings 
                ? $"{fieldName}: {string.Join(" ", validationResult.WarningMessages!)}"
                : "";
        }

        var primaryError = validationResult.PrimaryError;

        // Provide context-specific error messages
        return fieldName.ToLower() switch
        {
            "latitude" => GetLatitudeErrorMessage(primaryError),
            "longitude" => GetLongitudeErrorMessage(primaryError),
            "date" => GetDateErrorMessage(primaryError),
            _ => $"{fieldName}: {primaryError}"
        };
    }

    /// <summary>
    /// Handles edge case scenarios (like polar regions)
    /// </summary>
    public string HandleEdgeCase(string scenario, GeographicCoordinate location, DateTime date)
    {
        return scenario.ToLower() switch
        {
            "midnight sun" => 
                $"Midnight Sun condition at {location.Latitude:F2}°N on {date:MMM dd}. " +
                "The sun remains above the horizon for the entire day. Sunrise and sunset times are not applicable.",

            "polar night" => 
                $"Polar Night condition at {location.Latitude:F2}° on {date:MMM dd}. " +
                "The sun remains below the horizon for the entire day. Consider checking civil, nautical, or astronomical twilight times.",

            "civil twilight" => 
                $"Civil Twilight conditions at {location.Latitude:F2}° on {date:MMM dd}. " +
                "The sun stays between 0° and -6° below the horizon. Outdoor activities are possible without artificial lighting.",

            "nautical twilight" => 
                $"Nautical Twilight conditions at {location.Latitude:F2}° on {date:MMM dd}. " +
                "The sun stays between -6° and -12° below the horizon. Navigation by stars is possible.",

            "astronomical twilight" => 
                $"Astronomical Twilight conditions at {location.Latitude:F2}° on {date:MMM dd}. " +
                "The sun stays between -12° and -18° below the horizon. Ideal conditions for astronomical observations.",

            "extreme latitude" => 
                $"Extreme latitude location ({location.Latitude:F2}°). " +
                "Calculations near the poles may have reduced precision. Results should be interpreted carefully.",

            "equatorial region" => 
                $"Equatorial region location ({location.Latitude:F2}°). " +
                "The sun will pass nearly overhead during certain times of the year, resulting in very short shadows.",

            _ => $"Special condition detected: {scenario} at {location} on {date:yyyy-MM-dd}"
        };
    }

    /// <summary>
    /// Logs error for debugging purposes
    /// </summary>
    public void LogError(Exception exception, string context)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in {context}: {exception.Message}";
        
        if (exception.InnerException != null)
        {
            logMessage += $" | Inner: {exception.InnerException.Message}";
        }

        // Log to debug output
        Debug.WriteLine(logMessage);
        
        // In a production application, you might also log to a file or logging service
        // For now, we'll just use Debug output which is visible in the IDE
        
        // Also log the stack trace for debugging
        Debug.WriteLine($"Stack trace: {exception.StackTrace}");
    }

    /// <summary>
    /// Gets recovery suggestions for common errors
    /// </summary>
    public string[] GetRecoverySuggestions(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.InvalidCoordinates => new[]
            {
                "Check that latitude is between -90° and +90°",
                "Check that longitude is between -180° and +180°",
                "Use decimal degrees format (e.g., 51.4769) or DMS format (e.g., 51°28'38\"N)",
                "Verify coordinates using a map or GPS device"
            },

            ErrorType.InvalidDate => new[]
            {
                "Select a date between 1900 and 2100 for best accuracy",
                "Check that the date exists (e.g., February 29 in leap years only)",
                "Use current date if unsure about historical accuracy"
            },

            ErrorType.CalculationFailure => new[]
            {
                "Try a different date if the current one is causing issues",
                "Check coordinates for extreme values near the poles",
                "Restart the application if the problem persists",
                "Contact support if the error continues"
            },

            ErrorType.PolarRegionEdgeCase => new[]
            {
                "This is normal behavior in polar regions during certain seasons",
                "Try different dates to see seasonal variations",
                "Consider using twilight calculations instead of sunrise/sunset",
                "Check the polar condition information for more details"
            },

            ErrorType.NumericPrecisionError => new[]
            {
                "Results may be less accurate at extreme latitudes",
                "Consider rounding results to appropriate precision",
                "Use alternative calculation methods if available",
                "Verify results with other astronomical tools"
            },

            ErrorType.FileAccessError => new[]
            {
                "Check that the file exists and is accessible",
                "Verify file permissions",
                "Close other applications that might be using the file",
                "Try saving to a different location"
            },

            ErrorType.NetworkError => new[]
            {
                "Check your internet connection",
                "Try again in a few moments",
                "Use offline features if available",
                "Contact your network administrator if the problem persists"
            },

            _ => new[]
            {
                "Try the operation again",
                "Restart the application",
                "Check your input values",
                "Contact support if the problem continues"
            }
        };
    }

    /// <summary>
    /// Determines if an error is recoverable
    /// </summary>
    public bool IsRecoverableError(Exception exception)
    {
        return exception switch
        {
            OutOfMemoryException => false,
            StackOverflowException => false,
            AccessViolationException => false,
            ArgumentOutOfRangeException => true, // More specific first
            ArgumentException => true,
            InvalidOperationException => true,
            NotSupportedException => true,
            OverflowException => true,
            DivideByZeroException => true,
            FormatException => true,
            _ => true // Most other exceptions are potentially recoverable
        };
    }

    /// <summary>
    /// Gets latitude-specific error message
    /// </summary>
    private string GetLatitudeErrorMessage(string error)
    {
        if (error.Contains("must be between"))
        {
            return "Latitude must be between -90° (South Pole) and +90° (North Pole). " +
                   "Use positive values for North, negative for South.";
        }

        if (error.Contains("Unable to parse"))
        {
            return "Unable to understand the latitude format. " +
                   "Examples: 51.4769 or 51°28'38\"N or -23.5505";
        }

        return $"Latitude error: {error}";
    }

    /// <summary>
    /// Gets longitude-specific error message
    /// </summary>
    private string GetLongitudeErrorMessage(string error)
    {
        if (error.Contains("must be between"))
        {
            return "Longitude must be between -180° and +180°. " +
                   "Use positive values for East, negative for West.";
        }

        if (error.Contains("Unable to parse"))
        {
            return "Unable to understand the longitude format. " +
                   "Examples: -0.0005 or 0°0'5\"W or 139.6917";
        }

        return $"Longitude error: {error}";
    }

    /// <summary>
    /// Gets date-specific error message
    /// </summary>
    private string GetDateErrorMessage(string error)
    {
        if (error.Contains("too far"))
        {
            return "Please select a date between 1900 and 2100 for accurate calculations. " +
                   "Dates outside this range may produce unreliable results.";
        }

        if (error.Contains("February 29"))
        {
            return "February 29 only exists in leap years. Please select a valid date.";
        }

        return $"Date error: {error}";
    }
}