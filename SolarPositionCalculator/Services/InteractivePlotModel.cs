using OxyPlot;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Wrapper for PlotModel with additional interaction data
/// </summary>
public class InteractivePlotModel
{
    public PlotModel PlotModel { get; }
    public string ChartType { get; }
    public SolarPosition[]? VisiblePositions { get; }

    public InteractivePlotModel(PlotModel plotModel, string chartType, SolarPosition[]? visiblePositions = null)
    {
        PlotModel = plotModel;
        ChartType = chartType;
        VisiblePositions = visiblePositions;
    }
}