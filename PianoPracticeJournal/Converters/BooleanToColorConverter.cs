using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PianoPracticeJournal.Converters;

/// <summary>
/// Converts boolean values to colors for UI display
/// </summary>
public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Brushes.Green : Brushes.Red;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
