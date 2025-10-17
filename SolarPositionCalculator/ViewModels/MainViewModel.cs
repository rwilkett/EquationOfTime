using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// Main ViewModel for the solar position calculator application
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IAstronomicalCalculator _astronomicalCalculator;
    private readonly ICoordinateConverter _coordinateConverter;
    private readonly IRealTimeService _realTimeService;
    private readonly IValidationService _validationService;
    private readonly IErrorHandlingService _errorHandlingService;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    private double _latitude;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    private double _longitude;

    [ObservableProperty]
    private DateTime _selectedDateTime = DateTime.Now;

    [ObservableProperty]
    private bool _isRealTimeMode;

    [ObservableProperty]
    private SolarPosition? _currentSolarPosition;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isCalculating;

    [ObservableProperty]
    private string _coordinateFormat = "Decimal Degrees";

    [ObservableProperty]
    private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Local;

    [ObservableProperty]
    private DateTime _utcTime;

    [ObservableProperty]
    private string _sunVisibilityStatus = "";

    [ObservableProperty]
    private DateTime _currentLocalTime = DateTime.Now;

    [ObservableProperty]
    private DateTime _currentUtcTime = DateTime.UtcNow;

    [ObservableProperty]
    private string _latitudeError = "";

    [ObservableProperty]
    private string _longitudeError = "";

    [ObservableProperty]
    private string _dateError = "";

    [ObservableProperty]
    private string _validationWarnings = "";

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private string _edgeCaseMessage = "";

    public MainViewModel(IAstronomicalCalculator astronomicalCalculator, ICoordinateConverter coordinateConverter, 
        IRealTimeService realTimeService, IValidationService validationService, IErrorHandlingService errorHandlingService)
    {
        _astronomicalCalculator = astronomicalCalculator ?? throw new ArgumentNullException(nameof(astronomicalCalculator));
        _coordinateConverter = coordinateConverter ?? throw new ArgumentNullException(nameof(coordinateConverter));
        _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
        
        // Subscribe to real-time updates
        _realTimeService.TimeUpdated += OnRealTimeUpdate;
        
        // Set default coordinates (Greenwich, UK)
        Latitude = 51.4769;
        Longitude = -0.0005;
        
        // Initialize current time displays
        CurrentLocalTime = DateTime.Now;
        CurrentUtcTime = DateTime.UtcNow;
        
        UpdateUtcTime();
        CalculateSolarPosition();
    }

    /// <summary>
    /// Command to calculate solar position for current inputs
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCalculateSolarPosition))]
    private async Task CalculateSolarPositionAsync()
    {
        try
        {
            // Validate inputs before calculation
            if (!ValidateInputs())
            {
                StatusMessage = "Please correct the input errors before calculating.";
                return;
            }

            IsCalculating = true;
            StatusMessage = "Calculating solar position...";

            await Task.Run(() =>
            {
                var coordinate = new GeographicCoordinate(Latitude, Longitude);
                
                // Check for edge cases
                CheckForEdgeCases(coordinate, SelectedDateTime);
                
                CurrentSolarPosition = _astronomicalCalculator.CalculateSolarPosition(coordinate, SelectedDateTime);
                
                UpdateSunVisibilityStatus();
                UpdateUtcTime();
            });

            StatusMessage = "Calculation complete";
        }
        catch (Exception ex)
        {
            var errorMessage = _errorHandlingService.HandleCalculationError(ex, "Solar Position Calculation");
            StatusMessage = errorMessage;
            CurrentSolarPosition = null;
            
            // Check if error is recoverable and provide suggestions
            if (_errorHandlingService.IsRecoverableError(ex))
            {
                var suggestions = _errorHandlingService.GetRecoverySuggestions(ErrorType.CalculationFailure);
                ValidationWarnings = $"Suggestions: {string.Join("; ", suggestions.Take(2))}";
            }
        }
        finally
        {
            IsCalculating = false;
        }
    }

    /// <summary>
    /// Synchronous version for property change notifications
    /// </summary>
    private void CalculateSolarPosition()
    {
        if (!CanCalculateSolarPosition()) return;

        try
        {
            var coordinate = new GeographicCoordinate(Latitude, Longitude);
            CurrentSolarPosition = _astronomicalCalculator.CalculateSolarPosition(coordinate, SelectedDateTime);
            
            UpdateSunVisibilityStatus();
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            var errorMessage = _errorHandlingService.HandleCalculationError(ex, "Solar Position Calculation");
            StatusMessage = errorMessage;
            CurrentSolarPosition = null;
        }
    }

    /// <summary>
    /// Determines if solar position calculation can be performed
    /// </summary>
    private bool CanCalculateSolarPosition()
    {
        return !HasValidationErrors && !IsCalculating;
    }

    /// <summary>
    /// Command to set current date and time
    /// </summary>
    [RelayCommand]
    private void SetCurrentDateTime()
    {
        SelectedDateTime = DateTime.Now;
        UpdateUtcTime();
        CalculateSolarPosition();
    }

    /// <summary>
    /// Command to toggle real-time mode
    /// </summary>
    [RelayCommand]
    private void ToggleRealTimeMode()
    {
        IsRealTimeMode = !IsRealTimeMode;
        
        if (IsRealTimeMode)
        {
            _realTimeService.StartRealTimeUpdates();
            StatusMessage = "Real-time mode enabled - updating every minute";
            SetCurrentDateTime();
        }
        else
        {
            _realTimeService.StopRealTimeUpdates();
            StatusMessage = "Real-time mode disabled";
        }
    }

    /// <summary>
    /// Updates the UTC time display
    /// </summary>
    private void UpdateUtcTime()
    {
        UtcTime = SelectedDateTime.ToUniversalTime();
    }

    /// <summary>
    /// Updates the sun visibility status message
    /// </summary>
    private void UpdateSunVisibilityStatus()
    {
        if (CurrentSolarPosition == null)
        {
            SunVisibilityStatus = "";
            return;
        }

        if (CurrentSolarPosition.IsSunVisible)
        {
            SunVisibilityStatus = "Sun is above horizon";
        }
        else
        {
            SunVisibilityStatus = "Sun is below horizon";
        }
    }

    /// <summary>
    /// Handles real-time updates from the real-time service
    /// </summary>
    private void OnRealTimeUpdate(object sender, TimeUpdateEventArgs e)
    {
        if (!IsRealTimeMode) return;

        // Update current time displays
        CurrentLocalTime = e.CurrentTime;
        CurrentUtcTime = e.UtcTime;
        
        // Update selected date time to current time
        SelectedDateTime = e.CurrentTime;
        
        // Recalculate solar position with new time
        CalculateSolarPosition();
        
        StatusMessage = $"Real-time update: {e.CurrentTime:HH:mm:ss}";
    }

    /// <summary>
    /// Validates all inputs and updates error messages
    /// </summary>
    private bool ValidateInputs()
    {
        var hasErrors = false;
        
        // Clear previous errors
        LatitudeError = "";
        LongitudeError = "";
        DateError = "";
        ValidationWarnings = "";
        
        // Validate coordinates
        var coordinateValidation = _validationService.ValidateCoordinates(Latitude, Longitude);
        if (!coordinateValidation.IsValid)
        {
            var latValidation = _validationService.ValidateCoordinateInput(Latitude.ToString(), CoordinateType.Latitude);
            var lonValidation = _validationService.ValidateCoordinateInput(Longitude.ToString(), CoordinateType.Longitude);
            
            if (!latValidation.IsValid)
            {
                LatitudeError = _errorHandlingService.HandleValidationError(latValidation, "Latitude");
                hasErrors = true;
            }
            
            if (!lonValidation.IsValid)
            {
                LongitudeError = _errorHandlingService.HandleValidationError(lonValidation, "Longitude");
                hasErrors = true;
            }
        }
        else if (coordinateValidation.HasWarnings)
        {
            ValidationWarnings = string.Join("; ", coordinateValidation.WarningMessages!);
        }
        
        // Validate date
        var dateValidation = _validationService.ValidateDate(SelectedDateTime);
        if (!dateValidation.IsValid)
        {
            DateError = _errorHandlingService.HandleValidationError(dateValidation, "Date");
            hasErrors = true;
        }
        else if (dateValidation.HasWarnings)
        {
            if (!string.IsNullOrEmpty(ValidationWarnings))
                ValidationWarnings += "; ";
            ValidationWarnings += string.Join("; ", dateValidation.WarningMessages!);
        }
        
        HasValidationErrors = hasErrors;
        return !hasErrors;
    }

    /// <summary>
    /// Checks for edge cases and provides appropriate messaging
    /// </summary>
    private void CheckForEdgeCases(GeographicCoordinate location, DateTime date)
    {
        EdgeCaseMessage = "";
        
        try
        {
            // Check if location is in polar region
            if (_astronomicalCalculator.IsPolarRegion(location))
            {
                var polarCondition = _astronomicalCalculator.GetPolarCondition(location, date);
                if (polarCondition.Type != PolarConditionType.Normal)
                {
                    EdgeCaseMessage = _errorHandlingService.HandleEdgeCase(
                        polarCondition.Type.ToString().ToLower().Replace("_", " "), 
                        location, 
                        date);
                }
            }
            
            // Check for extreme latitudes
            if (Math.Abs(location.Latitude) > 89.9)
            {
                EdgeCaseMessage = _errorHandlingService.HandleEdgeCase("extreme latitude", location, date);
            }
            
            // Check for equatorial regions
            if (Math.Abs(location.Latitude) < 1)
            {
                EdgeCaseMessage = _errorHandlingService.HandleEdgeCase("equatorial region", location, date);
            }
        }
        catch (Exception ex)
        {
            _errorHandlingService.LogError(ex, "Edge Case Detection");
        }
    }

    /// <summary>
    /// Handles property changes to trigger recalculation and validation
    /// </summary>
    partial void OnLatitudeChanged(double value)
    {
        ValidateInputs();
        if (!HasValidationErrors)
        {
            CalculateSolarPosition();
            UpdateTimeZone();
        }
    }

    partial void OnLongitudeChanged(double value)
    {
        ValidateInputs();
        if (!HasValidationErrors)
        {
            CalculateSolarPosition();
            UpdateTimeZone();
        }
    }

    partial void OnSelectedDateTimeChanged(DateTime value)
    {
        ValidateInputs();
        if (!HasValidationErrors)
        {
            UpdateUtcTime();
            CalculateSolarPosition();
        }
    }

    /// <summary>
    /// Updates the time zone based on current coordinates
    /// </summary>
    private void UpdateTimeZone()
    {
        try
        {
            var coordinate = new GeographicCoordinate(Latitude, Longitude);
            if (coordinate.IsValid)
            {
                CurrentTimeZone = _coordinateConverter.DetectTimeZone(coordinate);
            }
        }
        catch
        {
            // Keep current time zone if detection fails
        }
    }

    /// <summary>
    /// Formatted display of current coordinates
    /// </summary>
    public string FormattedCoordinates
    {
        get
        {
            var coordinate = new GeographicCoordinate(Latitude, Longitude);
            return _coordinateConverter.FormatCoordinates(coordinate, Services.CoordinateFormat.DecimalDegrees);
        }
    }

    /// <summary>
    /// Formatted display of solar position
    /// </summary>
    public string FormattedSolarPosition
    {
        get
        {
            if (CurrentSolarPosition == null)
                return "No calculation available";

            return $"Azimuth: {CurrentSolarPosition.Azimuth:F2}°, Elevation: {CurrentSolarPosition.Elevation:F2}°";
        }
    }

    /// <summary>
    /// Indicates if coordinates are valid
    /// </summary>
    public bool AreCoordinatesValid
    {
        get
        {
            var coordinate = new GeographicCoordinate(Latitude, Longitude);
            return coordinate.IsValid;
        }
    }

    /// <summary>
    /// Indicates if real-time service is currently running
    /// </summary>
    public bool IsRealTimeServiceRunning => _realTimeService.IsRunning;

    /// <summary>
    /// Formatted display of current local time
    /// </summary>
    public string FormattedCurrentLocalTime => CurrentLocalTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Formatted display of current UTC time
    /// </summary>
    public string FormattedCurrentUtcTime => CurrentUtcTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

    /// <summary>
    /// Cleanup resources when ViewModel is disposed
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _realTimeService.StopRealTimeUpdates();
            _realTimeService.TimeUpdated -= OnRealTimeUpdate;
        }
        base.Dispose(disposing);
    }
}