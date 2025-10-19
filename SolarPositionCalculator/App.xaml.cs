using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarPositionCalculator.Services;
using SolarPositionCalculator.ViewModels;

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
        try
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
        catch (Exception ex)
        {
            var errorMessage = $"Application startup failed: {ex.Message}\n\n";
            if (ex.InnerException != null)
            {
                errorMessage += $"Inner Exception: {ex.InnerException.Message}\n\n";
            }
            errorMessage += $"Stack trace:\n{ex.StackTrace}";

            MessageBox.Show(errorMessage, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
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

