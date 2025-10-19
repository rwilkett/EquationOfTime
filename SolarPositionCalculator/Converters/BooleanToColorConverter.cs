using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SolarPositionCalculator.Converters;

/// <summary>
/// Converter that converts boolean values to colors
/// </summary>
public class BooleanToColorConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a color
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Brushes.Green : Brushes.Gray;
        }

        return Brushes.Gray;
    }

    /// <summary>
    /// Converts a color back to a boolean (not implemented)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}