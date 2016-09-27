using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar
{
    public static class Constants
    {
        public const string ApplicationName = "Booking Star";

        #region Uri

        public const string YavscHomeUrl = "http://dev.pschneider.fr";
        public static readonly string YavscApiUrl = YavscHomeUrl + "/api";
        public static readonly string MobileRegistrationUrl = YavscApiUrl + "/gcm/register";
        public static readonly string UserInfoUrl = YavscApiUrl + "/me";
        public static readonly string BlogUrl = YavscApiUrl + "/blogs";

        #endregion

        #region Colors & sizes
        [Obsolete]
        public static readonly Thickness PagePadding =
            new Thickness(5, Device.OnPlatform(20, 20, 20), 5, 5);

        [Obsolete]
        public static readonly Thickness DefaultPadding =
            new Thickness(5, Device.OnPlatform(20, 20, 20), 3, 3);

        [Obsolete]
        public static readonly Font TitleFont =
            Font.SystemFontOfSize(Device.OnPlatform(50, 50, 50), FontAttributes.Bold);

        [Obsolete]
        public static readonly Color OddBackgroundColor =
            Device.OnPlatform(Color.Aqua, Color.Aqua, Color.Aqua);

        [Obsolete]
        public static readonly Color ForegroundColor =
            Device.OnPlatform(Color.White, Color.White, Color.White);

        [Obsolete]
        public static readonly Color BackgroundColor =
            Device.OnPlatform(Color.Black, Color.White, Color.White);

        #endregion
    }
}
