using System.Windows;
using SolarPositionCalculator.ViewModels;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Interaction logic for CsvExportDialog.xaml
/// </summary>
public partial class CsvExportDialog : Window
{
    public CsvExportDialogViewModel ViewModel { get; }

    public CsvExportDialog(CsvExportDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
    }

    public bool? ShowDialog(Window owner)
    {
        Owner = owner;
        return ShowDialog();
    }
}