namespace SolarPositionCalculator.Services;

/// <summary>
/// Event arguments for chart interaction events
/// </summary>
public class ChartInteractionEventArgs : EventArgs
{
    public DateTime SelectedDate { get; }
    public double? SelectedValue { get; }
    public string? ChartType { get; }

    public ChartInteractionEventArgs(DateTime selectedDate, double? selectedValue = null, string? chartType = null)
    {
        SelectedDate = selectedDate;
        SelectedValue = selectedValue;
        ChartType = chartType;
    }
}

/// <summary>
/// Event arguments for sun path interaction events
/// </summary>
public class SunPathInteractionEventArgs : EventArgs
{
    public double Azimuth { get; }
    public double Elevation { get; }
    public DateTime? TimeOfDay { get; }

    public SunPathInteractionEventArgs(double azimuth, double elevation, DateTime? timeOfDay = null)
    {
        Azimuth = azimuth;
        Elevation = elevation;
        TimeOfDay = timeOfDay;
    }
}