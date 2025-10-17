using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Interface for application settings management
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current user settings
    /// </summary>
    UserSettings Settings { get; }

    /// <summary>
    /// Event fired when settings are changed
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Loads settings from persistent storage
    /// </summary>
    /// <returns>Task that completes when settings are loaded</returns>
    Task LoadSettingsAsync();

    /// <summary>
    /// Saves current settings to persistent storage
    /// </summary>
    /// <returns>Task that completes when settings are saved</returns>
    Task SaveSettingsAsync();

    /// <summary>
    /// Updates settings with new values
    /// </summary>
    /// <param name="newSettings">New settings to apply</param>
    /// <returns>Task that completes when settings are updated</returns>
    Task UpdateSettingsAsync(UserSettings newSettings);

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    /// <returns>Task that completes when settings are reset</returns>
    Task ResetToDefaultsAsync();

    /// <summary>
    /// Gets the settings file path
    /// </summary>
    string GetSettingsFilePath();

    /// <summary>
    /// Validates current settings
    /// </summary>
    /// <returns>Validation result</returns>
    ValidationResult ValidateSettings();
}

/// <summary>
/// Event arguments for settings changed events
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    public UserSettings OldSettings { get; }
    public UserSettings NewSettings { get; }
    public string[] ChangedProperties { get; }

    public SettingsChangedEventArgs(UserSettings oldSettings, UserSettings newSettings, string[] changedProperties)
    {
        OldSettings = oldSettings;
        NewSettings = newSettings;
        ChangedProperties = changedProperties;
    }
}