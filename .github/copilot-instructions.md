## Quick orientation for AI coding agents

This repository is a WPF (net8.0-windows) MVVM application that calculates and visualizes solar positions and sun paths.
Focus on small, well-scoped changes. Respect MVVM, DI, and background-calculation patterns used throughout.

### Big picture / architecture

- Project: `SolarPositionCalculator` (WPF). TargetFramework: `net8.0-windows` (UseWPF=true) — see `SolarPositionCalculator.csproj`.
- Layers: `Models/` (astronomy data types like `SolarPosition`, `SunPath`, `PolarCondition`), `Services/` (algorithm and visualization services), `ViewModels/` (CommunityToolkit.Mvvm-based view models), `Views/` (WPF XAML), `Converters/` (XAML converters).
- Startup: `App.xaml.cs` builds an `IHost` using `ServiceConfiguration.CreateHostBuilder()` and resolves `MainWindow` from DI. Modify `ServiceConfiguration` to change registrations.
- Key services/interfaces to read before changing behavior: `IAstronomicalCalculator`, `IVisualizationService` (look in `Services/`). Many view models expect these in constructors (example: `SunPathViewModel(IAstronomicalCalculator, IVisualizationService)`).

### Project-specific patterns and conventions

- MVVM with CommunityToolkit source generators: look for `[ObservableProperty]` and `[RelayCommand]` attributes in `ViewModels/`. Do not remove generated backing-field expectations — run a build to validate source-generator outputs.
- Long-running/calculation code runs off the UI thread via `Task.Run` and toggles `IsLoading`/`StatusMessage` — preserve this pattern to avoid UI freezes (example: `ViewModels/SunPathViewModel.cs` uses `Task.Run` for CalculateSunPathAsync).
- Polar-region handling: code paths detect `PolarCondition` and call specialized visualization methods (e.g., `CreatePolarSunPathDiagram`). When adding visualizations, extend `IVisualizationService` and update implementations in `Services/`.
- Plotting: uses `OxyPlot.Wpf` and a wrapper `InteractivePlotModel` (see `ViewModels` and `Services`). Respect PlotModel lifetimes when updating UI.
- DI / singleton vs scoped: project uses `Microsoft.Extensions.Hosting` for service registration. MainWindow and ViewModels are resolved from the host. To add a new service, register it in `ServiceConfiguration.CreateHostBuilder()`.

### Build / test / run workflows (concrete commands)

- Restore & build: `dotnet restore` then `dotnet build SolarPositionCalculator/SolarPositionCalculator.csproj` (or use the VS Code tasks in workspace). The app uses implicit usings and nullable enabled — compile to catch generator errors.
- Run (debug / manual): `dotnet run --project SolarPositionCalculator/SolarPositionCalculator.csproj` (launches the WPF app).
- Tests: run `dotnet test SolarPositionCalculator.Tests/SolarPositionCalculator.Tests.csproj`. Important tests include `NOAAValidationTests.cs`, `PolarRegionTests.cs` — these validate astronomical algorithms; run them after algorithm edits.
- CI / publish: use `dotnet publish --configuration Release --output publish` or the provided `publish-self-contained` task (runtime `win-x64`) when creating distributables.

### Files and locations to check for most change types

- UI/behavior changes: `ViewModels/` and `Views/` (XAML). Example: `ViewModels/SunPathViewModel.cs` and `Views/MainWindow.xaml`.
- Core algorithms: `Services/` and `Models/` (e.g., `IAstronomicalCalculator` implementation, `Models/SolarPosition.cs`, `SunPath.cs`). Tests live in `SolarPositionCalculator.Tests/`.
- DI/host configuration: search for `ServiceConfiguration.CreateHostBuilder()` and update registrations there.
- Converters: `Converters/` contains small XAML helpers (BooleanToVisibility, NullToVisibility, etc.). Keep them pure and simple.

### Edit & testing guidance for AI agents

- Small PRs: prefer focused changes (one ViewModel/Service at a time). Run `dotnet build` and `dotnet test` locally before proposing changes.
- When touching calculation code, run the test suite (`SolarPositionCalculator.Tests`) — tests are the primary safety net for algorithm regressions.
- UI changes must preserve threading contract: heavy calc → `Task.Run` or move into service; update `IsLoading` and `StatusMessage` accordingly.
- When adding public methods used by XAML or by source-generated properties/commands, run a build to ensure no missing members (CommunityToolkit will generate code at build time).

### Examples (copy-paste friendly)

- Run the app locally:

```
dotnet run --project SolarPositionCalculator/SolarPositionCalculator.csproj
```

- Run tests:

```
dotnet test SolarPositionCalculator.Tests/SolarPositionCalculator.Tests.csproj
```

- Build release and create self-contained publish (Windows x64):

```
dotnet publish SolarPositionCalculator/SolarPositionCalculator.csproj --configuration Release --self-contained true --runtime win-x64 -o publish/self-contained
```

### What to avoid / gotchas

- Don't edit files under `obj/` or compiled output in `bin/`.
- Avoid UI-thread heavy work — follow existing Task.Run pattern.
- Changing DI registrations can silently change lifetime behavior; prefer adding new registrations and tests rather than modifying many existing registrations at once.
- WPF + net8.0-windows requires a Windows environment to run UI tests; headless CI will only run unit tests.

If anything in these notes is unclear or you'd like more detail (e.g., list of concrete service implementations, exact DI registrations, or a suggested PR checklist), tell me which part to expand and I'll iterate.
