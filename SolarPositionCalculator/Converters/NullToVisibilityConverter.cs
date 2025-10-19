using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SolarPositionCalculator.Converters;

/// <summary>
/// Converter that converts null values to visibility
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a null value to visibility
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Converts visibility back to null/not null (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}