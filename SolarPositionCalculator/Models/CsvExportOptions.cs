namespace SolarPositionCalculator.Models;

/// <summary>
/// Options for CSV export functionality
/// </summary>
public record CsvExportOptions
{
    /// <summary>
    /// Whether to include column headers in the CSV file
    /// </summary>
    public bool IncludeHeaders { get; init; } = true;

    /// <summary>
    /// CSV delimiter character
    /// </summary>
    public string Delimiter { get; init; } = ",";

    /// <summary>
    /// Date/time format for timestamp columns
    /// </summary>
    public string DateTimeFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Number format for decimal values
    /// </summary>
    public string NumberFormat { get; init; } = "F6";

    /// <summary>
    /// Whether to include location coordinates in each row
    /// </summary>
    public bool IncludeLocationInRows { get; init; } = true;

    /// <summary>
    /// Whether to include equation of time values (if available)
    /// </summary>
    public bool IncludeEquationOfTime { get; init; } = false;

    /// <summary>
    /// Whether to include sun visibility status
    /// </summary>
    public bool IncludeSunVisibility { get; init; } = true;

    /// <summary>
    /// Custom column order (if null, uses default order)
    /// </summary>
    public string[]? CustomColumnOrder { get; init; }

    /// <summary>
    /// Additional metadata to include in header comments
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Whether to include metadata as comments at the top of the file
    /// </summary>
    public bool IncludeMetadataComments { get; init; } = true;
}

/// <summary>
/// Progress information for export operations
/// </summary>
public record ExportProgress(
    int ProcessedItems,
    int TotalItems,
    string CurrentOperation,
    TimeSpan ElapsedTime)
{
    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double PercentComplete => TotalItems > 0 ? (double)ProcessedItems / TotalItems * 100 : 0;

    /// <summary>
    /// Estimated time remaining
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (ProcessedItems <= 0 || ElapsedTime.TotalSeconds <= 0)
                return null;

            var itemsPerSecond = ProcessedItems / ElapsedTime.TotalSeconds;
            var remainingItems = TotalItems - ProcessedItems;
            return TimeSpan.FromSeconds(remainingItems / itemsPerSecond);
        }
    }
}

/// <summary>
/// Date range export configuration
/// </summary>
public record DateRangeExportConfig
{
    /// <summary>
    /// Geographic location for calculations
    /// </summary>
    public required GeographicCoordinate Location { get; init; }

    /// <summary>
    /// Start date for the range
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// End date for the range
    /// </summary>
    public required DateTime EndDate { get; init; }

    /// <summary>
    /// Time interval between calculations
    /// </summary>
    public required TimeSpan Interval { get; init; }

    /// <summary>
    /// Output file path
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// CSV export options
    /// </summary>
    public CsvExportOptions ExportOptions { get; init; } = new();

    /// <summary>
    /// Validates the configuration
    /// </summary>
    public bool IsValid => 
        Location.IsValid && 
        StartDate < EndDate && 
        Interval > TimeSpan.Zero &&
        !string.IsNullOrWhiteSpace(FilePath);

    /// <summary>
    /// Calculates the estimated number of data points
    /// </summary>
    public int EstimatedDataPoints => 
        (int)Math.Ceiling((EndDate - StartDate).TotalMilliseconds / Interval.TotalMilliseconds);
}