# Chart Export Functionality

This document describes the chart export features implemented in the Solar Position Calculator application.

## Features

### 1. Single Chart Export with Dialog

The application provides an interactive export dialog that allows users to:

- **Format Selection**: Choose between PNG and SVG formats
- **Custom Dimensions**: Set width and height with preset options (Standard, Large, HD)
- **Quality Settings**: Configure PNG resolution (DPI) and quality (1-100%)
- **Chart Options**: Include/exclude chart title and legend
- **Background Options**: White, transparent, or custom color backgrounds
- **Real-time Preview**: See export settings and estimated file size

**Usage:**
```csharp
var visualizationService = new VisualizationService();
bool success = visualizationService.ShowExportDialog(chartModel, "my_chart");
```

### 2. Batch Export with Dialog

Export multiple charts simultaneously with:

- **Chart Selection**: Choose which charts to export (Equation of Time, Sun Path, etc.)
- **Unified Settings**: Apply same format and quality settings to all charts
- **Organized Output**: Option to group charts by type in subdirectories
- **Timestamp Support**: Include timestamps in filenames
- **Progress Tracking**: Visual progress indicator during export

**Usage:**
```csharp
var charts = new Dictionary<string, PlotModel>
{
    ["EquationOfTime"] = equationChart,
    ["SunPath"] = sunPathChart
};

var visualizationService = new VisualizationService();
bool success = visualizationService.ShowBatchExportDialog(charts);
```

### 3. Programmatic Export

For automated or scripted exports:

**Single Chart with Custom Options:**
```csharp
var exportOptions = new ExportOptions
{
    Format = ExportFormat.Png,
    Width = 1200,
    Height = 800,
    Resolution = 150,
    Quality = 95,
    BackgroundColor = "White",
    IncludeTitle = true,
    IncludeLegend = true
};

visualizationService.ExportChartWithOptions(chart, "output.png", exportOptions);
```

**Batch Export:**
```csharp
var charts = new List<ChartExportInfo>
{
    new() { Chart = chart1, ChartType = "EquationOfTime", CustomFileName = "eot_2024" },
    new() { Chart = chart2, ChartType = "SunPath", CustomFileName = "sun_path" }
};

var batchOptions = new BatchExportOptions
{
    ExportOptions = exportOptions,
    OutputDirectory = @"C:\Exports",
    FileNamePrefix = "solar_chart",
    IncludeTimestamp = true,
    GroupByChartType = true
};

int successCount = visualizationService.BatchExportCharts(charts, batchOptions);
```

## Export Options

### ExportOptions Properties

- **Format**: PNG or SVG
- **Width/Height**: Image dimensions in pixels (1-10000)
- **Resolution**: DPI for PNG exports (1-600)
- **Quality**: PNG quality percentage (1-100)
- **BackgroundColor**: "White", "Transparent", or hex color (#FFFFFF)
- **IncludeTitle**: Whether to include chart title
- **IncludeLegend**: Whether to include chart legend

### BatchExportOptions Properties

- **ExportOptions**: Base export settings for all charts
- **OutputDirectory**: Target directory for exported files
- **FileNamePrefix**: Prefix for generated filenames
- **IncludeTimestamp**: Add timestamp to filenames (yyyyMMdd_HHmmss)
- **GroupByChartType**: Create subdirectories by chart type

## File Naming

### Single Export
- User-specified filename through dialog
- Automatic extension based on format (.png or .svg)

### Batch Export
- Pattern: `{FileNamePrefix}_{ChartType}_{Timestamp}.{extension}`
- Example: `solar_chart_EquationOfTime_20241016_143022.png`
- Custom filenames override the pattern

### Directory Structure (with GroupByChartType)
```
OutputDirectory/
├── EquationOfTime/
│   ├── solar_chart_EquationOfTime_20241016_143022.png
│   └── ...
├── SunPath/
│   ├── solar_chart_SunPath_20241016_143022.png
│   └── ...
└── ...
```

## Error Handling

The export functionality includes comprehensive error handling:

- **Input Validation**: Validates dimensions, quality settings, and file paths
- **Directory Creation**: Automatically creates output directories if they don't exist
- **File Conflicts**: Overwrites existing files (user is warned in dialogs)
- **Format Support**: Clear error messages for unsupported formats
- **Batch Resilience**: Continues exporting other charts if one fails

## Testing and Examples

Use the `ExportUtility` class for testing and examples:

```csharp
// Test single export dialog
var sampleChart = ExportUtility.CreateSampleEquationOfTimeChart();
ExportUtility.ExportSingleChartWithDialog(sampleChart, "test_chart");

// Test batch export dialog
ExportUtility.ExportMultipleChartsWithDialog();

// Test programmatic export
ExportUtility.ExportWithCustomOptions(sampleChart, @"C:\temp\custom_export.png");

// Test programmatic batch export
int exported = ExportUtility.BatchExportProgrammatically(@"C:\temp\batch_export");
```

## Integration with Main Application

The export functionality integrates with the main application through:

1. **Menu Items**: File → Export Chart, File → Batch Export
2. **Toolbar Buttons**: Export icons in the main toolbar
3. **Context Menus**: Right-click on charts for quick export
4. **Keyboard Shortcuts**: Ctrl+E for single export, Ctrl+Shift+E for batch export

## Performance Considerations

- **Large Images**: High-resolution exports may take several seconds
- **Batch Operations**: Progress is reported for user feedback
- **Memory Usage**: Charts are processed one at a time to minimize memory usage
- **File I/O**: Directory creation and file writing are optimized for batch operations

## Future Enhancements

Potential improvements for future versions:

- **PDF Export**: Add PDF format support using additional libraries
- **Print Support**: Direct printing of charts
- **Cloud Export**: Integration with cloud storage services
- **Template System**: Predefined export templates for common use cases
- **Watermarking**: Optional watermarks for exported images