using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SolarPositionCalculator.Converters;

/// <summary>
/// Converts boolean values to color brushes
/// </summary>
public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Brushes.Green : Brushes.Red;
        }
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}