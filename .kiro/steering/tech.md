# Technology Stack

## Framework & Runtime
- **.NET 8.0** with Windows-specific features (`net8.0-windows`)
- **WPF (Windows Presentation Foundation)** for desktop UI
- **C# 12** with nullable reference types enabled
- **Implicit usings** enabled for cleaner code

## Key Dependencies
- **CommunityToolkit.Mvvm** (8.4.0) - MVVM pattern implementation
- **OxyPlot.Wpf** (2.2.0) - Interactive charting and visualization
- **Microsoft.Extensions.DependencyInjection** (8.0.0) - Dependency injection container
- **Microsoft.Extensions.Hosting** (8.0.0) - Application hosting and lifecycle management

## Testing Framework
- **xUnit** (2.6.1) - Unit testing framework
- **Microsoft.NET.Test.Sdk** (17.8.0) - Test SDK
- **Coverlet.collector** (6.0.0) - Code coverage collection

## Build System
- **MSBuild** with SDK-style project files
- **Visual Studio** or **VS Code** recommended IDEs

## Common Commands

### Building
```bash
# Build the solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Testing
```bash
# Run all tests
dotnet test

# Run core tests only (recommended)
dotnet test --filter "BasicAstronomicalTests|TaskRequirementTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Running
```bash
# Run the application
dotnet run --project SolarPositionCalculator

# Run in Release mode
dotnet run --project SolarPositionCalculator -c Release
```

### Publishing
```bash
# Publish self-contained executable
dotnet publish SolarPositionCalculator -c Release -r win-x64 --self-contained

# Publish framework-dependent
dotnet publish SolarPositionCalculator -c Release
```