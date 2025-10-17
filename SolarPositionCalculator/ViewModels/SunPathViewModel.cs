using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// ViewModel for the Sun Path visualization and interaction
/// </summary>
public partial class SunPathViewModel : ViewModelBase
{
    private readonly IAstronomicalCalculator _astronomicalCalculator;
    private readonly IVisualizationService _visualizationService;

    [ObservableProperty]
    private InteractivePlotModel? _sunPathChart;

    [ObservableProperty]
    private SunPath? _currentSunPath;

    [ObservableProperty]
    private GeographicCoordinate _location = new(51.4769, -0.0005); // Default: Greenwich

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Now.Date;

    [ObservableProperty]
    private SolarPosition? _currentPosition;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _showSeasonalPaths;

    [ObservableProperty]
    private bool _highlightCurrentPosition = true;

    [ObservableProperty]
    private string _sunPathDescription = "";

    [ObservableProperty]
    private TimeSpan _dayLength;

    [ObservableProperty]
    private string _sunriseTime = "";

    [ObservableProperty]
    private string _sunsetTime = "";

    [ObservableProperty]
    private double _maxElevation;

    [ObservableProperty]
    private string _specialConditions = "";

    [ObservableProperty]
    private PolarCondition? _polarCondition;

    [ObservableProperty]
    private bool _isPolarRegion;

    [ObservableProperty]
    private string _polarConditionMessage = "";

