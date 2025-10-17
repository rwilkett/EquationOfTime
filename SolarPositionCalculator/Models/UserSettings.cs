using System.IO;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.Models;

/// <summary>
/// User settings and preferences for the application
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Default latitude for new calculations
    /// </summary>
    public double DefaultLatitude { get; set; } = 51.4769; // London, UK

    /// <summary>
    /// Default longitude for new calculations
    /// </summary>
    public double DefaultLongitude { get; set; } = -0.0005; // London, UK

    /// <summary>
    /// Preferred coordinate format for display
    /// </summary>
    public CoordinateFormat CoordinateFormat { get; set; } = CoordinateFormat.DecimalDegrees;

    /// <summary>
    /// Whether to start in real-time mode by default
    /// </summary>
    public bool StartInRealTimeMode { get; set; } = false;

    /// <summary>
    /// Default export directory for charts and data
    /// </summary>
    public string DefaultExportDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    /// <summary>
    /// Whether to show tooltips and help text
    /// </summary>
    public bool ShowTooltips { get; set; } = true;

    /// <summary>
    /// Whether to use high contrast mode for accessibility
    /// </summary>
    public bool UseHighContrastMode { get; set; } = false;

    /// <summary>
    /// Window width for persistence
    /// </summary>
    public double WindowWidth { get; set; } = 1200;

    /// <summary>
    /// Window height for persistence
    /// </summary>
    public double WindowHeight { get; set; } = 800;

    /// <summary>
    /// Window left position
    /// </summary>
    public double WindowLeft { get; set; } = 100;

    /// <summary>
    /// Window top position
    /// </summary>
    public double WindowTop { get; set; } = 100;

    /// <summary>
    /// Whether the window is maximized
    /// </summary>
    public bool IsWindowMaximized { get; set; } = false;

    /// <summary>
    /// Creates a copy of the current settings
    /// </summary>
    public UserSettings Clone()
    {
        return new UserSettings
        {
            DefaultLatitude = DefaultLatitude,
            DefaultLongitude = DefaultLongitude,
            CoordinateFormat = CoordinateFormat,
            StartInRealTimeMode = StartInRealTimeMode,
            DefaultExportDirectory = DefaultExportDirectory,
            ShowTooltips = ShowTooltips,
            UseHighContrastMode = UseHighContrastMode,
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            WindowLeft = WindowLeft,
            WindowTop = WindowTop,
            IsWindowMaximized = IsWindowMaximized
        };
    }

    /// <summary>
    /// Validates the settings and returns any issues
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate coordinates
        if (DefaultLatitude < -90 || DefaultLatitude > 90)
        {
            errors.Add($"Default latitude must be between -90° and +90°. Current: {DefaultLatitude}°");
        }

        if (DefaultLongitude < -180 || DefaultLongitude > 180)
        {
            errors.Add($"Default longitude must be between -180° and +180°. Current: {DefaultLongitude}°");
        }

        // Validate export directory
        if (!string.IsNullOrEmpty(DefaultExportDirectory) && !Directory.Exists(DefaultExportDirectory))
        {
            warnings.Add($"Default export directory does not exist: {DefaultExportDirectory}");
        }

        // Validate window dimensions
        if (WindowWidth < 400)
        {
            warnings.Add("Window width is very small and may cause display issues.");
        }

        if (WindowHeight < 300)
        {
            warnings.Add("Window height is very small and may cause display issues.");
        }

        if (errors.Count > 0)
        {
            return new ValidationResult(false, errors.ToArray(), warnings.ToArray());
        }

        return warnings.Count > 0 
            ? ValidationResult.Warning(warnings.ToArray())
            : ValidationResult.Success();
    }
}