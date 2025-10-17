using System.Windows.Controls;

namespace SolarPositionCalculator.Views;

/// <summary>
/// Interaction logic for DateTimePickerControl.xaml
/// </summary>
public partial class DateTimePickerControl : UserControl
{
    public DateTimePickerControl()
    {
        InitializeComponent();
        PopulateTimeComboBoxes();
    }

    /// <summary>
    /// Populates the hour and minute combo boxes
    /// </summary>
    private void PopulateTimeComboBoxes()
    {
        // Populate hours (0-23)
        for (int hour = 0; hour < 24; hour++)
        {
            HourComboBox.Items.Add(hour.ToString("D2"));
        }

        // Populate minutes (0-59)
        for (int minute = 0; minute < 60; minute++)
        {
            MinuteComboBox.Items.Add(minute.ToString("D2"));
        }

        // Set default values
        HourComboBox.SelectedIndex = DateTime.Now.Hour;
        MinuteComboBox.SelectedIndex = DateTime.Now.Minute;
    }
}