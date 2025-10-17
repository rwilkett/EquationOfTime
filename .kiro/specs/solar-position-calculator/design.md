# Design Document

## Overview

The Solar Position Calculator is a WPF desktop application that provides astronomical calculations and visualizations for solar positioning. The application implements standard astronomical algorithms to calculate solar coordinates and the equation of time, presenting results through interactive charts and diagrams. The architecture follows MVVM pattern with separate calculation engines for astronomical computations.

## Architecture

### High-Level Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Presentation  │    │   Business Logic │    │   Data Models   │
│     Layer       │◄──►│      Layer       │◄──►│     Layer       │
│   (WPF Views)   │    │  (Calculations)  │    │  (Coordinates)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Technology Stack

- **Framework**: .NET 8 WPF Application
- **UI Pattern**: MVVM with CommunityToolkit.Mvvm
- **Charting**: OxyPlot for equation of time graphs and sun path diagrams
- **Mathematics**: System.Math with custom astronomical calculation extensions
- **Data Binding**: Two-way binding for real-time updates
- **Export**: System.IO for CSV export, OxyPlot export for images

## Components and Interfaces

### Core Components

#### 1. Astronomical Calculation Engine
```csharp
public interface IAstronomicalCalculator
{
    SolarPosition CalculateSolarPosition(GeographicCoordinate location, DateTime dateTime);
    double CalculateEquationOfTime(DateTime date);
    SunPath CalculateDailySunPath(GeographicCoordinate location, DateTime date);
    EquationOfTimeData[] CalculateAnnualEquationOfTime(int year);
}
```

#### 2. Coordinate System Manager
```csharp
public interface ICoordinateConverter
{
    GeographicCoordinate ParseCoordinates(string input, CoordinateFormat format);
    string FormatCoordinates(GeographicCoordinate coordinate, CoordinateFormat format);
    DateTime ConvertToTimeZone(DateTime utc, TimeZoneInfo timeZone);
}
```

#### 3. Visualization Engine
```csharp
public interface IVisualizationService
{
    PlotModel CreateEquationOfTimeChart(EquationOfTimeData[] data);
    PlotModel CreateSunPathDiagram(SunPath sunPath, SolarPosition currentPosition);
    void ExportChart(PlotModel chart, string filePath, ExportFormat format);
}
```

#### 4. Real-Time Update Service
```csharp
public interface IRealTimeService
{
    event EventHandler<TimeUpdateEventArgs> TimeUpdated;
    void StartRealTimeUpdates();
    void StopRealTimeUpdates();
    bool IsRunning { get; }
}
```

### ViewModels

#### MainViewModel
- Coordinates input and validation
- Date/time selection and real-time mode
- Solar position display properties
- Command handlers for calculations and exports

#### EquationOfTimeViewModel
- Annual equation of time data
- Chart interaction handling
- Date selection from chart clicks

#### SunPathViewModel
- Sun path diagram data
- Current position highlighting
- Interactive sky dome controls

## Data Models

### Core Data Structures

```csharp
public record GeographicCoordinate(double Latitude, double Longitude)
{
    public bool IsValid => Latitude >= -90 && Latitude <= 90 && 
                          Longitude >= -180 && Longitude <= 180;
}

public record SolarPosition(
    double Azimuth,      // Degrees from North (0-360)
    double Elevation,    // Degrees above horizon (-90 to +90)
    DateTime Timestamp,
    GeographicCoordinate Location)
{
    public bool IsSunVisible => Elevation > 0;
}

public record EquationOfTimeData(DateTime Date, double Minutes);

public record SunPath(
    GeographicCoordinate Location,
    DateTime Date,
    SolarPosition[] DailyPositions,
    SolarPosition Sunrise,
    SolarPosition Sunset)
{
    public bool HasSunrise => Sunrise != null;
    public bool HasSunset => Sunset != null;
}
```

### Calculation Models

```csharp
public static class AstronomicalConstants
{
    public const double EarthObliquity = 23.4397; // degrees
    public const double EccentricityFactor = 0.0167;
    public const double SolarConstant = 1361; // W/m²
}

public record SolarCalculationParameters(
    double JulianDay,
    double SolarDeclination,
    double HourAngle,
    double EquationOfTime);
```

## Error Handling

### Input Validation Strategy
- Coordinate bounds checking with user-friendly error messages
- Date range validation (reasonable astronomical date limits)
- Time zone validation and automatic detection
- Graceful handling of polar region edge cases

### Calculation Error Management
- Numerical precision handling for extreme latitudes
- Midnight sun and polar night condition detection
- Invalid calculation result handling with fallback values
- Logging of calculation errors for debugging

### UI Error Presentation
- Inline validation messages for input fields
- Status bar notifications for calculation errors
- Tooltip warnings for edge case conditions
- Export operation error dialogs

## Testing Strategy

### Unit Testing Approach
- Astronomical calculation accuracy tests against known values
- Coordinate conversion and validation tests
- Edge case testing for polar regions and date boundaries
- Mathematical precision tests for equation of time calculations

### Integration Testing
- End-to-end calculation workflows
- Real-time update system testing
- Chart generation and export functionality
- MVVM binding and UI update testing

### Validation Data Sources
- NOAA Solar Position Calculator for reference values
- Astronomical Almanac data for equation of time verification
- Known solar events (solstices, equinoxes) for accuracy validation
- Multiple geographic locations for comprehensive testing

### Performance Testing
- Real-time update performance under continuous operation
- Large dataset handling for annual calculations
- Chart rendering performance with complex sun paths
- Memory usage monitoring for long-running sessions

## Implementation Notes

### Astronomical Algorithm References
- Jean Meeus "Astronomical Algorithms" for core calculations
- NOAA Solar Position Algorithm (SPA) for high accuracy
- Duffett-Smith "Practical Astronomy with Calculator" for validation

### UI/UX Considerations
- Responsive layout for different screen sizes
- Accessibility support for screen readers
- Keyboard navigation for all interactive elements
- High contrast mode support for visibility

### Performance Optimizations
- Caching of frequently calculated values
- Lazy loading of annual equation of time data
- Efficient chart update strategies for real-time mode
- Background threading for intensive calculations