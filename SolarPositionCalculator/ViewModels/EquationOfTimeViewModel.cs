using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// ViewModel for the Equation of Time visualization and interaction
/// </summary>
public partial class EquationOfTimeViewModel : ViewModelBase
{
    private readonly IAstronomicalCalculator _astronomicalCalculator;
    private readonly IVisualizationService _visualizationService;

    [ObservableProperty]
    private InteractivePlotModel? _equationOfTimeChart;

    [ObservableProperty]
    private EquationOfTimeData[]? _annualData;

    [ObservableProperty]
    private int _selectedYear = DateTime.Now.Year;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Now.Date;

    [ObservableProperty]
    private double _selectedEquationOfTime;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isChartInteractive = true;

    [ObservableProperty]
    private string _hoveredValue = "";

    public EquationOfTimeViewModel(IAstronomicalCalculator astronomicalCalculator, IVisualizationService visualizationService)
    {
        _astronomicalCalculator = astronomicalCalculator ?? throw new ArgumentNullException(nameof(astronomicalCalculator));
        _visualizationService = visualizationService ?? throw new ArgumentNullException(nameof(visualizationService));

        // Subscribe to chart interaction events
        _visualizationService.DateSelected += OnDateSelectedFromChart;

        // Load initial data
        LoadAnnualDataAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Command to load equation of time data for the selected year
    /// </summary>
    [RelayCommand]
    private async Task LoadAnnualDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = $"Loading equation of time data for {SelectedYear}...";

            await Task.Run(() =>
            {
                AnnualData = _astronomicalCalculator.CalculateAnnualEquationOfTime(SelectedYear);
                
                if (AnnualData != null && AnnualData.Length > 0)
                {
                    EquationOfTimeChart = _visualizationService.CreateEquationOfTimeChart(AnnualData);
                    UpdateSelectedDateValue();
                }
            });

            StatusMessage = $"Loaded {AnnualData?.Length ?? 0} data points for {SelectedYear}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            AnnualData = null;
            EquationOfTimeChart = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to refresh the chart with current data
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRefreshChart))]
    private void RefreshChart()
    {
        if (AnnualData != null)
        {
            EquationOfTimeChart = _visualizationService.CreateEquationOfTimeChart(AnnualData);
            StatusMessage = "Chart refreshed";
        }
    }

    /// <summary>
    /// Determines if the chart can be refreshed
    /// </summary>
    private bool CanRefreshChart()
    {
        return !IsLoading && AnnualData != null;
    }

    /// <summary>
    /// Command to export the current chart
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportChart))]
    private void ExportChart()
    {
        if (EquationOfTimeChart?.PlotModel != null)
        {
            var success = _visualizationService.ShowExportDialog(
                EquationOfTimeChart.PlotModel, 
                $"equation-of-time-{SelectedYear}");
            
            StatusMessage = success ? "Chart exported successfully" : "Export cancelled";
        }
    }

    /// <summary>
    /// Determines if the chart can be exported
    /// </summary>
    private bool CanExportChart()
    {
        return EquationOfTimeChart?.PlotModel != null && !IsLoading;
    }

    /// <summary>
    /// Command to select a specific date and update the equation of time value
    /// </summary>
    [RelayCommand]
    private void SelectDate(DateTime date)
    {
        SelectedDate = date.Date;
        UpdateSelectedDateValue();
        StatusMessage = $"Selected date: {SelectedDate:MMM dd, yyyy}";
    }

    /// <summary>
    /// Command to go to the previous year
    /// </summary>
    [RelayCommand]
    private async Task PreviousYear()
    {
        SelectedYear--;
        await LoadAnnualDataAsync();
    }

    /// <summary>
    /// Command to go to the next year
    /// </summary>
    [RelayCommand]
    private async Task NextYear()
    {
        SelectedYear++;
        await LoadAnnualDataAsync();
    }

    /// <summary>
    /// Command to go to the current year
    /// </summary>
    [RelayCommand]
    private async Task GoToCurrentYear()
    {
        SelectedYear = DateTime.Now.Year;
        SelectedDate = DateTime.Now.Date;
        await LoadAnnualDataAsync();
    }

    /// <summary>
    /// Handles date selection from chart interaction
    /// </summary>
    private void OnDateSelectedFromChart(object? sender, ChartInteractionEventArgs e)
    {
        if (e.ChartType == "EquationOfTime")
        {
            SelectDate(e.SelectedDate);
        }
    }

    /// <summary>
    /// Updates the equation of time value for the selected date
    /// </summary>
    private void UpdateSelectedDateValue()
    {
        if (AnnualData == null) return;

        // Find the closest data point to the selected date
        var targetDate = new DateTime(SelectedYear, SelectedDate.Month, SelectedDate.Day);
        var closestData = AnnualData
            .OrderBy(d => Math.Abs((d.Date - targetDate).TotalDays))
            .FirstOrDefault();

        if (closestData != null)
        {
            SelectedEquationOfTime = closestData.Minutes;
        }
        else
        {
            // Calculate directly if no data point found
            SelectedEquationOfTime = _astronomicalCalculator.CalculateEquationOfTime(targetDate);
        }
    }

    /// <summary>
    /// Handles property changes
    /// </summary>
    partial void OnSelectedYearChanged(int value)
    {
        if (value != DateTime.Now.Year)
        {
            // Adjust selected date to the new year if it was in the current year
            if (SelectedDate.Year == DateTime.Now.Year)
            {
                SelectedDate = new DateTime(value, SelectedDate.Month, SelectedDate.Day);
            }
        }
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        // Ensure the selected date year matches the selected year
        if (value.Year != SelectedYear)
        {
            SelectedDate = new DateTime(SelectedYear, value.Month, value.Day);
        }
        else
        {
            UpdateSelectedDateValue();
        }
    }

    /// <summary>
    /// Gets the formatted equation of time value for display
    /// </summary>
    public string FormattedEquationOfTime
    {
        get
        {
            var sign = SelectedEquationOfTime >= 0 ? "+" : "";
            return $"{sign}{SelectedEquationOfTime:F2} minutes";
        }
    }

    /// <summary>
    /// Gets the description of what the equation of time represents
    /// </summary>
    public string EquationOfTimeDescription
    {
        get
        {
            if (Math.Abs(SelectedEquationOfTime) < 0.1)
            {
                return "Solar time matches clock time closely";
            }
            else if (SelectedEquationOfTime > 0)
            {
                return "Solar time is ahead of clock time";
            }
            else
            {
                return "Solar time is behind clock time";
            }
        }
    }

    /// <summary>
    /// Gets statistics about the current year's equation of time data
    /// </summary>
    public string DataStatistics
    {
        get
        {
            if (AnnualData == null || AnnualData.Length == 0)
                return "No data available";

            var min = AnnualData.Min(d => d.Minutes);
            var max = AnnualData.Max(d => d.Minutes);
            var range = max - min;

            return $"Range: {min:F1} to {max:F1} minutes (Δ {range:F1} min)";
        }
    }

    /// <summary>
    /// Cleanup when the ViewModel is disposed
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _visualizationService.DateSelected -= OnDateSelectedFromChart;
        }
        base.Dispose(disposing);
    }
}