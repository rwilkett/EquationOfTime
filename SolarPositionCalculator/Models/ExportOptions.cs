using OxyPlot;

namespace SolarPositionCalculator.Models;

/// <summary>
/// Supported export formats for charts
/// </summary>
public enum ExportFormat
{
    Png,
    Svg,
    Pdf
}

/// <summary>
/// Options for chart export functionality
/// </summary>
public record ExportOptions
{
    /// <summary>
    /// Export format (PNG, SVG)
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Png;

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int Width { get; init; } = 800;

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int Height { get; init; } = 600;

    /// <summary>
    /// DPI resolution for PNG exports
    /// </summary>
    public int Resolution { get; init; } = 96;

    /// <summary>
    /// Quality setting for PNG exports (1-100)
    /// </summary>
    public int Quality { get; init; } = 90;

    /// <summary>
    /// Background color for exports
    /// </summary>
    public string BackgroundColor { get; init; } = "White";

    /// <summary>
    /// Whether to include chart title in export
    /// </summary>
    public bool IncludeTitle { get; init; } = true;

    /// <summary>
    /// Whether to include legend in export
    /// </summary>
    public bool IncludeLegend { get; init; } = true;
}

/// <summary>
/// Batch export configuration for multiple charts
/// </summary>
public record BatchExportOptions
{
    /// <summary>
    /// Base export options to apply to all charts
    /// </summary>
    public ExportOptions ExportOptions { get; init; } = new();

    /// <summary>
    /// Output directory for batch export
    /// </summary>
    public string OutputDirectory { get; init; } = string.Empty;

    /// <summary>
    /// File name prefix for exported files
    /// </summary>
    public string FileNamePrefix { get; init; } = "chart";

    /// <summary>
    /// Whether to include timestamp in file names
    /// </summary>
    public bool IncludeTimestamp { get; init; } = true;

    /// <summary>
    /// Whether to create subdirectories by chart type
    /// </summary>
    public bool GroupByChartType { get; init; } = false;
}

/// <summary>
/// Chart information for batch export
/// </summary>
public record ChartExportInfo
{
    /// <summary>
    /// The chart plot model to export
    /// </summary>
    public required PlotModel Chart { get; init; }

    /// <summary>
    /// Chart type identifier
    /// </summary>
    public required string ChartType { get; init; }

    /// <summary>
    /// Custom file name (without extension)
    /// </summary>
    public string? CustomFileName { get; init; }

    /// <summary>
    /// Additional metadata for the chart
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}