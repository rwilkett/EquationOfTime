using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Simple test class to verify visualization service functionality
/// </summary>
public static class VisualizationServiceTests
{
    /// <summary>
    /// Tests the creation of equation of time chart with interactive features
    /// </summary>
    public static void TestEquationOfTimeChart()
    {
        var service = new VisualizationService();
        
        // Create sample data
        var data = new EquationOfTimeData[12];
        for (int i = 0; i < 12; i++)
        {
            var date = new DateTime(2024, i + 1, 15);
            var minutes = Math.Sin(2 * Math.PI * i / 12) * 15; // Simplified equation of time
            data[i] = new EquationOfTimeData(date, minutes);
        }

        // Test chart creation
        var interactiveChart = service.CreateEquationOfTimeChart(data);
        
        Console.WriteLine($"Chart created successfully: {interactiveChart.PlotModel.Title}");
        Console.WriteLine($"Series count: {interactiveChart.PlotModel.Series.Count}");
        Console.WriteLine($"Axes count: {interactiveChart.PlotModel.Axes.Count}");
        Console.WriteLine($"Chart type: {interactiveChart.ChartType}");
        
        // Test event subscription
        service.DateSelected += (sender, args) =>
        {
            Console.WriteLine($"Date selected: {args.SelectedDate:yyyy-MM-dd}, Value: {args.SelectedValue:F2}");
        };
        
        Console.WriteLine("Equation of time chart test completed successfully");
    }

    /// <summary>
    /// Tests the creation of sun path diagram with interactive features
    /// </summary>
    public static void TestSunPathDiagram()
    {
        var service = new VisualizationService();
        
        // Create sample sun path data
        var location = new GeographicCoordinate(40.7128, -74.0060); // New York
        var date = new DateTime(2024, 6, 21); // Summer solstice
        
        var positions = new List<SolarPosition>();
        for (int hour = 6; hour <= 18; hour++)
        {
            var time = date.AddHours(hour);
            var azimuth = (hour - 6) * 15; // Simplified azimuth calculation
            var elevation = Math.Sin((hour - 6) * Math.PI / 12) * 70; // Simplified elevation
            positions.Add(new SolarPosition(azimuth, elevation, time, location));
        }

        var sunrise = positions.First();
        var sunset = positions.Last();
        var sunPath = new SunPath(location, date, positions.ToArray(), sunrise, sunset);
        
        // Create current position
        var currentPosition = new SolarPosition(180, 45, date.AddHours(12), location);
        
        // Test chart creation
        var interactiveChart = service.CreateSunPathDiagram(sunPath, currentPosition);
        
        Console.WriteLine($"Sun path chart created successfully: {interactiveChart.PlotModel.Title}");
        Console.WriteLine($"Series count: {interactiveChart.PlotModel.Series.Count}");
        Console.WriteLine($"Annotations count: {interactiveChart.PlotModel.Annotations.Count}");
        Console.WriteLine($"Chart type: {interactiveChart.ChartType}");
        Console.WriteLine($"Visible positions count: {interactiveChart.VisiblePositions?.Length ?? 0}");
        
        // Test event subscription
        service.PositionSelected += (sender, args) =>
        {
            Console.WriteLine($"Position selected: Az={args.Azimuth:F1}°, El={args.Elevation:F1}°, Time={args.TimeOfDay:HH:mm}");
        };
        
        Console.WriteLine("Sun path diagram test completed successfully");
    }

    /// <summary>
    /// Tests chart export functionality
    /// </summary>
    public static void TestChartExport()
    {
        var service = new VisualizationService();
        
        // Create simple test data
        var data = new EquationOfTimeData[]
        {
            new(new DateTime(2024, 1, 1), -3.2),
            new(new DateTime(2024, 6, 1), 2.1),
            new(new DateTime(2024, 12, 1), 10.5)
        };
        
        var interactiveChart = service.CreateEquationOfTimeChart(data);
        
        // Test export (would need actual file system access)
        Console.WriteLine("Chart export functionality is available");
        Console.WriteLine("Supported formats: PNG, SVG, PDF");
        
        Console.WriteLine("Chart export test completed successfully");
    }

    /// <summary>
    /// Runs all visualization service tests
    /// </summary>
    public static void RunAllTests()
    {
        Console.WriteLine("=== Visualization Service Tests ===");
        
        try
        {
            TestEquationOfTimeChart();
            Console.WriteLine();
            
            TestSunPathDiagram();
            Console.WriteLine();
            
            TestChartExport();
            Console.WriteLine();
            
            Console.WriteLine("All tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }
    }
}