namespace SolarPositionCalculator.Models;

/// <summary>
/// Represents equation of time data for a specific date
/// </summary>
public record EquationOfTimeData(DateTime Date, double Minutes)
{
    /// <summary>
    /// Returns a string representation of the equation of time data
    /// </summary>
    public override string ToString()
    {
        return $"{Date:MMM dd}: {Minutes:F2} min";
    }
}