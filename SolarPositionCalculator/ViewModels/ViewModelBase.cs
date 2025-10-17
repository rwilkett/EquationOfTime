using CommunityToolkit.Mvvm.ComponentModel;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common MVVM functionality
/// </summary>
public abstract class ViewModelBase : ObservableValidator, IDisposable
{
    private bool _disposed = false;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources here
            }
            _disposed = true;
        }
    }
}