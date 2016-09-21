using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BookAStar
{
    [ContentProperty("Source")]
    public class AvatarImageSourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            
            // Do your translation lookup here, using whatever method you require
            
            if (Source != null)
            {
                return ImageSource.FromUri(new Uri(Source));
            }
            return ImageSource.FromResource("BookAStar.icon-anon.png");
        }
    }
}
