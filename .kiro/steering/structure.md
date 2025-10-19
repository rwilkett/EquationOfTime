# Project Structure

## Solution Organization
```
SolarPositionCalculator/          # Main WPF application
├── Models/                       # Data models and domain objects
├── ViewModels/                   # MVVM view models
├── Views/                        # WPF user controls and dialogs
├── Services/                     # Business logic and calculations
├── Converters/                   # WPF value converters
├── App.xaml[.cs]                # Application entry point
├── MainWindow.xaml[.cs]         # Main application window
└── AssemblyInfo.cs              # Assembly metadata

SolarPositionCalculator.Tests/    # Unit test project
├── *Tests.cs                    # Test classes by category
└── README.md                    # Test documentation
```

## Architecture Patterns

### MVVM (Model-View-ViewModel)
- **Models**: Pure data classes in `Models/` folder
- **ViewModels**: Inherit from `ViewModelBase`, use CommunityToolkit.Mvvm
- **Views**: XAML files with minimal code-behind
- **Dependency Injection**: Configured in `Services/ServiceConfiguration.cs`

### Service Layer
- **Interfaces**: All services implement interfaces (I*Service pattern)
- **Registration**: Services registered in DI container during startup
- **Separation**: Business logic separated from UI concerns

## Naming Conventions

### Files and Classes
- **ViewModels**: `*ViewModel.cs` (e.g., `MainViewModel.cs`)
- **Services**: `*Service.cs` with corresponding `I*Service.cs` interface
- **Models**: Descriptive names (e.g., `SolarPosition.cs`, `GeographicCoordinate.cs`)
- **Views**: `*Control.xaml` for user controls, `*Dialog.xaml` for dialogs
- **Converters**: `*Converter.cs` (e.g., `BooleanToVisibilityConverter.cs`)

### Code Style
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Enabled for cleaner files
- **Accessibility**: All UI elements have proper accessibility support
- **Documentation**: XML documentation for public APIs

## Key Folders

### `/Models`
Domain objects and data structures. Keep these as simple POCOs without business logic.

### `/Services`
Business logic, calculations, and external integrations. Each service should have a single responsibility.

### `/ViewModels`
UI state management and command handling. Use CommunityToolkit.Mvvm attributes for property change notifications.

### `/Views`
XAML user controls and dialogs. Minimize code-behind - prefer data binding and commands.

### `/Converters`
WPF value converters for data binding. Keep these stateless and focused on single conversions.

## Testing Structure
- **Core Tests**: `BasicAstronomicalTests` and `TaskRequirementTests` (must pass)
- **Reference Tests**: NOAA validation tests (may have tolerance differences)
- **Specialized Tests**: Polar regions, edge cases, integration 