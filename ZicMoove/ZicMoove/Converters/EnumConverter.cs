using System;
using System.Globalization;
using Xamarin.Forms;

namespace ZicMoove.Converters
{
    /// <summary>
    /// When EnumType:int
    /// </summary>
    class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value;
        }
    }
}
