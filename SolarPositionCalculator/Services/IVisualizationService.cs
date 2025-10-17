using OxyPlot;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for visualization services
/// </summary>
public interface IVisualizationService
{
    /// <summary>
    /// Event fired when a date is selected on the equation of time chart
    /// </summary>
    event EventHandler<ChartInteractionEventArgs>? DateSelected;

    /// <summary>
    /// Event fired when a position is selected on the sun path diagram
    /// </summary>
    event EventHandler<SunPathInteractionEventArgs>? PositionSelected;
    /// <summary>
    /// Creates an interactive equation of time chart
    /// </summary>
    /// <param name="data">Equation of time data</param>
    /// <returns>Interactive plot model</returns>
    InteractivePlotModel CreateEquationOfTimeChart(EquationOfTimeData[] data);

    /// <summary>
    /// Creates an interactive sun path diagram
    /// </summary>
    /// <param name="sunPath">Sun path data</param>
    /// <param name="currentPosition">Current solar position</param>
    /// <returns>Interactive plot model</returns>
    InteractivePlotModel CreateSunPathDiagram(SunPath sunPath, SolarPosition currentPosition);

    /// <summary>
    /// Creates a specialized polar region sun path diagram
    /// </summary>
    /// <param name="sunPath">Sun path data</param>
    /// <param name="polarCondition">Polar condition information</param>
    /// <param name="currentPosition">Current solar position</param>
    /// <returns>Interactive plot model</returns>
    InteractivePlotModel CreatePolarSunPathDiagram(SunPath sunPath, PolarCondition polarCondition, SolarPosition? currentPosition = null);

    /// <summary>
    /// Exports a chart with specified options
    /// </summary>
    /// <param name="chart">Chart to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    void ExportChartWithOptions(PlotModel chart, string filePath, ExportOptions options);

    /// <summary>
    /// Shows an export dialog for a single chart
    /// </summary>
    /// <param name="chart">Chart to export</param>
    /// <param name="defaultFileName">Default file name</param>
    /// <returns>True if export was successful</returns>
    bool ShowExportDialog(PlotModel chart, string defaultFileName);

    /// <summary>
    /// Shows a batch export dialog for multiple charts
    /// </summary>
    /// <param name="charts">Dictionary of chart names and plot models</param>
    /// <returns>True if export was successful</returns>
    bool ShowBatchExportDialog(Dictionary<string, PlotModel> charts);

    /// <summary>
    /// Performs batch export of charts
    /// </summary>
    /// <param name="charts">Charts to export</param>
    /// <param name="options">Batch export options</param>
    /// <returns>Number of charts exported successfully</returns>
    int BatchExportCharts(IEnumerable<ChartExportInfo> charts, BatchExportOptions options);
}