    public SunPathViewModel(IAstronomicalCalculator astronomicalCalculator, IVisualizationService visualizationService)
    {
        _astronomicalCalculator = astronomicalCalculator ?? throw new ArgumentNullException(nameof(astronomicalCalculator));
        _visualizationService = visualizationService ?? throw new ArgumentNullException(nameof(visualizationService));

        // Subscribe to chart interaction events
        _visualizationService.PositionSelected += OnPositionSelectedFromChart;

        // Calculate initial sun path
        CalculateSunPathAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Command to calculate and display the sun path for the current date and location
    /// </summary>
    [RelayCommand]
    private async Task CalculateSunPathAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Calculating sun path...";

            await Task.Run(() =>
            {
                // Calculate the sun path for the selected date
                CurrentSunPath = _astronomicalCalculator.CalculateDailySunPath(Location, SelectedDate);
                
                // Check for polar conditions
                PolarCondition = _astronomicalCalculator.GetPolarCondition(Location, SelectedDate);
                IsPolarRegion = _astronomicalCalculator.IsPolarRegion(Location);
                PolarConditionMessage = PolarCondition?.GetUserMessage() ?? "";
                
                // Calculate current position if highlighting is enabled
                if (HighlightCurrentPosition)
                {
                    CurrentPosition = _astronomicalCalculator.CalculateSolarPosition(Location, DateTime.Now);
                }

                // Create the appropriate visualization based on polar conditions
                if (CurrentSunPath != null)
                {
                    if (PolarCondition != null && PolarCondition.RequiresSpecialVisualization)
                    {
                        SunPathChart = _visualizationService.CreatePolarSunPathDiagram(CurrentSunPath, PolarCondition, CurrentPosition);
                    }
                    else
                    {
                        SunPathChart = _visualizationService.CreateSunPathDiagram(CurrentSunPath, CurrentPosition);
                    }
                    UpdateSunPathInfo();
                }
            });

            StatusMessage = "Sun path calculated successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error calculating sun path: {ex.Message}";
            CurrentSunPath = null;
            SunPathChart = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to refresh the current position highlight
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefreshCurrentPosition))]
    private async Task RefreshCurrentPositionAsync()
    {
        if (CurrentSunPath != null)
        {
            try
            {
                CurrentPosition = _astronomicalCalculator.CalculateSolarPosition(Location, DateTime.Now);
                
                // Use appropriate visualization based on polar conditions
                if (PolarCondition != null && PolarCondition.RequiresSpecialVisualization)
                {
                    SunPathChart = _visualizationService.CreatePolarSunPathDiagram(CurrentSunPath, PolarCondition, CurrentPosition);
                }
                else
                {
                    SunPathChart = _visualizationService.CreateSunPathDiagram(CurrentSunPath, CurrentPosition);
                }
                StatusMessage = "Current position updated";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating current position: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Determines if current position can be refreshed
    /// </summary>
    private bool CanRefreshCurrentPosition()
    {
        return !IsLoading && CurrentSunPath != null && HighlightCurrentPosition;
    }

    /// <summary>
    /// Command to show seasonal sun path variations
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowSeasonalPaths))]
    private async Task ToggleSeasonalPathsAsync()
    {
        ShowSeasonalPaths = !ShowSeasonalPaths;
        
        if (ShowSeasonalPaths)
        {
            await CalculateSeasonalPathsAsync();
        }
        else
        {
            await CalculateSunPathAsync();
        }
    }

    /// <summary>
    /// Determines if seasonal paths can be shown
    /// </summary>
    private bool CanShowSeasonalPaths()
    {
        return !IsLoading;
    }

    /// <summary>
    /// Command to export the current sun path chart
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportChart))]
    private void ExportChart()
    {
        if (SunPathChart?.PlotModel != null)
        {
            var success = _visualizationService.ShowExportDialog(
                SunPathChart.PlotModel, 
                $"sun-path-{Location.Latitude:F2}N-{Location.Longitude:F2}E-{SelectedDate:yyyy-MM-dd}");
            
            StatusMessage = success ? "Chart exported successfully" : "Export cancelled";
        }
    }

    /// <summary>
    /// Determines if the chart can be exported
    /// </summary>
    private bool CanExportChart()
    {
        return SunPathChart?.PlotModel != null && !IsLoading;
    }

    /// <summary>
    /// Command to go to solstice dates
    /// </summary>
    [RelayCommand]
    private async Task GoToSummerSolstice()
    {
        SelectedDate = new DateTime(SelectedDate.Year, 6, 21); // Approximate summer solstice
        await CalculateSunPathAsync();
    }

    [RelayCommand]
    private async Task GoToWinterSolstice()
    {
        SelectedDate = new DateTime(SelectedDate.Year, 12, 21); // Approximate winter solstice
        await CalculateSunPathAsync();
    }

    [RelayCommand]
    private async Task GoToEquinox()
    {
        SelectedDate = new DateTime(SelectedDate.Year, 3, 20); // Approximate spring equinox
        await CalculateSunPathAsync();
    }

    /// <summary>
    /// Command to go to today's date
    /// </summary>
    [RelayCommand]
    private async Task GoToToday()
    {
        SelectedDate = DateTime.Now.Date;
        await CalculateSunPathAsync();
    }

    /// <summary>
    /// Calculates seasonal sun paths for comparison
    /// </summary>
    private async Task CalculateSeasonalPathsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Calculating seasonal sun paths...";

            await Task.Run(() =>
            {
                // Calculate sun paths for key dates
                var year = SelectedDate.Year;
                var summerSolstice = _astronomicalCalculator.CalculateDailySunPath(Location, new DateTime(year, 6, 21));
                var winterSolstice = _astronomicalCalculator.CalculateDailySunPath(Location, new DateTime(year, 12, 21));
                var equinox = _astronomicalCalculator.CalculateDailySunPath(Location, new DateTime(year, 3, 20));

                // Use the current date's sun path as the primary one
                CurrentSunPath = _astronomicalCalculator.CalculateDailySunPath(Location, SelectedDate);

                // Create visualization with seasonal paths
                // Note: This would require extending the visualization service to handle multiple paths
                if (CurrentSunPath != null)
                {
                    SunPathChart = _visualizationService.CreateSunPathDiagram(CurrentSunPath, CurrentPosition);
                    UpdateSunPathInfo();
                }
            });

            StatusMessage = "Seasonal sun paths calculated";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error calculating seasonal paths: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handles position selection from chart interaction
    /// </summary>
    private void OnPositionSelectedFromChart(object? sender, SunPathInteractionEventArgs e)
    {
        StatusMessage = $"Selected position: Az {e.Azimuth:F1}°, El {e.Elevation:F1}°";
        
        if (e.TimeOfDay.HasValue)
        {
            StatusMessage += $" at {e.TimeOfDay.Value:HH:mm}";
        }
    }

    /// <summary>
    /// Updates sun path information and statistics
    /// </summary>
    private void UpdateSunPathInfo()
    {
        if (CurrentSunPath == null) return;

        // Update basic information
        SunPathDescription = GetSunPathDescription();
        SpecialConditions = GetSpecialConditions();

        // Calculate day length and sunrise/sunset times
        if (CurrentSunPath.HasSunrise && CurrentSunPath.HasSunset)
        {
            var sunrise = CurrentSunPath.Sunrise!.Timestamp;
            var sunset = CurrentSunPath.Sunset!.Timestamp;
            
            DayLength = sunset - sunrise;
            SunriseTime = sunrise.ToString("HH:mm");
            SunsetTime = sunset.ToString("HH:mm");
        }
        else
        {
            DayLength = TimeSpan.Zero;
            SunriseTime = "N/A";
            SunsetTime = "N/A";
        }

        // Calculate maximum elevation
        MaxElevation = CurrentSunPath.DailyPositions.Max(p => p.Elevation);
    }

    /// <summary>
    /// Gets a description of the current sun path
    /// </summary>
    private string GetSunPathDescription()
    {
        if (CurrentSunPath == null) return "";

        var visiblePositions = CurrentSunPath.DailyPositions.Count(p => p.IsSunVisible);
        var totalPositions = CurrentSunPath.DailyPositions.Length;
        var visibilityPercent = (double)visiblePositions / totalPositions * 100;

        return $"Sun visible for {visibilityPercent:F1}% of the day ({visiblePositions}/{totalPositions} positions)";
    }

    /// <summary>
    /// Gets special conditions text for polar regions or unusual circumstances
    /// </summary>
    private string GetSpecialConditions()
    {
        if (CurrentSunPath == null) return "";

        // Use polar condition information if available
        if (PolarCondition != null && PolarCondition.Type != PolarConditionType.Normal)
        {
            return PolarCondition.GetUserMessage();
        }

        if (!CurrentSunPath.HasSunrise && !CurrentSunPath.HasSunset)
        {
            return "No sunrise or sunset - Check location and date";
        }
        else if (IsPolarRegion)
        {
            return "Arctic/Antarctic region - Sun paths may show extreme seasonal variation";
        }

        return "";
    }

    /// <summary>
    /// Handles property changes
    /// </summary>
    partial void OnLocationChanged(GeographicCoordinate value)
    {
        CalculateSunPathAsync().ConfigureAwait(false);
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        CalculateSunPathAsync().ConfigureAwait(false);
    }

    partial void OnHighlightCurrentPositionChanged(bool value)
    {
        if (value)
        {
            RefreshCurrentPositionAsync().ConfigureAwait(false);
        }
        else
        {
            CurrentPosition = null;
            if (CurrentSunPath != null)
            {
                SunPathChart = _visualizationService.CreateSunPathDiagram(CurrentSunPath, null);
            }
        }
    }

    /// <summary>
    /// Gets formatted day length for display
    /// </summary>
    public string FormattedDayLength
    {
        get
        {
            if (DayLength == TimeSpan.Zero)
                return "N/A";

            return $"{DayLength.Hours:D2}h {DayLength.Minutes:D2}m";
        }
    }

    /// <summary>
    /// Gets formatted maximum elevation for display
    /// </summary>
    public string FormattedMaxElevation
    {
        get
        {
            return $"{MaxElevation:F1}°";
        }
    }

    /// <summary>
    /// Gets the current sun visibility status
    /// </summary>
    public string SunVisibilityStatus
    {
        get
        {
            if (CurrentPosition == null)
                return "Current position not calculated";

            return CurrentPosition.IsSunVisible ? "Sun is currently visible" : "Sun is currently below horizon";
        }
    }

    /// <summary>
    /// Cleanup when the ViewModel is disposed
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _visualizationService.PositionSelected -= OnPositionSelectedFromChart;
        }
        base.Dispose(disposing);
    }
}