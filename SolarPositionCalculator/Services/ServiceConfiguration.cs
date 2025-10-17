using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarPositionCalculator.ViewModels;

namespace SolarPositionCalculator.Services;

/// <summary>
/// Configuration class for setting up dependency injection services
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures all application services for dependency injection
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IAstronomicalCalculator, AstronomicalCalculator>();
        services.AddSingleton<ITimeZoneService, TimeZoneService>();
        services.AddSingleton<ICoordinateConverter, CoordinateConverter>();
        services.AddSingleton<IVisualizationService, VisualizationService>();
        services.AddSingleton<IRealTimeService, RealTimeService>();
        services.AddSingleton<ICsvExportService, CsvExportService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<EquationOfTimeViewModel>();
        services.AddTransient<SunPathViewModel>();
        services.AddTransient<CompositeViewModel>();
        services.AddTransient<CsvExportDialogViewModel>();

        // Register the main window
        services.AddTransient<MainWindow>();
    }

    /// <summary>
    /// Creates and configures the application host with dependency injection
    /// </summary>
    /// <returns>Configured host builder</returns>
    public static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            });
    }
}