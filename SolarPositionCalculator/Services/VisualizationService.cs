using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Wpf;
using SolarPositionCalculator.Models;
using System.IO;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of visualization service for creating interactive charts
/// </summary>
public class VisualizationService : IVisualizationService
{

    /// <summary>
    /// Creates an interactive equation of time chart with hover tooltips and click selection
    /// </summary>
    public InteractivePlotModel CreateEquationOfTimeChart(EquationOfTimeData[] data)
    {
        var plotModel = new PlotModel
        {
            Title = "Equation of Time",
            Background = OxyColors.White,
            PlotAreaBorderColor = OxyColors.Black,
            PlotAreaBorderThickness = new OxyThickness(1)
        };

        // Configure axes
        var dateAxis = new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Date",
            StringFormat = "MMM",
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray
        };

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Minutes",
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray,
            Minimum = -20,
            Maximum = 20
        };

        plotModel.Axes.Add(dateAxis);
        plotModel.Axes.Add(valueAxis);

        // Create line series with interactive features
        var lineSeries = new LineSeries
        {
            Title = "Equation of Time",
            Color = OxyColors.Blue,
            StrokeThickness = 2,
            MarkerType = MarkerType.None,
            CanTrackerInterpolatePoints = true,
            TrackerFormatString = "{0}\n{1}: {2:MMM dd}\n{3}: {4:F2} min"
        };

        // Add data points
        foreach (var point in data)
        {
            lineSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.Date, point.Minutes));
        }

        plotModel.Series.Add(lineSeries);

        // Add zero reference line
        var zeroLine = new LineSeries
        {
            Color = OxyColors.Red,
            StrokeThickness = 1,
            LineStyle = LineStyle.Dash,
            IsVisible = true
        };
        
        var startDate = data.First().Date;
        var endDate = data.Last().Date;
        zeroLine.Points.Add(DateTimeAxis.CreateDataPoint(startDate, 0));
        zeroLine.Points.Add(DateTimeAxis.CreateDataPoint(endDate, 0));
        
        plotModel.Series.Add(zeroLine);

        return new InteractivePlotModel(plotModel, "EquationOfTime");
    }

    /// <summary>
    /// Creates an interactive sun path diagram with current position highlighting
    /// </summary>
    public InteractivePlotModel CreateSunPathDiagram(SunPath sunPath, SolarPosition? currentPosition = null)
    {
        var plotModel = new PlotModel
        {
            Title = $"Sun Path - {sunPath.Date:yyyy-MM-dd}",
            Background = OxyColors.White,
            PlotAreaBorderColor = OxyColors.Black,
            PlotAreaBorderThickness = new OxyThickness(1)
        };

        // Configure polar-style axes for sky dome
        var azimuthAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Azimuth (°)",
            Minimum = 0,
            Maximum = 360,
            MajorStep = 45,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray
        };

        var elevationAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Elevation (°)",
            Minimum = -10,
            Maximum = 90,
            MajorStep = 15,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray
        };

        plotModel.Axes.Add(azimuthAxis);
        plotModel.Axes.Add(elevationAxis);

        // Create sun path line series
        var sunPathSeries = new LineSeries
        {
            Title = "Sun Path",
            Color = OxyColors.Orange,
            StrokeThickness = 2,
            MarkerType = MarkerType.None,
            CanTrackerInterpolatePoints = true,
            TrackerFormatString = "{0}\n{1}: {2:F1}°\n{3}: {4:F1}°\nTime: {Tag}"
        };

        // Add visible sun path points only
        var visiblePositions = sunPath.DailyPositions.Where(p => p.IsSunVisible).ToArray();
        foreach (var position in visiblePositions)
        {
            sunPathSeries.Points.Add(new DataPoint(position.Azimuth, position.Elevation));
        }

        plotModel.Series.Add(sunPathSeries);

        // Add sunrise/sunset markers
        if (sunPath.HasSunrise && sunPath.Sunrise != null)
        {
            var sunriseMarker = new ScatterSeries
            {
                Title = "Sunrise",
                MarkerType = MarkerType.Circle,
                MarkerSize = 8,
                MarkerFill = OxyColors.Yellow,
                MarkerStroke = OxyColors.Orange,
                MarkerStrokeThickness = 2
            };
            sunriseMarker.Points.Add(new ScatterPoint(sunPath.Sunrise.Azimuth, sunPath.Sunrise.Elevation));
            plotModel.Series.Add(sunriseMarker);
        }

        if (sunPath.HasSunset && sunPath.Sunset != null)
        {
            var sunsetMarker = new ScatterSeries
            {
                Title = "Sunset",
                MarkerType = MarkerType.Circle,
                MarkerSize = 8,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.DarkRed,
                MarkerStrokeThickness = 2
            };
            sunsetMarker.Points.Add(new ScatterPoint(sunPath.Sunset.Azimuth, sunPath.Sunset.Elevation));
            plotModel.Series.Add(sunsetMarker);
        }

        // Highlight current position if provided
        if (currentPosition != null && currentPosition.IsSunVisible)
        {
            var currentPositionMarker = new ScatterSeries
            {
                Title = "Current Position",
                MarkerType = MarkerType.Star,
                MarkerSize = 12,
                MarkerFill = OxyColors.Gold,
                MarkerStroke = OxyColors.DarkGoldenrod,
                MarkerStrokeThickness = 2
            };
            currentPositionMarker.Points.Add(new ScatterPoint(currentPosition.Azimuth, currentPosition.Elevation));
            plotModel.Series.Add(currentPositionMarker);

            // Add annotation for current position
            var annotation = new TextAnnotation
            {
                Text = $"Current\n{currentPosition.Timestamp:HH:mm}",
                TextPosition = new DataPoint(currentPosition.Azimuth, currentPosition.Elevation + 5),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 10,
                Background = OxyColors.White,
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };
            plotModel.Annotations.Add(annotation);
        }

        // Add horizon line
        var horizonLine = new LineSeries
        {
            Title = "Horizon",
            Color = OxyColors.Brown,
            StrokeThickness = 2,
            LineStyle = LineStyle.Solid
        };
        horizonLine.Points.Add(new DataPoint(0, 0));
        horizonLine.Points.Add(new DataPoint(360, 0));
        plotModel.Series.Add(horizonLine);

        // Handle special polar conditions with enhanced visualizations
        AddPolarConditionVisualization(plotModel, sunPath);

        return new InteractivePlotModel(plotModel, "SunPath", visiblePositions);
    }

    /// <summary>
    /// Creates a specialized polar region sun path diagram
    /// </summary>
    public InteractivePlotModel CreatePolarSunPathDiagram(SunPath sunPath, PolarCondition polarCondition, SolarPosition? currentPosition = null)
    {
        var plotModel = new PlotModel
        {
            Title = $"Polar Sun Path - {sunPath.Date:yyyy-MM-dd} ({polarCondition.Type})",
            Background = OxyColors.White,
            PlotAreaBorderColor = OxyColors.Black,
            PlotAreaBorderThickness = new OxyThickness(1)
        };

        // Configure axes with extended range for polar conditions
        var azimuthAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Azimuth (°)",
            Minimum = 0,
            Maximum = 360,
            MajorStep = 30,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray
        };

        var elevationAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Elevation (°)",
            Minimum = polarCondition.Type == PolarConditionType.PolarNight ? -25 : -10,
            Maximum = 90,
            MajorStep = polarCondition.Type == PolarConditionType.PolarNight ? 5 : 15,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColors.LightGray
        };

        plotModel.Axes.Add(azimuthAxis);
        plotModel.Axes.Add(elevationAxis);

        // Get visible positions for interaction
        var visiblePositions = sunPath.DailyPositions.Where(p => p.IsSunVisible).ToArray();

        // Create sun path with special styling for polar conditions
        var sunPathSeries = new LineSeries
        {
            Title = "Sun Path",
            Color = GetPolarConditionColor(polarCondition.Type),
            StrokeThickness = 3,
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            CanTrackerInterpolatePoints = true,
            TrackerFormatString = "{0}\n{1}: {2:F1}°\n{3}: {4:F1}°\nTime: {Tag}"
        };

        // Add all positions (including below horizon for polar night)
        foreach (var position in sunPath.DailyPositions)
        {
            sunPathSeries.Points.Add(new DataPoint(position.Azimuth, position.Elevation));
        }

        plotModel.Series.Add(sunPathSeries);

        // Add twilight zones for polar night
        if (polarCondition.Type == PolarConditionType.PolarNight || 
            polarCondition.Type == PolarConditionType.CivilTwilight ||
            polarCondition.Type == PolarConditionType.NauticalTwilight ||
            polarCondition.Type == PolarConditionType.AstronomicalTwilight)
        {
            AddTwilightZones(plotModel);
        }

        // Add current position if provided
        if (currentPosition != null)
        {
            var currentPositionMarker = new ScatterSeries
            {
                Title = "Current Position",
                MarkerType = MarkerType.Star,
                MarkerSize = 15,
                MarkerFill = OxyColors.Gold,
                MarkerStroke = OxyColors.DarkGoldenrod,
                MarkerStrokeThickness = 3
            };
            currentPositionMarker.Points.Add(new ScatterPoint(currentPosition.Azimuth, currentPosition.Elevation));
            plotModel.Series.Add(currentPositionMarker);

            // Add detailed annotation for current position
            var annotation = new TextAnnotation
            {
                Text = $"Current Position\n{currentPosition.Timestamp:HH:mm}\nEl: {currentPosition.Elevation:F1}°",
                TextPosition = new DataPoint(currentPosition.Azimuth, currentPosition.Elevation + 3),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 10,
                Background = OxyColors.White,
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };
            plotModel.Annotations.Add(annotation);
        }

        // Add horizon and twilight reference lines
        AddHorizonAndTwilightLines(plotModel);

        // Add polar condition specific annotations
        AddPolarConditionAnnotations(plotModel, polarCondition);

        return new InteractivePlotModel(plotModel, "PolarSunPath", sunPath.DailyPositions);
    }

    /// <summary>
    /// Adds polar condition visualization elements to the plot
    /// </summary>
    private void AddPolarConditionVisualization(PlotModel plotModel, SunPath sunPath)
    {
        if (sunPath.IsPolarDay)
        {
            // Add midnight sun visualization
            var polarDayAnnotation = new TextAnnotation
            {
                Text = $"MIDNIGHT SUN\nMax Elevation: {sunPath.MaxElevation:F1}°",
                TextPosition = new DataPoint(180, 80),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextColor = OxyColors.Red,
                Background = OxyColors.LightYellow,
                Stroke = OxyColors.Red,
                StrokeThickness = 2
            };
            plotModel.Annotations.Add(polarDayAnnotation);

            // Add sun visibility indicator
            var visibilityArea = new RectangleAnnotation
            {
                MinimumX = 0,
                MaximumX = 360,
                MinimumY = 0,
                MaximumY = 90,
                Fill = OxyColor.FromArgb(30, 255, 255, 0), // Light yellow overlay
                Text = "Sun Always Visible"
            };
            plotModel.Annotations.Add(visibilityArea);
        }
        else if (sunPath.IsPolarNight)
        {
            var conditionText = sunPath.MaxElevation > -6 ? "CIVIL TWILIGHT" :
                               sunPath.MaxElevation > -12 ? "NAUTICAL TWILIGHT" :
                               sunPath.MaxElevation > -18 ? "ASTRONOMICAL TWILIGHT" : "POLAR NIGHT";

            var polarNightAnnotation = new TextAnnotation
            {
                Text = $"{conditionText}\nMax Elevation: {sunPath.MaxElevation:F1}°",
                TextPosition = new DataPoint(180, Math.Max(sunPath.MaxElevation + 5, -5)),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextColor = OxyColors.DarkBlue,
                Background = OxyColors.LightBlue,
                Stroke = OxyColors.DarkBlue,
                StrokeThickness = 2
            };
            plotModel.Annotations.Add(polarNightAnnotation);
        }

        // This method adds visualization elements to the existing plot model
    }

    /// <summary>
    /// Exports a chart to the specified file path and format
    /// </summary>
    public void ExportChart(PlotModel chart, string filePath, ExportFormat format)
    {
        var defaultOptions = new ExportOptions { Format = format };
        ExportChartWithOptions(chart, filePath, defaultOptions);
    }

    /// <summary>
    /// Exports a chart with advanced options
    /// </summary>
    public void ExportChartWithOptions(PlotModel chart, string filePath, ExportOptions options)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create a copy of the chart to modify for export
        var exportChart = CloneChartForExport(chart, options);

        switch (options.Format)
        {
            case ExportFormat.Png:
                var pngExporter = new OxyPlot.Wpf.PngExporter
                {
                    Width = options.Width,
                    Height = options.Height,
                    Resolution = options.Resolution
                };
                pngExporter.ExportToFile(exportChart, filePath);
                break;

            case ExportFormat.Svg:
                var svgExporter = new OxyPlot.Wpf.SvgExporter
                {
                    Width = options.Width,
                    Height = options.Height
                };
                svgExporter.ExportToFile(exportChart, filePath);
                break;

            case ExportFormat.Pdf:
                throw new NotSupportedException("PDF export is not supported in this version. Use PNG or SVG instead.");

            default:
                throw new ArgumentException($"Unsupported export format: {options.Format}");
        }
    }

    /// <summary>
    /// Shows export dialog and exports chart if user confirms
    /// </summary>
    public bool ShowExportDialog(PlotModel chart, string defaultFileName = "chart")
    {
        var dialog = new Views.ExportDialog();
        
        // Set default file path
        var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            $"{defaultFileName}.png");
        dialog.FilePathTextBox.Text = defaultPath;

        if (dialog.ShowDialog() == true && dialog.ExportOptions != null && dialog.SelectedFilePath != null)
        {
            try
            {
                ExportChartWithOptions(chart, dialog.SelectedFilePath, dialog.ExportOptions);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Export failed: {ex.Message}", "Export Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Shows batch export dialog and exports selected charts
    /// </summary>
    public bool ShowBatchExportDialog(Dictionary<string, PlotModel> availableCharts)
    {
        var dialog = new Views.BatchExportDialog
        {
            AvailableCharts = availableCharts
        };

        if (dialog.ShowDialog() == true && dialog.BatchExportOptions != null)
        {
            try
            {
                // Create chart export info list from selected charts
                var chartsToExport = new List<ChartExportInfo>();
                
                foreach (var chartType in dialog.SelectedChartTypes)
                {
                    if (availableCharts.TryGetValue(chartType, out var chart))
                    {
                        chartsToExport.Add(new ChartExportInfo
                        {
                            Chart = chart,
                            ChartType = chartType,
                            CustomFileName = null // Use default naming
                        });
                    }
                }

                // Perform batch export
                var successCount = BatchExportCharts(chartsToExport, dialog.BatchExportOptions);
                
                System.Windows.MessageBox.Show($"Successfully exported {successCount} of {chartsToExport.Count} charts.", 
                    "Batch Export Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Batch export failed: {ex.Message}", "Export Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Exports multiple charts in batch
    /// </summary>
    public int BatchExportCharts(IEnumerable<ChartExportInfo> charts, BatchExportOptions batchOptions)
    {
        int successCount = 0;
        var timestamp = batchOptions.IncludeTimestamp ? DateTime.Now.ToString("yyyyMMdd_HHmmss") : "";

        foreach (var chartInfo in charts)
        {
            try
            {
                // Generate file name
                var fileName = GenerateBatchFileName(chartInfo, batchOptions, timestamp);
                
                // Determine output directory
                var outputDir = batchOptions.OutputDirectory;
                if (batchOptions.GroupByChartType)
                {
                    outputDir = Path.Combine(outputDir, chartInfo.ChartType);
                }

                // Ensure directory exists
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                var filePath = Path.Combine(outputDir, fileName);
                
                // Export the chart
                ExportChartWithOptions(chartInfo.Chart, filePath, batchOptions.ExportOptions);
                successCount++;
            }
            catch (Exception ex)
            {
                // Log error but continue with other charts
                System.Diagnostics.Debug.WriteLine($"Failed to export chart {chartInfo.ChartType}: {ex.Message}");
            }
        }

        return successCount;
    }

    /// <summary>
    /// Event raised when a date is selected from the equation of time chart
    /// </summary>
    public event EventHandler<ChartInteractionEventArgs>? DateSelected;

    /// <summary>
    /// Event raised when a position is selected from the sun path diagram
    /// </summary>
    public event EventHandler<SunPathInteractionEventArgs>? PositionSelected;

    /// <summary>
    /// Handles mouse click on interactive chart for selection
    /// </summary>
    /// <param name="interactivePlot">The interactive plot model that was clicked</param>
    /// <param name="screenPosition">Screen position of the click</param>
    public void HandleChartClick(InteractivePlotModel interactivePlot, ScreenPoint screenPosition)
    {
        if (interactivePlot.ChartType == "EquationOfTime")
        {
            var series = interactivePlot.PlotModel.Series.FirstOrDefault() as LineSeries;
            if (series != null)
            {
                var point = series.GetNearestPoint(screenPosition, false);
                if (point != null)
                {
                    var selectedDate = DateTime.FromOADate(point.DataPoint.X);
                    var selectedValue = point.DataPoint.Y;
                    DateSelected?.Invoke(this, new ChartInteractionEventArgs(selectedDate, selectedValue, "EquationOfTime"));
                }
            }
        }
        else if (interactivePlot.ChartType == "SunPath" && interactivePlot.VisiblePositions != null)
        {
            var sunPathLineSeries = interactivePlot.PlotModel.Series.FirstOrDefault(series => series.Title == "Sun Path") as LineSeries;
            
            if (sunPathLineSeries != null)
            {
                var point = sunPathLineSeries.GetNearestPoint(screenPosition, false);
                if (point != null)
                {
                    var azimuth = point.DataPoint.X;
                    var elevation = point.DataPoint.Y;
                    
                    // Find the corresponding time from the original data
                    DateTime? timeOfDay = null;
                    var nearestPosition = interactivePlot.VisiblePositions
                        .OrderBy(p => Math.Abs(p.Azimuth - azimuth) + Math.Abs(p.Elevation - elevation))
                        .FirstOrDefault();
                    
                    if (nearestPosition != null)
                    {
                        timeOfDay = nearestPosition.Timestamp;
                    }

                    PositionSelected?.Invoke(this, new SunPathInteractionEventArgs(azimuth, elevation, timeOfDay));
                }
            }
        }
    }

    /// <summary>
    /// Creates a copy of the chart with export-specific modifications
    /// </summary>
    private PlotModel CloneChartForExport(PlotModel originalChart, ExportOptions options)
    {
        var exportChart = new PlotModel();
        
        // Copy basic properties
        exportChart.Title = options.IncludeTitle ? originalChart.Title : null;
        exportChart.Subtitle = options.IncludeTitle ? originalChart.Subtitle : null;
        
        // Set background color
        exportChart.Background = ParseBackgroundColor(options.BackgroundColor);
        exportChart.PlotAreaBorderColor = originalChart.PlotAreaBorderColor;
        exportChart.PlotAreaBorderThickness = originalChart.PlotAreaBorderThickness;

        // Copy axes
        foreach (var axis in originalChart.Axes)
        {
            exportChart.Axes.Add(axis);
        }

        // Copy series
        foreach (var series in originalChart.Series)
        {
            exportChart.Series.Add(series);
        }

        // Copy annotations
        foreach (var annotation in originalChart.Annotations)
        {
            exportChart.Annotations.Add(annotation);
        }

        // Handle legend visibility
        if (!options.IncludeLegend)
        {
            exportChart.IsLegendVisible = false;
        }
        else
        {
            exportChart.IsLegendVisible = originalChart.IsLegendVisible;
            // Note: LegendPosition and LegendPlacement properties may not be available in all OxyPlot versions
        }

        return exportChart;
    }

    /// <summary>
    /// Parses background color string to OxyColor
    /// </summary>
    private OxyColor ParseBackgroundColor(string colorString)
    {
        return colorString.ToLowerInvariant() switch
        {
            "white" => OxyColors.White,
            "transparent" => OxyColors.Transparent,
            "black" => OxyColors.Black,
            _ when colorString.StartsWith("#") => OxyColor.Parse(colorString),
            _ => OxyColors.White
        };
    }

    /// <summary>
    /// Generates file name for batch export
    /// </summary>
    private string GenerateBatchFileName(ChartExportInfo chartInfo, BatchExportOptions batchOptions, string timestamp)
    {
        var baseName = chartInfo.CustomFileName ?? $"{batchOptions.FileNamePrefix}_{chartInfo.ChartType}";
        
        if (batchOptions.IncludeTimestamp && !string.IsNullOrEmpty(timestamp))
        {
            baseName += $"_{timestamp}";
        }

        var extension = batchOptions.ExportOptions.Format switch
        {
            ExportFormat.Png => ".png",
            ExportFormat.Svg => ".svg",
            _ => ".png"
        };

        return baseName + extension;
    }

    /// <summary>
    /// Gets the appropriate color for different polar conditions
    /// </summary>
    private OxyColor GetPolarConditionColor(PolarConditionType conditionType)
    {
        return conditionType switch
        {
            PolarConditionType.MidnightSun => OxyColors.Gold,
            PolarConditionType.PolarNight => OxyColors.DarkBlue,
            PolarConditionType.CivilTwilight => OxyColors.SkyBlue,
            PolarConditionType.NauticalTwilight => OxyColors.RoyalBlue,
            PolarConditionType.AstronomicalTwilight => OxyColors.MidnightBlue,
            _ => OxyColors.Orange
        };
    }

    /// <summary>
    /// Adds twilight zones to the plot for polar night conditions
    /// </summary>
    private void AddTwilightZones(PlotModel plotModel)
    {
        // Civil twilight zone (0° to -6°)
        var civilTwilight = new RectangleAnnotation
        {
            MinimumX = 0,
            MaximumX = 360,
            MinimumY = -6,
            MaximumY = 0,
            Fill = OxyColor.FromArgb(40, 135, 206, 235), // Light sky blue
            Text = "Civil Twilight"
        };
        plotModel.Annotations.Add(civilTwilight);

        // Nautical twilight zone (-6° to -12°)
        var nauticalTwilight = new RectangleAnnotation
        {
            MinimumX = 0,
            MaximumX = 360,
            MinimumY = -12,
            MaximumY = -6,
            Fill = OxyColor.FromArgb(40, 65, 105, 225), // Royal blue
            Text = "Nautical Twilight"
        };
        plotModel.Annotations.Add(nauticalTwilight);

        // Astronomical twilight zone (-12° to -18°)
        var astronomicalTwilight = new RectangleAnnotation
        {
            MinimumX = 0,
            MaximumX = 360,
            MinimumY = -18,
            MaximumY = -12,
            Fill = OxyColor.FromArgb(40, 25, 25, 112), // Midnight blue
            Text = "Astronomical Twilight"
        };
        plotModel.Annotations.Add(astronomicalTwilight);
    }

    /// <summary>
    /// Adds horizon and twilight reference lines
    /// </summary>
    private void AddHorizonAndTwilightLines(PlotModel plotModel)
    {
        // Horizon line
        var horizonLine = new LineSeries
        {
            Title = "Horizon",
            Color = OxyColors.Brown,
            StrokeThickness = 3,
            LineStyle = LineStyle.Solid
        };
        horizonLine.Points.Add(new DataPoint(0, 0));
        horizonLine.Points.Add(new DataPoint(360, 0));
        plotModel.Series.Add(horizonLine);

        // Civil twilight line
        var civilTwilightLine = new LineSeries
        {
            Title = "Civil Twilight (-6°)",
            Color = OxyColors.SkyBlue,
            StrokeThickness = 2,
            LineStyle = LineStyle.Dash
        };
        civilTwilightLine.Points.Add(new DataPoint(0, -6));
        civilTwilightLine.Points.Add(new DataPoint(360, -6));
        plotModel.Series.Add(civilTwilightLine);

        // Nautical twilight line
        var nauticalTwilightLine = new LineSeries
        {
            Title = "Nautical Twilight (-12°)",
            Color = OxyColors.RoyalBlue,
            StrokeThickness = 2,
            LineStyle = LineStyle.Dot
        };
        nauticalTwilightLine.Points.Add(new DataPoint(0, -12));
        nauticalTwilightLine.Points.Add(new DataPoint(360, -12));
        plotModel.Series.Add(nauticalTwilightLine);

        // Astronomical twilight line
        var astronomicalTwilightLine = new LineSeries
        {
            Title = "Astronomical Twilight (-18°)",
            Color = OxyColors.MidnightBlue,
            StrokeThickness = 2,
            LineStyle = LineStyle.DashDot
        };
        astronomicalTwilightLine.Points.Add(new DataPoint(0, -18));
        astronomicalTwilightLine.Points.Add(new DataPoint(360, -18));
        plotModel.Series.Add(astronomicalTwilightLine);
    }

    /// <summary>
    /// Adds polar condition specific annotations
    /// </summary>
    private void AddPolarConditionAnnotations(PlotModel plotModel, PolarCondition polarCondition)
    {
        var mainAnnotation = new TextAnnotation
        {
            Text = polarCondition.GetUserMessage(),
            TextPosition = new DataPoint(180, polarCondition.Type == PolarConditionType.MidnightSun ? 75 : -20),
            TextHorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            TextColor = GetPolarConditionColor(polarCondition.Type),
            Background = OxyColors.White,
            Stroke = GetPolarConditionColor(polarCondition.Type),
            StrokeThickness = 2
        };
        plotModel.Annotations.Add(mainAnnotation);

        // Add duration information if available
        if (polarCondition.DaylightDuration.HasValue)
        {
            var durationText = polarCondition.Type == PolarConditionType.MidnightSun 
                ? "24 hours of daylight" 
                : polarCondition.Type == PolarConditionType.PolarNight 
                    ? "0 hours of daylight" 
                    : $"Daylight: {polarCondition.DaylightDuration.Value:hh\\:mm}";

            var durationAnnotation = new TextAnnotation
            {
                Text = durationText,
                TextPosition = new DataPoint(180, polarCondition.Type == PolarConditionType.MidnightSun ? 65 : -25),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 10,
                Background = OxyColors.LightGray
            };
            plotModel.Annotations.Add(durationAnnotation);
        }
    }
}