using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using SolarPositionCalculator.Views;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// ViewModel for the CSV export dialog
/// </summary>
public partial class CsvExportDialogViewModel : ObservableObject
{
    private readonly ICsvExportService _csvExportService;

    [ObservableProperty]
    private GeographicCoordinate _location = new(40.7128, -74.0060); // Default to NYC

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private TimeSpan _interval = TimeSpan.FromHours(1);

    [ObservableProperty]
    private string _outputFilePath = string.Empty;

    [ObservableProperty]
    private bool _includeHeaders = true;

    [ObservableProperty]
    private bool _includeLocationInRows = true;

    [ObservableProperty]
    private bool _includeEquationOfTime = false;

    [ObservableProperty]
    private bool _includeSunVisibility = true;

    [ObservableProperty]
    private bool _includeMetadataComments = true;

    [ObservableProperty]
    private string _delimiter = ",";

    [ObservableProperty]
    private string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    [ObservableProperty]
    private string _numberFormat = "F6";

    [ObservableProperty]
    private bool _isExporting = false;

    [ObservableProperty]
    private double _exportProgress = 0;

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    [ObservableProperty]
    private int _estimatedDataPoints = 0;

    [ObservableProperty]
    private TimeSpan? _estimatedTimeRemaining;

    public ObservableCollection<IntervalOption> AvailableIntervals { get; }
    public ObservableCollection<string> AvailableDelimiters { get; }
    public ObservableCollection<string> AvailableDateTimeFormats { get; }

    public bool CanExport => !IsExporting && 
                            Location.IsValid && 
                            StartDate < EndDate && 
                            Interval > TimeSpan.Zero &&
                            !string.IsNullOrWhiteSpace(OutputFilePath);

    public CsvExportDialogViewModel(ICsvExportService csvExportService)
    {
        _csvExportService = csvExportService ?? throw new ArgumentNullException(nameof(csvExportService));

        AvailableIntervals = new ObservableCollection<IntervalOption>
        {
            new("1 minute", TimeSpan.FromMinutes(1)),
            new("5 minutes", TimeSpan.FromMinutes(5)),
            new("15 minutes", TimeSpan.FromMinutes(15)),
            new("30 minutes", TimeSpan.FromMinutes(30)),
            new("1 hour", TimeSpan.FromHours(1)),
            new("2 hours", TimeSpan.FromHours(2)),
            new("6 hours", TimeSpan.FromHours(6)),
            new("12 hours", TimeSpan.FromHours(12)),
            new("1 day", TimeSpan.FromDays(1)),
            new("1 week", TimeSpan.FromDays(7)),
            new("1 month", TimeSpan.FromDays(30))
        };

        AvailableDelimiters = new ObservableCollection<string> { ",", ";", "\t", "|" };
        
        AvailableDateTimeFormats = new ObservableCollection<string>
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy"
        };

