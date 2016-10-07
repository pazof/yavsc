using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Converters
{
    class RatingText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var rating = (int)value;
            if (rating == 0)
                return "Nul!";
            if (rating == 1)
                return "Décevant!";
            if (rating == 2)
                return "Pas terrible!";
            if (rating == 3)
                return "Bien!";
            if (rating == 4)
                return "J'aime!";
            if (rating == 5)
                return "J'adore";

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
