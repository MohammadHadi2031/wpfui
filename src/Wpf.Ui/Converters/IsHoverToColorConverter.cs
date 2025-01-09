using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.Ui.Converters;

internal class IsHoverToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 3 ||
            values[0] is not bool isHover ||
            values[1] is not SolidColorBrush hoveredBrush ||
            values[2] is not SolidColorBrush notHoveredBrush
        )
        {
            return Binding.DoNothing;
        }

        return isHover ? hoveredBrush : notHoveredBrush;
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
