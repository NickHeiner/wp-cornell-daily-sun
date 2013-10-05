using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace CornellSunNewsreader
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool?)
            {
                if (string.IsNullOrEmpty((string)parameter))
                {
                    return (value as bool?).Value ? Visibility.Visible : Visibility.Collapsed;
                } else
                {
                    return (value as bool?).Value ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
