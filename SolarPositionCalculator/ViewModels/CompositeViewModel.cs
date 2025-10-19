using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;
using SolarPositionCalculator.Views;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// Composite ViewModel that holds all the individual ViewModels
/// </summary>
public partial class CompositeViewModel : ObservableObject, IDisposable
{
    private readonly ICsvExportService _csvExportService;
    private readonly IVisualizationService _visualizationService;
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel MainViewModel { get; }
    public EquationOfTimeViewModel EquationOfTimeViewModel { get; }
    public SunPathViewModel SunPathViewModel { get; }

    public CompositeViewModel(
        ICsvExportService csvExportService,
        IVisualizationService visualizationService,
        IServiceProvider serviceProvider,
        MainViewModel mainViewModel,
        EquationOfTimeViewModel equationOfTimeViewModel,
        SunPathViewModel sunPathViewModel)
    {
        _csvExportService = csvExportService ?? throw new ArgumentNullException(nameof(csvExportService));
        _visualizationService = visualizationService ?? throw new ArgumentNullException(nameof(visualizationService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        MainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        EquationOfTimeViewModel = equationOfTimeViewModel ?? throw new ArgumentNullException(nameof(equationOfTimeViewModel));
        SunPathViewModel = sunPathViewModel ?? throw new ArgumentNullException(nameof(sunPathViewModel));
    }

    /// <summary>
    /// Command to export solar position data to CSV
    /// </summary>
    [RelayCommand]
    private void ExportData()
    {
        try
        {
            var csvExportViewModel = _serviceProvider.GetRequiredService<CsvExportDialogViewModel>();

            // Set current location from MainViewModel
            csvExportViewModel.Location = new GeographicCoordinate(MainViewModel.Latitude, MainViewModel.Longitude);

            var dialog = new CsvExportDialog(csvExportViewModel);
            var result = dialog.ShowDialog(Application.Current.MainWindow);

            if (result == true)
            {
                MainViewModel.StatusMessage = "Data export completed successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Command to export charts and visualizations
    /// </summary>
    [RelayCommand]
    private void ExportCharts()
    {
        try
        {
            var charts = new Dictionary<string, OxyPlot.PlotModel>();

            // Add equation of time chart if available
            if (EquationOfTimeViewModel?.EquationOfTimeChart?.PlotModel != null)
            {
                charts["EquationOfTime"] = EquationOfTimeViewModel.EquationOfTimeChart.PlotModel;
            }

            // Add sun path chart if available
            if (SunPathViewModel?.SunPathChart?.PlotModel != null)
            {
                charts["SunPath"] = SunPathViewModel.SunPathChart.PlotModel;
            }

            if (charts.Count == 0)
            {
                MessageBox.Show("No charts available for export. Please generate visualizations first.",
                              "No Charts Available", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var success = _visualizationService.ShowBatchExportDialog(charts);

            if (success)
            {
                MainViewModel.StatusMessage = "Chart export completed successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Chart export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Dispose()
    {
        MainViewModel?.Dispose();
        EquationOfTimeViewModel?.Dispose();
        SunPathViewModel?.Dispose();
    }
}