        // Update estimated data points when relevant properties change
        PropertyChanged += OnPropertyChanged;
        UpdateEstimatedDataPoints();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        try
        {
            IsExporting = true;
            ExportProgress = 0;
            ExportStatus = "Starting export...";

            var options = new CsvExportOptions
            {
                IncludeHeaders = IncludeHeaders,
                Delimiter = Delimiter,
                DateTimeFormat = DateTimeFormat,
                NumberFormat = NumberFormat,
                IncludeLocationInRows = IncludeLocationInRows,
                IncludeEquationOfTime = IncludeEquationOfTime,
                IncludeSunVisibility = IncludeSunVisibility,
                IncludeMetadataComments = IncludeMetadataComments
            };

            // Show progress dialog for large exports (more than 1000 data points)
            if (EstimatedDataPoints > 1000)
            {
                await ExportWithProgressDialog(options);
            }
            else
            {
                var progress = new Progress<ExportProgress>(OnExportProgress);
                await _csvExportService.ExportDateRangeAsync(
                    Location,
                    StartDate,
                    EndDate,
                    Interval,
                    OutputFilePath,
                    options,
                    progress);

                ExportStatus = "Export completed successfully!";
            }
        }
        catch (Exception ex)
        {
            ExportStatus = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private async Task ExportWithProgressDialog(CsvExportOptions options)
    {
        var progressViewModel = new ExportProgressDialogViewModel
        {
            Title = "Exporting Solar Position Data"
        };

        var progressDialog = new Views.ExportProgressDialog(progressViewModel);
        
        // Start the export task
        var exportTask = Task.Run(async () =>
        {
            var progress = new Progress<ExportProgress>(progressViewModel.UpdateProgress);
            
            try
            {
                await _csvExportService.ExportDateRangeAsync(
                    Location,
                    StartDate,
                    EndDate,
                    Interval,
                    OutputFilePath,
                    options,
                    progress);
                
                progressViewModel.CompleteSuccessfully();
            }
            catch (Exception ex)
            {
                progressViewModel.CompleteFailed(ex.Message);
            }
        });

        // Show the progress dialog
        var result = progressDialog.ShowDialog();
        
        // Wait for the export to complete
        await exportTask;
        
        if (progressViewModel.WasSuccessful)
        {
            ExportStatus = "Export completed successfully!";
        }
        else
        {
            ExportStatus = "Export was cancelled or failed";
        }
    }

    [RelayCommand]
    private void BrowseOutputFile()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save CSV Export",
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = GenerateDefaultFileName()
        };

        if (dialog.ShowDialog() == true)
        {
            OutputFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void SetCurrentLocation(GeographicCoordinate location)
    {
        Location = location;
    }

    [RelayCommand]
    private void SetDateRangeToToday()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddDays(1);
    }

    [RelayCommand]
    private void SetDateRangeToWeek()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddDays(7);
    }

    [RelayCommand]
    private void SetDateRangeToMonth()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddMonths(1);
    }

    [RelayCommand]
    private void SetDateRangeToYear()
    {
        StartDate = new DateTime(DateTime.Today.Year, 1, 1);
        EndDate = new DateTime(DateTime.Today.Year, 12, 31);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StartDate) or nameof(EndDate) or nameof(Interval))
        {
            UpdateEstimatedDataPoints();
        }

        // Update CanExecute for commands
        ExportCommand.NotifyCanExecuteChanged();
    }

    private void UpdateEstimatedDataPoints()
    {
        if (StartDate < EndDate && Interval > TimeSpan.Zero)
        {
            EstimatedDataPoints = (int)Math.Ceiling((EndDate - StartDate).TotalMilliseconds / Interval.TotalMilliseconds);
        }
        else
        {
            EstimatedDataPoints = 0;
        }
    }

    private void OnExportProgress(ExportProgress progress)
    {
        ExportProgress = progress.PercentComplete;
        ExportStatus = $"{progress.CurrentOperation} ({progress.ProcessedItems}/{progress.TotalItems})";
        EstimatedTimeRemaining = progress.EstimatedTimeRemaining;
    }

    private string GenerateDefaultFileName()
    {
        var locationStr = $"lat{Location.Latitude:F2}_lon{Location.Longitude:F2}".Replace(".", "_").Replace("-", "neg");
        var dateStr = StartDate.ToString("yyyyMMdd");
        var intervalStr = FormatIntervalForFileName(Interval);
        
        return $"solar_positions_{locationStr}_{dateStr}_{intervalStr}.csv";
    }

    private static string FormatIntervalForFileName(TimeSpan interval)
    {
        if (interval.TotalDays >= 1)
            return $"{interval.TotalDays:F0}d";
        if (interval.TotalHours >= 1)
            return $"{interval.TotalHours:F0}h";
        if (interval.TotalMinutes >= 1)
            return $"{interval.TotalMinutes:F0}m";
        return $"{interval.TotalSeconds:F0}s";
    }
}

/// <summary>
/// Represents an interval option for the dropdown
/// </summary>
public record IntervalOption(string DisplayName, TimeSpan Value)
{
    public override string ToString() => DisplayName;
}