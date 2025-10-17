using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarPositionCalculator.Services;

namespace SolarPositionCalculator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Gets the current service provider
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Application startup - configure dependency injection
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Create and configure the host
        _host = ServiceConfiguration.CreateHostBuilder().Build();
        
        // Start the host
        await _host.StartAsync();
        
        // Set the service provider
        ServiceProvider = _host.Services;

        // Create and show the main window using DI
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// Application shutdown - dispose of host
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}

