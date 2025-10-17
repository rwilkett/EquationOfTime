using OxyPlot;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Utility class for chart export operations
/// </summary>
public static class ExportUtility
{
    /// <summary>
    /// Creates a sample equation of time chart for testing export functionality
    /// </summary>
    public static PlotModel CreateSampleEquationOfTimeChart()
    {
        var data = GenerateSampleEquationOfTimeData();
        var visualizationService = new VisualizationService();
        var interactivePlot = visualizationService.CreateEquationOfTimeChart(data);
        return interactivePlot.PlotModel;
    }

    /// <summary>
    /// Creates a sample sun path chart for testing export functionality
    /// </summary>
    public static PlotModel CreateSampleSunPathChart()
    {
        var sunPath = GenerateSampleSunPath();
        var currentPosition = new SolarPosition(180, 45, DateTime.Now, new GeographicCoordinate(40.7128, -74.0060));
        var visualizationService = new VisualizationService();
        var interactivePlot = visualizationService.CreateSunPathDiagram(sunPath, currentPosition);
        return interactivePlot.PlotModel;
    }

    /// <summary>
    /// Demonstrates single chart export with dialog
    /// </summary>
    public static bool ExportSingleChartWithDialog(PlotModel chart, string defaultFileName = "sample_chart")
    {
        var visualizationService = new VisualizationService();
        return visualizationService.ShowExportDialog(chart, defaultFileName);
    }

    /// <summary>
    /// Demonstrates batch export with dialog
    /// </summary>
    public static bool ExportMultipleChartsWithDialog()
    {
        var charts = new Dictionary<string, PlotModel>
        {
            ["EquationOfTime"] = CreateSampleEquationOfTimeChart(),
            ["SunPath"] = CreateSampleSunPathChart(),
            ["CurrentDayPath"] = CreateSampleSunPathChart() // Using same for demo
        };

        var visualizationService = new VisualizationService();
        return visualizationService.ShowBatchExportDialog(charts);
    }

    /// <summary>
    /// Demonstrates programmatic export with custom options
    /// </summary>
    public static void ExportWithCustomOptions(PlotModel chart, string outputPath)
    {
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.Png,
            Width = 1200,
            Height = 800,
            Resolution = 150,
            Quality = 95,
            BackgroundColor = "White",
            IncludeTitle = true,
            IncludeLegend = true
        };

        var visualizationService = new VisualizationService();
        visualizationService.ExportChartWithOptions(chart, outputPath, exportOptions);
    }

    /// <summary>
    /// Demonstrates programmatic batch export
    /// </summary>
    public static int BatchExportProgrammatically(string outputDirectory)
    {
        var charts = new List<ChartExportInfo>
        {
            new()
            {
                Chart = CreateSampleEquationOfTimeChart(),
                ChartType = "EquationOfTime",
                CustomFileName = "equation_of_time_2024"
            },
            new()
            {
                Chart = CreateSampleSunPathChart(),
                ChartType = "SunPath",
                CustomFileName = "sun_path_diagram"
            }
        };

        var batchOptions = new BatchExportOptions
        {
            ExportOptions = new ExportOptions
            {
                Format = ExportFormat.Png,
                Width = 1000,
                Height = 750,
                Quality = 90,
                IncludeTitle = true,
                IncludeLegend = true
            },
            OutputDirectory = outputDirectory,
            FileNamePrefix = "solar_chart",
            IncludeTimestamp = true,
            GroupByChartType = true
        };

        var visualizationService = new VisualizationService();
        return visualizationService.BatchExportCharts(charts, batchOptions);
    }

    private static EquationOfTimeData[] GenerateSampleEquationOfTimeData()
    {
        var data = new List<EquationOfTimeData>();
        var startDate = new DateTime(DateTime.Now.Year, 1, 1);
        
        for (int day = 0; day < 365; day++)
        {
            var date = startDate.AddDays(day);
            var dayOfYear = date.DayOfYear;
            
            // Simplified equation of time calculation for demo
            var B = 2 * Math.PI * (dayOfYear - 81) / 365.0;
            var equationOfTime = 9.87 * Math.Sin(2 * B) - 7.53 * Math.Cos(B) - 1.5 * Math.Sin(B);
            
            data.Add(new EquationOfTimeData(date, equationOfTime));
        }
        
        return data.ToArray();
    }

    private static SunPath GenerateSampleSunPath()
    {
        var location = new GeographicCoordinate(40.7128, -74.0060); // New York City
        var date = DateTime.Today;
        var positions = new List<SolarPosition>();
        
        // Generate hourly positions for demonstration
        for (int hour = 6; hour <= 18; hour++)
        {
            var time = date.AddHours(hour);
            var azimuth = 90 + (hour - 12) * 15; // Simplified calculation
            var elevation = Math.Max(0, 60 - Math.Abs(hour - 12) * 5); // Simplified calculation
            
            positions.Add(new SolarPosition(azimuth, elevation, time, location));
        }
        
        var sunrise = positions.FirstOrDefault(p => p.IsSunVisible);
        var sunset = positions.LastOrDefault(p => p.IsSunVisible);
        
        return new SunPath(location, date, positions.ToArray(), sunrise, sunset);
    }
}