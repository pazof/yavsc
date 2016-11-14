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

#if DEV
        public const string YavscHomeUrl = "http://dev.pschneider.fr";
#else
#if YAVSC
        public const string YavscHomeUrl = "https://yavsc.pschneider.fr";
#else
#if LUA
        public const string YavscHomeUrl = "https://lua.pschneider.fr";
#else
        public const string YavscHomeUrl = "https://booking.pschneider.fr";
#endif
#endif
#endif
        public static readonly string YavscApiUrl = YavscHomeUrl + "/api";
        public static readonly string MobileRegistrationUrl = YavscApiUrl + "/gcm/register";
        public static readonly string UserInfoUrl = YavscApiUrl + "/me";
        public static readonly string BlogUrl = YavscApiUrl + "/blogs";
        public static readonly string FsUrl = YavscApiUrl + "/fs";
        public static readonly string SignalRHubsUrl = YavscHomeUrl + "/api/signalr/hubs";
#endregion

#region Permissions ids
        public static int AllowBeATarget = 1;
#endregion

        public static int CloudTimeout = 400;
    }
}
