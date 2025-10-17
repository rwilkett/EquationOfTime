using System.IO;
using System.Text.Json;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Implementation of settings service using JSON file storage
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private UserSettings _settings;

    /// <summary>
    /// Gets the current user settings
    /// </summary>
    public UserSettings Settings => _settings;

    /// <summary>
    /// Event fired when settings are changed
    /// </summary>
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public SettingsService()
    {
        _settingsFilePath = GetSettingsFilePath();
        _settings = new UserSettings();
    }

    /// <summary>
    /// Loads settings from persistent storage
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<UserSettings>(json);
                
                if (loadedSettings != null)
                {
                    var validation = loadedSettings.Validate();
                    if (validation.IsValid)
                    {
                        _settings = loadedSettings;
                    }
                    else
                    {
                        // If settings are invalid, use defaults but log the issue
                        System.Diagnostics.Debug.WriteLine($"Invalid settings loaded, using defaults: {validation.PrimaryError}");
                        _settings = new UserSettings();
                    }
                }
            }
            else
            {
                // First run - create default settings file
                _settings = new UserSettings();
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            // If loading fails, use default settings
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            _settings = new UserSettings();
        }
    }

    /// <summary>
    /// Saves current settings to persistent storage
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        try
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(_settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            throw new InvalidOperationException($"Unable to save settings: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates settings with new values
    /// </summary>
    public async Task UpdateSettingsAsync(UserSettings newSettings)
    {
        if (newSettings == null)
            throw new ArgumentNullException(nameof(newSettings));

        var validation = newSettings.Validate();
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Invalid settings: {validation.PrimaryError}");
        }

        var oldSettings = _settings.Clone();
        var changedProperties = GetChangedProperties(oldSettings, newSettings);

        _settings = newSettings.Clone();
        
        await SaveSettingsAsync();

        // Fire the settings changed event
        if (changedProperties.Length > 0)
        {
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(oldSettings, _settings, changedProperties));
        }
    }

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    public async Task ResetToDefaultsAsync()
    {
        var oldSettings = _settings.Clone();
        _settings = new UserSettings();
        
        await SaveSettingsAsync();

        var allProperties = typeof(UserSettings).GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p => p.Name)
            .ToArray();

        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(oldSettings, _settings, allProperties));
    }

    /// <summary>
    /// Gets the settings file path
    /// </summary>
    public string GetSettingsFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "SolarPositionCalculator");
        return Path.Combine(appFolder, "settings.json");
    }

    /// <summary>
    /// Validates current settings
    /// </summary>
    public ValidationResult ValidateSettings()
    {
        return _settings.Validate();
    }

    /// <summary>
    /// Gets the list of properties that changed between two settings objects
    /// </summary>
    private string[] GetChangedProperties(UserSettings oldSettings, UserSettings newSettings)
    {
        var changedProperties = new List<string>();
        var properties = typeof(UserSettings).GetProperties().Where(p => p.CanRead);

        foreach (var property in properties)
        {
            var oldValue = property.GetValue(oldSettings);
            var newValue = property.GetValue(newSettings);

            if (!Equals(oldValue, newValue))
            {
                changedProperties.Add(property.Name);
            }
        }

        return changedProperties.ToArray();
    }
}