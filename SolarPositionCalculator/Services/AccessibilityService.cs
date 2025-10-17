using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Service for managing accessibility features and UI enhancements
/// </summary>
public class AccessibilityService
{
    /// <summary>
    /// Applies high contrast theme to the application
    /// </summary>
    /// <param name="useHighContrast">Whether to use high contrast mode</param>
    public static void ApplyHighContrastTheme(bool useHighContrast)
    {
        var app = Application.Current;
        if (app == null) return;

        if (useHighContrast)
        {
            // Apply high contrast colors
            app.Resources["BackgroundBrush"] = new SolidColorBrush(Colors.Black);
            app.Resources["ForegroundBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["AccentBrush"] = new SolidColorBrush(Colors.Yellow);
            app.Resources["BorderBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["DisabledBrush"] = new SolidColorBrush(Colors.Gray);
        }
        else
        {
            // Apply normal theme colors
            app.Resources["BackgroundBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["ForegroundBrush"] = new SolidColorBrush(Colors.Black);
            app.Resources["AccentBrush"] = new SolidColorBrush(Colors.Blue);
            app.Resources["BorderBrush"] = new SolidColorBrush(Colors.Gray);
            app.Resources["DisabledBrush"] = new SolidColorBrush(Colors.LightGray);
        }
    }

    /// <summary>
    /// Sets up keyboard navigation for a window
    /// </summary>
    /// <param name="window">Window to configure</param>
    public static void SetupKeyboardNavigation(Window window)
    {
        if (window == null) return;

        // Enable keyboard navigation
        window.KeyDown += (sender, e) =>
        {
            // F1 for help
            if (e.Key == System.Windows.Input.Key.F1)
            {
                ShowKeyboardShortcutsHelp();
                e.Handled = true;
            }
            
            // Ctrl+, for settings
            if (e.Key == System.Windows.Input.Key.OemComma && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            {
                // Trigger settings dialog - this would need to be handled by the main window
                e.Handled = true;
            }
        };
    }

    /// <summary>
    /// Shows keyboard shortcuts help dialog
    /// </summary>
    public static void ShowKeyboardShortcutsHelp()
    {
        var helpText = "Keyboard Shortcuts:\n\n" +
                      "F1 - Show this help\n" +
                      "Ctrl+, - Open Settings\n" +
                      "Ctrl+E - Export Data\n" +
                      "Ctrl+Shift+E - Export Charts\n" +
                      "F5 - Calculate Solar Position\n" +
                      "Ctrl+R - Toggle Real-time Mode\n" +
                      "Tab - Navigate between controls\n" +
                      "Shift+Tab - Navigate backwards\n" +
                      "Enter - Activate focused button\n" +
                      "Space - Toggle checkboxes\n" +
                      "Alt+F4 - Exit application";

        MessageBox.Show(helpText, "Keyboard Shortcuts", 
                       MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Adds accessibility properties to a UI element
    /// </summary>
    /// <param name="element">UI element to enhance</param>
    /// <param name="name">Accessible name</param>
    /// <param name="description">Accessible description</param>
    public static void SetAccessibilityProperties(FrameworkElement element, string name, string? description = null)
    {
        if (element == null) return;

        System.Windows.Automation.AutomationProperties.SetName(element, name);
        
        if (!string.IsNullOrEmpty(description))
        {
            System.Windows.Automation.AutomationProperties.SetHelpText(element, description);
        }
    }

    /// <summary>
    /// Checks if the system is using high contrast mode
    /// </summary>
    /// <returns>True if system high contrast is enabled</returns>
    public static bool IsSystemHighContrastEnabled()
    {
        return SystemParameters.HighContrast;
    }

    /// <summary>
    /// Gets the system's preferred font size scaling
    /// </summary>
    /// <returns>Font size scaling factor</returns>
    public static double GetSystemFontSizeScaling()
    {
        // This is a simplified implementation
        // In a real application, you might want to check system DPI settings
        return 1.0;
    }

    /// <summary>
    /// Applies focus visual styles for better keyboard navigation visibility
    /// </summary>
    /// <param name="element">Element to apply focus styles to</param>
    public static void ApplyFocusVisualStyle(FrameworkElement element)
    {
        if (element == null) return;

        // Create a focus visual style with high contrast border
        var focusVisual = new Style();
        focusVisual.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(2)));
        
        var focusVisualTemplate = new ControlTemplate();
        var border = new FrameworkElementFactory(typeof(System.Windows.Controls.Border));
        border.SetValue(System.Windows.Controls.Border.BorderBrushProperty, 
                       new SolidColorBrush(Colors.Black));
        border.SetValue(System.Windows.Controls.Border.BorderThicknessProperty, 
                       new Thickness(2));
        border.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, 
                       new CornerRadius(2));
        
        focusVisualTemplate.VisualTree = border;
        focusVisual.Setters.Add(new Setter(System.Windows.Controls.Control.TemplateProperty, focusVisualTemplate));
        
        element.FocusVisualStyle = focusVisual;
    }
}