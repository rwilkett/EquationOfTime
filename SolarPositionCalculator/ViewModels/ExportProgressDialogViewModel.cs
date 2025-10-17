using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.ViewModels;

/// <summary>
/// ViewModel for the export progress dialog
/// </summary>
public partial class ExportProgressDialogViewModel : ObservableObject
{
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string _title = "Exporting Data";

    [ObservableProperty]
    private string _currentOperation = "Preparing export...";

    [ObservableProperty]
    private double _progressPercentage = 0;

    [ObservableProperty]
    private string _progressText = "0 / 0 items";

    [ObservableProperty]
    private TimeSpan _elapsedTime = TimeSpan.Zero;

    [ObservableProperty]
    private TimeSpan? _estimatedTimeRemaining;

    [ObservableProperty]
    private bool _canCancel = true;

    [ObservableProperty]
    private bool _wasSuccessful = false;

    public event EventHandler? ExportCompleted;

    public CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

    public ExportProgressDialogViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Updates the progress based on export progress information
    /// </summary>
    public void UpdateProgress(ExportProgress progress)
    {
        CurrentOperation = progress.CurrentOperation;
        ProgressPercentage = progress.PercentComplete;
        ProgressText = $"{progress.ProcessedItems:N0} / {progress.TotalItems:N0} items";
        ElapsedTime = progress.ElapsedTime;
        EstimatedTimeRemaining = progress.EstimatedTimeRemaining;
    }

    /// <summary>
    /// Marks the export as completed successfully
    /// </summary>
    public void CompleteSuccessfully()
    {
        WasSuccessful = true;
        CanCancel = false;
        CurrentOperation = "Export completed successfully";
        ProgressPercentage = 100;
        ExportCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Marks the export as failed
    /// </summary>
    public void CompleteFailed(string errorMessage)
    {
        WasSuccessful = false;
        CanCancel = false;
        CurrentOperation = $"Export failed: {errorMessage}";
        ExportCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Command to cancel the export operation
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        CanCancel = false;
        CurrentOperation = "Cancelling export...";
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName == nameof(CanCancel))
        {
            CancelCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}