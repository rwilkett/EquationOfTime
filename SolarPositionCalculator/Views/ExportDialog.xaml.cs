using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Export dialog for chart export functionality
/// </summary>
public partial class ExportDialog : Window
{
    /// <summary>
    /// Gets the export options configured by the user
    /// </summary>
    public ExportOptions? ExportOptions { get; private set; }

    /// <summary>
    /// Gets the selected file path for export
    /// </summary>
    public string? SelectedFilePath { get; private set; }

    public ExportDialog()
    {
        InitializeComponent();
        UpdatePreview();
        
        // Wire up event handlers for real-time preview updates
        PngRadioButton.Checked += (s, e) => UpdatePreview();
        SvgRadioButton.Checked += (s, e) => UpdatePreview();
        WidthTextBox.TextChanged += (s, e) => UpdatePreview();
        HeightTextBox.TextChanged += (s, e) => UpdatePreview();
        ResolutionTextBox.TextChanged += (s, e) => UpdatePreview();
        QualityTextBox.TextChanged += (s, e) => UpdatePreview();
        IncludeTitleCheckBox.Checked += (s, e) => UpdatePreview();
        IncludeTitleCheckBox.Unchecked += (s, e) => UpdatePreview();
        IncludeLegendCheckBox.Checked += (s, e) => UpdatePreview();
        IncludeLegendCheckBox.Unchecked += (s, e) => UpdatePreview();
        WhiteBackgroundRadio.Checked += (s, e) => UpdatePreview();
        TransparentBackgroundRadio.Checked += (s, e) => UpdatePreview();
        CustomBackgroundRadio.Checked += (s, e) => { CustomBackgroundTextBox.IsEnabled = true; UpdatePreview(); };
        CustomBackgroundTextBox.TextChanged += (s, e) => UpdatePreview();
        
        // Handle custom background radio selection
        WhiteBackgroundRadio.Checked += (s, e) => CustomBackgroundTextBox.IsEnabled = false;
        TransparentBackgroundRadio.Checked += (s, e) => CustomBackgroundTextBox.IsEnabled = false;
    }

    private void SetStandardSize_Click(object sender, RoutedEventArgs e)
    {
        WidthTextBox.Text = "800";
        HeightTextBox.Text = "600";
    }

    private void SetLargeSize_Click(object sender, RoutedEventArgs e)
    {
        WidthTextBox.Text = "1200";
        HeightTextBox.Text = "900";
    }

    private void SetHDSize_Click(object sender, RoutedEventArgs e)
    {
        WidthTextBox.Text = "1920";
        HeightTextBox.Text = "1080";
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog();
        
        if (PngRadioButton.IsChecked == true)
        {
            saveDialog.Filter = "PNG Images (*.png)|*.png";
            saveDialog.DefaultExt = ".png";
        }
        else if (SvgRadioButton.IsChecked == true)
        {
            saveDialog.Filter = "SVG Images (*.svg)|*.svg";
            saveDialog.DefaultExt = ".svg";
        }

        saveDialog.Title = "Save Chart Export";
        saveDialog.FileName = "chart_export";

        if (saveDialog.ShowDialog() == true)
        {
            FilePathTextBox.Text = saveDialog.FileName;
            UpdatePreview();
        }
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateInput())
        {
            ExportOptions = CreateExportOptions();
            SelectedFilePath = FilePathTextBox.Text;
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput()
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(FilePathTextBox.Text))
        {
            MessageBox.Show("Please select an output file path.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate dimensions
        if (!int.TryParse(WidthTextBox.Text, out int width) || width <= 0 || width > 10000)
        {
            MessageBox.Show("Width must be a positive integer between 1 and 10000.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(HeightTextBox.Text, out int height) || height <= 0 || height > 10000)
        {
            MessageBox.Show("Height must be a positive integer between 1 and 10000.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate PNG-specific options
        if (PngRadioButton.IsChecked == true)
        {
            if (!int.TryParse(ResolutionTextBox.Text, out int resolution) || resolution <= 0 || resolution > 600)
            {
                MessageBox.Show("Resolution must be a positive integer between 1 and 600 DPI.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(QualityTextBox.Text, out int quality) || quality < 1 || quality > 100)
            {
                MessageBox.Show("Quality must be an integer between 1 and 100.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        // Validate output directory exists
        var directory = Path.GetDirectoryName(FilePathTextBox.Text);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            var result = MessageBox.Show($"The directory '{directory}' does not exist. Create it?", 
                "Directory Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create directory: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private ExportOptions CreateExportOptions()
    {
        var format = PngRadioButton.IsChecked == true ? ExportFormat.Png : ExportFormat.Svg;
        var width = int.Parse(WidthTextBox.Text);
        var height = int.Parse(HeightTextBox.Text);
        var resolution = PngRadioButton.IsChecked == true ? int.Parse(ResolutionTextBox.Text) : 96;
        var quality = PngRadioButton.IsChecked == true ? int.Parse(QualityTextBox.Text) : 90;

        string backgroundColor = "White";
        if (TransparentBackgroundRadio.IsChecked == true)
        {
            backgroundColor = "Transparent";
        }
        else if (CustomBackgroundRadio.IsChecked == true)
        {
            backgroundColor = CustomBackgroundTextBox.Text;
        }

        return new ExportOptions
        {
            Format = format,
            Width = width,
            Height = height,
            Resolution = resolution,
            Quality = quality,
            BackgroundColor = backgroundColor,
            IncludeTitle = IncludeTitleCheckBox.IsChecked == true,
            IncludeLegend = IncludeLegendCheckBox.IsChecked == true
        };
    }

    private void UpdatePreview()
    {
        try
        {
            var options = CreateExportOptions();
            var filePath = FilePathTextBox.Text;
            
            var preview = $"Format: {options.Format}\n" +
                         $"Dimensions: {options.Width} x {options.Height} pixels\n";

            if (options.Format == ExportFormat.Png)
            {
                preview += $"Resolution: {options.Resolution} DPI\n" +
                          $"Quality: {options.Quality}%\n";
            }

            preview += $"Background: {options.BackgroundColor}\n" +
                      $"Include Title: {(options.IncludeTitle ? "Yes" : "No")}\n" +
                      $"Include Legend: {(options.IncludeLegend ? "Yes" : "No")}\n";

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                preview += $"\nOutput: {fileInfo.Name}\n" +
                          $"Directory: {fileInfo.DirectoryName}";
                
                // Estimate file size
                var estimatedSizeKB = EstimateFileSize(options);
                preview += $"\nEstimated size: ~{estimatedSizeKB:F0} KB";
            }

            PreviewTextBlock.Text = preview;
            
            // Update PNG quality group visibility
            PngQualityGroup.Visibility = PngRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            PreviewTextBlock.Text = "Invalid input values. Please check your settings.";
        }
    }

    private double EstimateFileSize(ExportOptions options)
    {
        // Rough estimation based on format and dimensions
        var pixels = options.Width * options.Height;
        
        if (options.Format == ExportFormat.Png)
        {
            // PNG: roughly 3-4 bytes per pixel for charts with compression
            return (pixels * 3.5 * options.Quality / 100.0) / 1024.0;
        }
        else // SVG
        {
            // SVG: text-based, much smaller but depends on complexity
            return Math.Max(10, pixels / 1000.0); // Rough estimate
        }
    }
}