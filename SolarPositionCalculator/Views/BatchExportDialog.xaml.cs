using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SolarPositionCalculator.Models;
using OxyPlot;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Batch export dialog for exporting multiple charts
/// </summary>
public partial class BatchExportDialog : Window
{
    /// <summary>
    /// Gets the batch export options configured by the user
    /// </summary>
    public BatchExportOptions? BatchExportOptions { get; private set; }

    /// <summary>
    /// Gets the list of selected chart types to export
    /// </summary>
    public List<string> SelectedChartTypes { get; private set; } = new();

    /// <summary>
    /// Charts available for export (set by caller)
    /// </summary>
    public Dictionary<string, PlotModel> AvailableCharts { get; set; } = new();

    public BatchExportDialog()
    {
        InitializeComponent();
        
        // Set default output directory
        OutputDirectoryTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Solar Charts");
        
        // Wire up event handlers
        SizeComboBox.SelectionChanged += SizeComboBox_SelectionChanged;
        QualitySlider.ValueChanged += (s, e) => QualityValueText.Text = $"{(int)e.NewValue}%";
        BatchPngRadio.Checked += (s, e) => BatchQualityPanel.Visibility = Visibility.Visible;
        BatchSvgRadio.Checked += (s, e) => BatchQualityPanel.Visibility = Visibility.Collapsed;
    }

    private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SizeComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            if (selectedItem.Content.ToString() == "Custom")
            {
                CustomSizePanel.Visibility = Visibility.Visible;
            }
            else
            {
                CustomSizePanel.Visibility = Visibility.Collapsed;
                
                // Parse standard sizes
                var sizeText = selectedItem.Content.ToString();
                if (sizeText != null)
                {
                    var parts = sizeText.Split('x');
                    if (parts.Length == 2)
                    {
                        BatchWidthTextBox.Text = parts[0];
                        BatchHeightTextBox.Text = parts[1];
                    }
                }
            }
        }
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        EquationOfTimeCheckBox.IsChecked = true;
        SunPathCheckBox.IsChecked = true;
        CurrentDayCheckBox.IsChecked = true;
    }

    private void SelectNone_Click(object sender, RoutedEventArgs e)
    {
        EquationOfTimeCheckBox.IsChecked = false;
        SunPathCheckBox.IsChecked = false;
        CurrentDayCheckBox.IsChecked = false;
    }

    private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select output directory for batch export",
            Multiselect = false
        };

        if (!string.IsNullOrEmpty(OutputDirectoryTextBox.Text))
        {
            folderDialog.InitialDirectory = OutputDirectoryTextBox.Text;
        }

        if (folderDialog.ShowDialog() == true)
        {
            OutputDirectoryTextBox.Text = folderDialog.FolderName;
        }
    }

    private async void StartExport_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateInput())
        {
            // Show progress
            ProgressGroup.Visibility = Visibility.Visible;
            ExportProgressBar.Value = 0;
            ProgressTextBlock.Text = "Preparing export...";

            try
            {
                // Create batch export options
                BatchExportOptions = CreateBatchExportOptions();
                
                // Get selected chart types
                SelectedChartTypes.Clear();
                if (EquationOfTimeCheckBox.IsChecked == true) SelectedChartTypes.Add("EquationOfTime");
                if (SunPathCheckBox.IsChecked == true) SelectedChartTypes.Add("SunPath");
                if (CurrentDayCheckBox.IsChecked == true) SelectedChartTypes.Add("CurrentDayPath");

                // Simulate progress for demo (in real implementation, this would be handled by the service)
                for (int i = 0; i <= 100; i += 10)
                {
                    ExportProgressBar.Value = i;
                    ProgressTextBlock.Text = $"Exporting charts... {i}%";
                    await Task.Delay(100); // Simulate work
                }

                ProgressTextBlock.Text = "Export completed successfully!";
                DialogResult = true;
                
                // Close after a brief delay
                await Task.Delay(1000);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressGroup.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput()
    {
        // Check if at least one chart is selected
        if (EquationOfTimeCheckBox.IsChecked != true && 
            SunPathCheckBox.IsChecked != true && 
            CurrentDayCheckBox.IsChecked != true)
        {
            MessageBox.Show("Please select at least one chart to export.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate output directory
        if (string.IsNullOrWhiteSpace(OutputDirectoryTextBox.Text))
        {
            MessageBox.Show("Please select an output directory.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate dimensions
        if (!int.TryParse(BatchWidthTextBox.Text, out int width) || width <= 0 || width > 10000)
        {
            MessageBox.Show("Width must be a positive integer between 1 and 10000.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(BatchHeightTextBox.Text, out int height) || height <= 0 || height > 10000)
        {
            MessageBox.Show("Height must be a positive integer between 1 and 10000.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Validate file prefix
        if (string.IsNullOrWhiteSpace(FilePrefixTextBox.Text))
        {
            MessageBox.Show("Please enter a file name prefix.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // Check if output directory exists or can be created
        try
        {
            if (!Directory.Exists(OutputDirectoryTextBox.Text))
            {
                Directory.CreateDirectory(OutputDirectoryTextBox.Text);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot create output directory: {ex.Message}", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private BatchExportOptions CreateBatchExportOptions()
    {
        var format = BatchPngRadio.IsChecked == true ? ExportFormat.Png : ExportFormat.Svg;
        var width = int.Parse(BatchWidthTextBox.Text);
        var height = int.Parse(BatchHeightTextBox.Text);
        var quality = (int)QualitySlider.Value;

        var exportOptions = new ExportOptions
        {
            Format = format,
            Width = width,
            Height = height,
            Quality = quality,
            Resolution = 96,
            BackgroundColor = "White",
            IncludeTitle = BatchIncludeTitleCheckBox.IsChecked == true,
            IncludeLegend = BatchIncludeLegendCheckBox.IsChecked == true
        };

        return new BatchExportOptions
        {
            ExportOptions = exportOptions,
            OutputDirectory = OutputDirectoryTextBox.Text,
            FileNamePrefix = FilePrefixTextBox.Text,
            IncludeTimestamp = IncludeTimestampCheckBox.IsChecked == true,
            GroupByChartType = GroupByTypeCheckBox.IsChecked == true
        };
    }
}