using System;
using System.Windows;
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
        // Populate when loaded so XAML names and generated fields are available
        this.Loaded += DateTimePickerControl_Loaded;
    }

    /// <summary>
    /// Populates the hour and minute combo boxes
    /// </summary>
    private void PopulateTimeComboBoxes()
    {
        // Directly use generated fields from the XAML partial class.
        // These fields are declared in the generated .g.cs file during build.
        HourComboBoxControl.Items.Clear();
        MinuteComboBoxControl.Items.Clear();

        // Populate hours (0-23)
        for (int hour = 0; hour < 24; hour++)
        {
            HourComboBoxControl.Items.Add(hour.ToString("D2"));
        }

        // Populate minutes (0-59)
        for (int minute = 0; minute < 60; minute++)
        {
            MinuteComboBoxControl.Items.Add(minute.ToString("D2"));
        }

        // Set default values
        HourComboBoxControl.SelectedIndex = DateTime.Now.Hour;
        MinuteComboBoxControl.SelectedIndex = DateTime.Now.Minute;
    }

    private void DateTimePickerControl_Loaded(object? sender, RoutedEventArgs e)
    {
        // Populate the combo boxes once the control is loaded and the visual tree is ready.
        PopulateTimeComboBoxes();
        this.Loaded -= DateTimePickerControl_Loaded;
    }


}