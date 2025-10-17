using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for CSV export functionality
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports solar position data to CSV format
    /// </summary>
    /// <param name="data">Solar position data to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    Task ExportSolarPositionsAsync(IEnumerable<SolarPosition> data, string filePath, CsvExportOptions? options = null);

    /// <summary>
    /// Exports equation of time data to CSV format
    /// </summary>
    /// <param name="data">Equation of time data to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    Task ExportEquationOfTimeAsync(IEnumerable<EquationOfTimeData> data, string filePath, CsvExportOptions? options = null);

    /// <summary>
    /// Generates solar position data for a date range with specified intervals
    /// </summary>
    /// <param name="location">Geographic location</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="interval">Time interval between calculations</param>
    /// <returns>Solar position data for the specified range</returns>
    Task<IEnumerable<SolarPosition>> GenerateDateRangeDataAsync(
        GeographicCoordinate location, 
        DateTime startDate, 
        DateTime endDate, 
        TimeSpan interval);

    /// <summary>
    /// Exports solar position data for a date range directly to CSV
    /// </summary>
    /// <param name="location">Geographic location</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="interval">Time interval between calculations</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <param name="progress">Progress reporting callback</param>
    Task ExportDateRangeAsync(
        GeographicCoordinate location,
        DateTime startDate,
        DateTime endDate,
        TimeSpan interval,
        string filePath,
        CsvExportOptions? options = null,
        IProgress<ExportProgress>? progress = null);
}