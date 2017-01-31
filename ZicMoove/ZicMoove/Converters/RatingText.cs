using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove.Converters
{
    class RatingText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var rating = (int)value;
            if (rating == 0)
                return Strings.NoStar;
            if (rating == 1)
                return Strings.OneStar;
            if (rating == 2)
                return Strings.TwoStars;
            if (rating == 3)
                return Strings.ThreeStars;
            if (rating == 4)
                return Strings.ForStars;
            if (rating == 5)
                return Strings.FiveStars;

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
