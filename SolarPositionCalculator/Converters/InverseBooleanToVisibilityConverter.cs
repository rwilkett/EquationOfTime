using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SolarPositionCalculator.Converters;

/// <summary>
/// Converter that converts boolean values to visibility (inverted)
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to visibility (inverted)
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts visibility back to boolean (inverted)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return true;
    }
}