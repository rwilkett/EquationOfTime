using System.Globalization;
using System.Text;
using System.IO;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Service for exporting solar position data to CSV format
/// </summary>
public class CsvExportService : ICsvExportService
{
    private readonly IAstronomicalCalculator _astronomicalCalculator;

    public CsvExportService(IAstronomicalCalculator astronomicalCalculator)
    {
        _astronomicalCalculator = astronomicalCalculator ?? throw new ArgumentNullException(nameof(astronomicalCalculator));
    }

    /// <inheritdoc />
    public async Task ExportSolarPositionsAsync(IEnumerable<SolarPosition> data, string filePath, CsvExportOptions? options = null)
    {
        options ??= new CsvExportOptions();
        
        var csv = new StringBuilder();
        
        // Add metadata comments if requested
        if (options.IncludeMetadataComments)
        {
            AddMetadataComments(csv, options.Metadata);
        }

        // Add headers if requested
        if (options.IncludeHeaders)
        {
            AddSolarPositionHeaders(csv, options);
        }

        // Add data rows
        foreach (var position in data)
        {
            AddSolarPositionRow(csv, position, options);
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());
    }

    /// <inheritdoc />
    public async Task ExportEquationOfTimeAsync(IEnumerable<EquationOfTimeData> data, string filePath, CsvExportOptions? options = null)
    {
        options ??= new CsvExportOptions();
        
        var csv = new StringBuilder();
        
        // Add metadata comments if requested
        if (options.IncludeMetadataComments)
        {
            AddMetadataComments(csv, options.Metadata);
        }

        // Add headers if requested
        if (options.IncludeHeaders)
        {
            AddEquationOfTimeHeaders(csv, options);
        }

        // Add data rows
        foreach (var eotData in data)
        {
            AddEquationOfTimeRow(csv, eotData, options);
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SolarPosition>> GenerateDateRangeDataAsync(
        GeographicCoordinate location, 
        DateTime startDate, 
        DateTime endDate, 
        TimeSpan interval)
    {
        if (!location.IsValid)
            throw new ArgumentException("Invalid geographic coordinates", nameof(location));
        
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");
        
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive", nameof(interval));

        var positions = new List<SolarPosition>();
        var currentDate = startDate;

        await Task.Run(() =>
        {
            while (currentDate <= endDate)
            {
                var position = _astronomicalCalculator.CalculateSolarPosition(location, currentDate);
                positions.Add(position);
                currentDate = currentDate.Add(interval);
            }
        });

        return positions;
    }

    /// <inheritdoc />
    public async Task ExportDateRangeAsync(
        GeographicCoordinate location,
        DateTime startDate,
        DateTime endDate,
        TimeSpan interval,
        string filePath,
        CsvExportOptions? options = null,
        IProgress<ExportProgress>? progress = null)
    {
        options ??= new CsvExportOptions();
        
        if (!location.IsValid)
            throw new ArgumentException("Invalid geographic coordinates", nameof(location));
        
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");
        
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive", nameof(interval));

        var totalItems = (int)Math.Ceiling((endDate - startDate).TotalMilliseconds / interval.TotalMilliseconds);
        var processedItems = 0;
        var startTime = DateTime.Now;

        var csv = new StringBuilder();
        
        // Add metadata comments
        if (options.IncludeMetadataComments)
        {
            var metadata = new Dictionary<string, string>(options.Metadata)
            {
                ["Location"] = location.ToString(),
                ["StartDate"] = startDate.ToString(options.DateTimeFormat),
                ["EndDate"] = endDate.ToString(options.DateTimeFormat),
                ["Interval"] = FormatInterval(interval),
                ["TotalDataPoints"] = totalItems.ToString(),
                ["GeneratedOn"] = DateTime.Now.ToString(options.DateTimeFormat)
            };
            AddMetadataComments(csv, metadata);
        }

        // Add headers
        if (options.IncludeHeaders)
        {
            AddSolarPositionHeaders(csv, options);
        }

        // Generate and write data
        await Task.Run(() =>
        {
            var currentDate = startDate;
            
            while (currentDate <= endDate)
            {
                var position = _astronomicalCalculator.CalculateSolarPosition(location, currentDate);
                
                // Add equation of time if requested
                if (options.IncludeEquationOfTime)
                {
                    var eot = _astronomicalCalculator.CalculateEquationOfTime(currentDate);
                    AddSolarPositionRowWithEot(csv, position, eot, options);
                }
                else
                {
                    AddSolarPositionRow(csv, position, options);
                }

                processedItems++;
                currentDate = currentDate.Add(interval);

                // Report progress
                if (progress != null && processedItems % 100 == 0) // Report every 100 items
                {
                    var elapsed = DateTime.Now - startTime;
                    var progressInfo = new ExportProgress(processedItems, totalItems, "Calculating solar positions", elapsed);
                    progress.Report(progressInfo);
                }
            }
        });

        // Final progress report
        if (progress != null)
        {
            var elapsed = DateTime.Now - startTime;
            var finalProgress = new ExportProgress(processedItems, totalItems, "Writing to file", elapsed);
            progress.Report(finalProgress);
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());

        // Complete progress report
        if (progress != null)
        {
            var elapsed = DateTime.Now - startTime;
            var completeProgress = new ExportProgress(totalItems, totalItems, "Export complete", elapsed);
            progress.Report(completeProgress);
        }
    }

    private static void AddMetadataComments(StringBuilder csv, Dictionary<string, string> metadata)
    {
        csv.AppendLine("# Solar Position Calculator Export");
        csv.AppendLine($"# Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        foreach (var kvp in metadata)
        {
            csv.AppendLine($"# {kvp.Key}: {kvp.Value}");
        }
        
        csv.AppendLine("#");
    }

    private static void AddSolarPositionHeaders(StringBuilder csv, CsvExportOptions options)
    {
        var headers = new List<string>();

        if (options.CustomColumnOrder != null)
        {
            headers.AddRange(options.CustomColumnOrder);
        }
        else
        {
            headers.Add("Timestamp");
            headers.Add("Azimuth");
            headers.Add("Elevation");
            
            if (options.IncludeLocationInRows)
            {
                headers.Add("Latitude");
                headers.Add("Longitude");
            }
            
            if (options.IncludeEquationOfTime)
            {
                headers.Add("EquationOfTime");
            }
            
            if (options.IncludeSunVisibility)
            {
                headers.Add("SunVisible");
            }
        }

        csv.AppendLine(string.Join(options.Delimiter, headers));
    }

    private static void AddEquationOfTimeHeaders(StringBuilder csv, CsvExportOptions options)
    {
        var headers = new List<string> { "Date", "EquationOfTime" };
        csv.AppendLine(string.Join(options.Delimiter, headers));
    }

    private static void AddSolarPositionRow(StringBuilder csv, SolarPosition position, CsvExportOptions options)
    {
        var values = new List<string>();

        if (options.CustomColumnOrder != null)
        {
            foreach (var column in options.CustomColumnOrder)
            {
                values.Add(GetColumnValue(column, position, null, options));
            }
        }
        else
        {
            values.Add(position.Timestamp.ToString(options.DateTimeFormat));
            values.Add(position.Azimuth.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            values.Add(position.Elevation.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            
            if (options.IncludeLocationInRows)
            {
                values.Add(position.Location.Latitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
                values.Add(position.Location.Longitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            }
            
            if (options.IncludeSunVisibility)
            {
                values.Add(position.IsSunVisible.ToString());
            }
        }

        csv.AppendLine(string.Join(options.Delimiter, values));
    }

    private static void AddSolarPositionRowWithEot(StringBuilder csv, SolarPosition position, double equationOfTime, CsvExportOptions options)
    {
        var values = new List<string>();

        if (options.CustomColumnOrder != null)
        {
            foreach (var column in options.CustomColumnOrder)
            {
                values.Add(GetColumnValue(column, position, equationOfTime, options));
            }
        }
        else
        {
            values.Add(position.Timestamp.ToString(options.DateTimeFormat));
            values.Add(position.Azimuth.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            values.Add(position.Elevation.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            
            if (options.IncludeLocationInRows)
            {
                values.Add(position.Location.Latitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
                values.Add(position.Location.Longitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            }
            
            if (options.IncludeEquationOfTime)
            {
                values.Add(equationOfTime.ToString(options.NumberFormat, CultureInfo.InvariantCulture));
            }
            
            if (options.IncludeSunVisibility)
            {
                values.Add(position.IsSunVisible.ToString());
            }
        }

        csv.AppendLine(string.Join(options.Delimiter, values));
    }

    private static void AddEquationOfTimeRow(StringBuilder csv, EquationOfTimeData eotData, CsvExportOptions options)
    {
        var values = new List<string>
        {
            eotData.Date.ToString(options.DateTimeFormat),
            eotData.Minutes.ToString(options.NumberFormat, CultureInfo.InvariantCulture)
        };

        csv.AppendLine(string.Join(options.Delimiter, values));
    }

    private static string GetColumnValue(string columnName, SolarPosition position, double? equationOfTime, CsvExportOptions options)
    {
        return columnName.ToLowerInvariant() switch
        {
            "timestamp" => position.Timestamp.ToString(options.DateTimeFormat),
            "azimuth" => position.Azimuth.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            "elevation" => position.Elevation.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            "latitude" => position.Location.Latitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            "longitude" => position.Location.Longitude.ToString(options.NumberFormat, CultureInfo.InvariantCulture),
            "equationoftime" => equationOfTime?.ToString(options.NumberFormat, CultureInfo.InvariantCulture) ?? "N/A",
            "sunvisible" => position.IsSunVisible.ToString(),
            _ => string.Empty
        };
    }

    private static string FormatInterval(TimeSpan interval)
    {
        if (interval.TotalDays >= 1)
            return $"{interval.TotalDays:F1} days";
        if (interval.TotalHours >= 1)
            return $"{interval.TotalHours:F1} hours";
        if (interval.TotalMinutes >= 1)
            return $"{interval.TotalMinutes:F1} minutes";
        return $"{interval.TotalSeconds:F1} seconds";
    }
}