using Xamarin.Forms;

namespace ZicMoove.Views
{
    public class SvgImage : View
    {
        public string Svg
        {
            get
            {
                return GetValue(SvgProperty) as string;
            }
            set
            {
                SetValue(SvgProperty, value);
            }
        }

        public static readonly BindableProperty SvgProperty =
            BindableProperty.Create("Svg", typeof(string), typeof(SvgImage),
                null, BindingMode.TwoWay);
    }
}
