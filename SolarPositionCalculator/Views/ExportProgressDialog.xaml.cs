using System.Windows;
using SolarPositionCalculator.ViewModels;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Interaction logic for ExportProgressDialog.xaml
/// </summary>
public partial class ExportProgressDialog : Window
{
    public ExportProgressDialogViewModel ViewModel { get; }

    public ExportProgressDialog(ExportProgressDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        
        // Subscribe to completion event to close dialog
        ViewModel.ExportCompleted += OnExportCompleted;
    }

    private void OnExportCompleted(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            DialogResult = ViewModel.WasSuccessful;
            Close();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel.ExportCompleted -= OnExportCompleted;
        base.OnClosed(e);
    }
}