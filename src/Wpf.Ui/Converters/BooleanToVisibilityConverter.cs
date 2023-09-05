using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Ui.Converters;

internal class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool b)
        {
            return Visibility.Collapsed;
        }

        if (parameter is string s)
        {
            if (s.Contains("invert", StringComparison.OrdinalIgnoreCase))
            {
                b = !b;
            }

            if (s.Contains("hidden", StringComparison.OrdinalIgnoreCase))
            {
                return b ? Visibility.Visible : Visibility.Hidden;
            }
        }

        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Visibility visibility)
        {
            return value;
        }

        var b = visibility == Visibility.Visible;

        if (parameter is string s)
        {
            if (s.Contains("invert", StringComparison.OrdinalIgnoreCase))
            {
                b = !b;
            }
        }

        return b;
    }
}
