using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SolarPositionCalculator.ViewModels;
using SolarPositionCalculator.Services;
using SolarPositionCalculator.Views;
using SolarPositionCalculator.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SolarPositionCalculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CompositeViewModel _compositeViewModel;
    private readonly ISettingsService _settingsService;

    public MainWindow(CompositeViewModel compositeViewModel, ISettingsService settingsService)
    {
        _compositeViewModel = compositeViewModel ?? throw new ArgumentNullException(nameof(compositeViewModel));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        InitializeComponent();
        InitializeViewModels();
        InitializeSettings();
    }

    /// <summary>
    /// Initializes the ViewModels and sets up the DataContext
    /// </summary>
    private void InitializeViewModels()
    {
        // Set the DataContext to the injected composite view model
        DataContext = _compositeViewModel;
    }

    /// <summary>
    /// Initializes settings and applies them to the window
    /// </summary>
    private async void InitializeSettings()
    {
        try
        {
            // Load settings
            await _settingsService.LoadSettingsAsync();
            
            // Apply window settings
            ApplyWindowSettings();
            
            // Apply default coordinates to MainViewModel
            ApplyDefaultCoordinates();
            
            // Apply accessibility settings
            ApplyAccessibilitySettings();
            
            // Subscribe to settings changes
            _settingsService.SettingsChanged += OnSettingsChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load settings: {ex.Message}", 
                          "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Applies window settings from user preferences
    /// </summary>
    private void ApplyWindowSettings()
    {
        var settings = _settingsService.Settings;
        
        // Apply window size and position
        if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
        {
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
        }

        if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
        {
            Left = settings.WindowLeft;
            Top = settings.WindowTop;
        }

        if (settings.IsWindowMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    /// <summary>
    /// Applies default coordinates from settings to the MainViewModel
    /// </summary>
    private void ApplyDefaultCoordinates()
    {
        var settings = _settingsService.Settings;
        
        if (_compositeViewModel.MainViewModel != null)
        {
            _compositeViewModel.MainViewModel.Latitude = settings.DefaultLatitude;
            _compositeViewModel.MainViewModel.Longitude = settings.DefaultLongitude;
            
            // Start in real-time mode if configured
            if (settings.StartInRealTimeMode)
            {
                _compositeViewModel.MainViewModel.IsRealTimeMode = true;
            }
        }
    }

    /// <summary>
    /// Applies accessibility settings
    /// </summary>
    private void ApplyAccessibilitySettings()
    {
        var settings = _settingsService.Settings;
        
        // Apply high contrast mode
        AccessibilityService.ApplyHighContrastTheme(settings.UseHighContrastMode);
        
        // Set up keyboard navigation
        AccessibilityService.SetupKeyboardNavigation(this);
        
        // Add keyboard shortcuts
        SetupKeyboardShortcuts();
        
        // Set accessibility properties
        SetupAccessibilityProperties();
    }

    /// <summary>
    /// Sets up keyboard shortcuts for the application
    /// </summary>
    private void SetupKeyboardShortcuts()
    {
        // Add input bindings for keyboard shortcuts
        var calculateBinding = new System.Windows.Input.KeyBinding(
            _compositeViewModel.MainViewModel?.CalculateSolarPositionCommand,
            System.Windows.Input.Key.F5,
            System.Windows.Input.ModifierKeys.None);
        InputBindings.Add(calculateBinding);

        var realTimeBinding = new System.Windows.Input.KeyBinding(
            _compositeViewModel.MainViewModel?.ToggleRealTimeModeCommand,
            System.Windows.Input.Key.R,
            System.Windows.Input.ModifierKeys.Control);
        InputBindings.Add(realTimeBinding);

        var exportDataBinding = new System.Windows.Input.KeyBinding(
            _compositeViewModel.ExportDataCommand,
            System.Windows.Input.Key.E,
            System.Windows.Input.ModifierKeys.Control);
        InputBindings.Add(exportDataBinding);

        var exportChartsBinding = new System.Windows.Input.KeyBinding(
            _compositeViewModel.ExportChartsCommand,
            System.Windows.Input.Key.E,
            System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift);
        InputBindings.Add(exportChartsBinding);
    }

    /// <summary>
    /// Sets up accessibility properties for UI elements
    /// </summary>
    private void SetupAccessibilityProperties()
    {
        // Set window accessibility properties
        AccessibilityService.SetAccessibilityProperties(this, 
            "Solar Position Calculator", 
            "Application for calculating solar positions and visualizing astronomical data");
    }

    /// <summary>
    /// Handles settings changes
    /// </summary>
    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        // Apply changes that affect the current session
        if (e.ChangedProperties.Contains(nameof(UserSettings.UseHighContrastMode)))
        {
            AccessibilityService.ApplyHighContrastTheme(_settingsService.Settings.UseHighContrastMode);
        }

        if (e.ChangedProperties.Contains(nameof(UserSettings.ShowTooltips)))
        {
            // Update tooltip visibility throughout the application
            UpdateTooltipVisibility(_settingsService.Settings.ShowTooltips);
        }
    }

    /// <summary>
    /// Updates tooltip visibility throughout the application
    /// </summary>
    private void UpdateTooltipVisibility(bool showTooltips)
    {
        // This is a simplified implementation
        // In a full implementation, you would traverse the visual tree
        // and update tooltip visibility for all controls
        ToolTipService.SetShowOnDisabled(this, showTooltips);
    }

    /// <summary>
    /// Handles the Exit menu item click
    /// </summary>
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Handles the Settings menu item click
    /// </summary>
    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsDialog = new Views.SettingsDialog(_settingsService.Settings);
            var result = settingsDialog.ShowDialog();
            
            if (result == true && settingsDialog.UpdatedSettings != null)
            {
                await _settingsService.UpdateSettingsAsync(settingsDialog.UpdatedSettings);
                MessageBox.Show("Settings have been saved successfully.", 
                              "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings: {ex.Message}", 
                          "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles the Keyboard Shortcuts menu item click
    /// </summary>
    private void KeyboardShortcuts_Click(object sender, RoutedEventArgs e)
    {
        AccessibilityService.ShowKeyboardShortcutsHelp();
    }

    /// <summary>
    /// Handles the About menu item click
    /// </summary>
    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Solar Position Calculator\n\n" +
            "A comprehensive tool for calculating solar positions and visualizing the equation of time.\n\n" +
            "Features:\n" +
            "• Solar position calculations (azimuth and elevation)\n" +
            "• Equation of time visualization\n" +
            "• Sun path diagrams\n" +
            "• Real-time tracking\n" +
            "• Data export capabilities\n" +
            "• Customizable settings and preferences\n" +
            "• Full keyboard navigation support\n" +
            "• High contrast accessibility mode\n\n" +
            "Built with .NET 8 and WPF\n\n" +
            "Press F1 for keyboard shortcuts",
            "About Solar Position Calculator",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Handles window closing to clean up resources and save settings
    /// </summary>
    protected override async void OnClosed(EventArgs e)
    {
        try
        {
            // Save current window state to settings
            await SaveWindowStateToSettings();
            
            // Unsubscribe from settings events
            _settingsService.SettingsChanged -= OnSettingsChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save window state: {ex.Message}");
        }

        if (DataContext is CompositeViewModel compositeViewModel)
        {
            compositeViewModel.Dispose();
        }
        base.OnClosed(e);
    }

    /// <summary>
    /// Saves the current window state to settings
    /// </summary>
    private async Task SaveWindowStateToSettings()
    {
        var currentSettings = _settingsService.Settings.Clone();
        
        // Update window state
        currentSettings.IsWindowMaximized = WindowState == WindowState.Maximized;
        
        if (WindowState == WindowState.Normal)
        {
            currentSettings.WindowWidth = Width;
            currentSettings.WindowHeight = Height;
            currentSettings.WindowLeft = Left;
            currentSettings.WindowTop = Top;
        }

        await _settingsService.UpdateSettingsAsync(currentSettings);
    }
}

