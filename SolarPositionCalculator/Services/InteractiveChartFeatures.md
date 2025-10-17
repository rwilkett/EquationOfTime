# Interactive Chart Features Documentation

## Overview

The VisualizationService now includes comprehensive interactive features for both equation of time charts and sun path diagrams, fulfilling the requirements for task 4.2.

## Implemented Interactive Features

### 1. Equation of Time Chart Interactions

#### Hover Tooltips
- **Feature**: Displays detailed information when hovering over data points
- **Information Shown**:
  - Chart title
  - Date (formatted as "MMM dd")
  - Equation of time value in minutes (formatted to 2 decimal places)
- **Implementation**: Uses OxyPlot's built-in tracker functionality with custom format string
- **Requirements Satisfied**: 2.3 (hover tooltips for equation of time values)

#### Click-to-Select Date Functionality
- **Feature**: Allows users to click on any point on the equation of time curve to select that date
- **Behavior**: 
  - Left mouse click on chart detects nearest data point
  - Fires `DateSelected` event with selected date and equation of time value
  - Event includes chart type identifier for proper handling
- **Event Data**: `ChartInteractionEventArgs` containing:
  - `SelectedDate`: The date corresponding to the clicked point
  - `SelectedValue`: The equation of time value at that date
  - `ChartType`: "EquationOfTime" identifier
- **Requirements Satisfied**: 2.4 (click-to-select date functionality on charts)

### 2. Sun Path Diagram Interactions

#### Current Position Highlighting
- **Feature**: Prominently displays the current sun position on the sun path diagram
- **Visual Elements**:
  - Gold star marker (size 12) with dark goldenrod border
  - Text annotation showing "Current" and time (HH:mm format)
  - Positioned 5 degrees above the actual position for visibility
- **Conditional Display**: Only shown when current position is above horizon (`IsSunVisible = true`)
- **Requirements Satisfied**: 3.3 (current position highlighting on sun path diagrams)

#### Interactive Position Selection
- **Feature**: Click anywhere on the sun path to get detailed position information
- **Behavior**:
  - Left mouse click finds nearest point on sun path curve
  - Calculates corresponding time of day from original data
  - Fires `PositionSelected` event with position and time information
- **Event Data**: `SunPathInteractionEventArgs` containing:
  - `Azimuth`: Compass direction in degrees (0-360)
  - `Elevation`: Height above horizon in degrees
  - `TimeOfDay`: Corresponding time when sun is at that position (if available)

#### Enhanced Visual Features
- **Sunrise/Sunset Markers**: 
  - Yellow circle for sunrise
  - Red circle for sunset
  - Size 8 with colored borders for visibility
- **Horizon Line**: Brown dashed line at 0° elevation
- **Polar Condition Indicators**:
  - "Midnight Sun" annotation for polar day conditions
  - "Polar Night" annotation for polar night conditions
- **Interactive Tooltips**: Show azimuth, elevation, and time for any point on the path

### 3. Chart Styling and Interaction Enhancements

#### Professional Appearance
- **Grid Lines**: Major and minor grid lines for both axes
- **Color Scheme**: 
  - Blue line for equation of time curve
  - Orange line for sun path
  - Appropriate colors for markers and annotations
- **Axis Configuration**:
  - Proper scaling and labeling
  - Date formatting for time axes
  - Degree symbols for angular measurements

#### Mouse Interaction Handling
- **Responsive Design**: Charts respond immediately to mouse interactions
- **Event System**: Robust event handling with proper error checking
- **Data Point Detection**: Accurate nearest-point algorithms for precise selection

## Event System Architecture

### Event Handler Signatures
```csharp
// For equation of time chart interactions
public event EventHandler<ChartInteractionEventArgs>? DateSelected;

// For sun path diagram interactions  
public event EventHandler<SunPathInteractionEventArgs>? PositionSelected;
```

### Usage Example
```csharp
var visualizationService = new VisualizationService();

// Subscribe to equation of time chart events
visualizationService.DateSelected += (sender, args) =>
{
    Console.WriteLine($"Selected date: {args.SelectedDate:yyyy-MM-dd}");
    Console.WriteLine($"Equation of time: {args.SelectedValue:F2} minutes");
    // Update UI or perform calculations for selected date
};

// Subscribe to sun path diagram events
visualizationService.PositionSelected += (sender, args) =>
{
    Console.WriteLine($"Sun position: {args.Azimuth:F1}° Az, {args.Elevation:F1}° El");
    if (args.TimeOfDay.HasValue)
    {
        Console.WriteLine($"Time: {args.TimeOfDay:HH:mm}");
    }
    // Update UI or show detailed position information
};
```

## Requirements Compliance

### Requirement 2.3: Hover Tooltips
✅ **Implemented**: Equation of time chart displays exact values on hover with formatted tooltips showing date and minutes.

### Requirement 2.4: Click-to-Select Date
✅ **Implemented**: Users can click on equation of time chart to select dates, firing events with selected date information.

### Requirement 3.3: Current Position Highlighting  
✅ **Implemented**: Sun path diagrams highlight current sun position with distinctive star marker and time annotation.

## Technical Implementation Details

### OxyPlot Integration
- Uses OxyPlot's native mouse event handling
- Leverages built-in tracker functionality for tooltips
- Implements custom nearest-point detection algorithms
- Utilizes OxyPlot's annotation system for labels and markers

### Performance Considerations
- Efficient event handling with minimal computational overhead
- Optimized nearest-point calculations
- Lazy evaluation of time-of-day calculations for sun path interactions
- Memory-efficient event argument objects

### Error Handling
- Null checks for optional parameters
- Graceful handling of edge cases (polar conditions, below-horizon positions)
- Robust event firing with proper exception handling

## Future Enhancement Possibilities

While not required for the current task, the architecture supports:
- Multi-touch gesture support
- Zoom and pan interactions
- Custom tooltip formatting
- Animation effects for position updates
- Keyboard navigation support