using System.IO;
using System.Windows;
using Microsoft.Win32;
using SolarPositionCalculator.Models;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Interaction logic for SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    private readonly UserSettings _originalSettings;
    private UserSettings _currentSettings;

    /// <summary>
    /// Gets the updated settings if the dialog was accepted
    /// </summary>
    public UserSettings? UpdatedSettings { get; private set; }

    public SettingsDialog(UserSettings currentSettings)
    {
        InitializeComponent();
        
        _originalSettings = currentSettings ?? throw new ArgumentNullException(nameof(currentSettings));
        _currentSettings = _originalSettings.Clone();
        
        LoadSettingsIntoControls();
    }

    /// <summary>
    /// Loads the current settings into the dialog controls
    /// </summary>
    private void LoadSettingsIntoControls()
    {
        // Default location
        LatitudeTextBox.Text = _currentSettings.DefaultLatitude.ToString("F6");
        LongitudeTextBox.Text = _currentSettings.DefaultLongitude.ToString("F6");
        
        // Coordinate format
        foreach (var item in CoordinateFormatComboBox.Items.Cast<System.Windows.Controls.ComboBoxItem>())
        {
            if (item.Tag?.ToString() == _currentSettings.CoordinateFormat.ToString())
            {
                CoordinateFormatComboBox.SelectedItem = item;
                break;
            }
        }

        // Application behavior
        StartInRealTimeModeCheckBox.IsChecked = _currentSettings.StartInRealTimeMode;
        ShowTooltipsCheckBox.IsChecked = _currentSettings.ShowTooltips;

        // Export settings
        DefaultExportDirectoryTextBox.Text = _currentSettings.DefaultExportDirectory;

        // Accessibility
        UseHighContrastModeCheckBox.IsChecked = _currentSettings.UseHighContrastMode;

        // Window settings
        WindowWidthTextBox.Text = _currentSettings.WindowWidth.ToString("F0");
        WindowHeightTextBox.Text = _currentSettings.WindowHeight.ToString("F0");
        StartMaximizedCheckBox.IsChecked = _currentSettings.IsWindowMaximized;
    }

    /// <summary>
    /// Saves the control values back to the settings object
    /// </summary>
    private bool SaveControlsToSettings()
    {
        try
        {
            // Validate and parse latitude
            if (!double.TryParse(LatitudeTextBox.Text, out double latitude) || latitude < -90 || latitude > 90)
            {
                MessageBox.Show("Latitude must be a number between -90 and +90 degrees.", 
                              "Invalid Latitude", MessageBoxButton.OK, MessageBoxImage.Warning);
                LatitudeTextBox.Focus();
                return false;
            }

            // Validate and parse longitude
            if (!double.TryParse(LongitudeTextBox.Text, out double longitude) || longitude < -180 || longitude > 180)
            {
                MessageBox.Show("Longitude must be a number between -180 and +180 degrees.", 
                              "Invalid Longitude", MessageBoxButton.OK, MessageBoxImage.Warning);
                LongitudeTextBox.Focus();
                return false;
            }

            // Validate window dimensions
            if (!double.TryParse(WindowWidthTextBox.Text, out double width) || width < 400)
            {
                MessageBox.Show("Window width must be at least 400 pixels.", 
                              "Invalid Window Width", MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowWidthTextBox.Focus();
                return false;
            }

            if (!double.TryParse(WindowHeightTextBox.Text, out double height) || height < 300)
            {
                MessageBox.Show("Window height must be at least 300 pixels.", 
                              "Invalid Window Height", MessageBoxButton.OK, MessageBoxImage.Warning);
                WindowHeightTextBox.Focus();
                return false;
            }

            // Validate export directory
            var exportDirectory = DefaultExportDirectoryTextBox.Text;
            if (!string.IsNullOrEmpty(exportDirectory) && !Directory.Exists(exportDirectory))
            {
                var result = MessageBox.Show(
                    $"The export directory '{exportDirectory}' does not exist. Do you want to create it?",
                    "Directory Not Found", 
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(exportDirectory);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create directory: {ex.Message}", 
                                      "Directory Creation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
                // If No, continue with the invalid directory (user's choice)
            }

            // Update settings object
            _currentSettings.DefaultLatitude = latitude;
            _currentSettings.DefaultLongitude = longitude;
            
            if (CoordinateFormatComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedFormat)
            {
                if (Enum.TryParse<CoordinateFormat>(selectedFormat.Tag?.ToString(), out var format))
                {
                    _currentSettings.CoordinateFormat = format;
                }
            }

            _currentSettings.StartInRealTimeMode = StartInRealTimeModeCheckBox.IsChecked ?? false;
            _currentSettings.ShowTooltips = ShowTooltipsCheckBox.IsChecked ?? true;
            _currentSettings.DefaultExportDirectory = exportDirectory;
            _currentSettings.UseHighContrastMode = UseHighContrastModeCheckBox.IsChecked ?? false;
            _currentSettings.WindowWidth = width;
            _currentSettings.WindowHeight = height;
            _currentSettings.IsWindowMaximized = StartMaximizedCheckBox.IsChecked ?? false;

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", 
                          "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void BrowseExportDirectory_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Select Default Export Directory",
            Multiselect = false
        };

        if (!string.IsNullOrEmpty(DefaultExportDirectoryTextBox.Text))
        {
            folderDialog.InitialDirectory = DefaultExportDirectoryTextBox.Text;
        }

        if (folderDialog.ShowDialog() == true)
        {
            DefaultExportDirectoryTextBox.Text = folderDialog.FolderName;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (SaveControlsToSettings())
        {
            UpdatedSettings = _currentSettings;
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        UpdatedSettings = null;
        DialogResult = false;
        Close();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to their default values?",
            "Reset Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _currentSettings = new UserSettings();
            LoadSettingsIntoControls();
        }
    }
}