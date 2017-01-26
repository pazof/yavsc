using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove
{
    public static class Constants
    {

#if DEV
        public const string ApplicationName = "yadev";
        public const string YavscHomeUrl = "http://dev.pschneider.fr";
#endif
#if WDEV
        // against a Windows local API
        public const string ApplicationName = "yawindev";
        public const string YavscHomeUrl = "http://192.168.0.29:5000";
#endif
#if YAVSC
        public const string ApplicationName = "yavsc";
        public const string YavscHomeUrl = "https://yavsc.pschneider.fr";
#endif
#if LUA
        public const string ApplicationName = "Luap";
        public const string YavscHomeUrl = "https://lua.pschneider.fr";
#endif
#if ZICMOOVE
        public const string ApplicationName = "ZicMoove";
        public const string YavscHomeUrl = "https://zicmoove.pschneider.fr";
#endif
        public static readonly string AuthorizeUrl = YavscHomeUrl + "/authorize";
        public static readonly string RedirectUrl = YavscHomeUrl + "/oauth/success";
        public static readonly string AccessTokenUrl = YavscHomeUrl + "/token";

        public static readonly string YavscApiUrl = YavscHomeUrl + "/api";
        public static readonly string MobileRegistrationUrl = YavscApiUrl + "/gcm/register";
        public static readonly string UserInfoUrl = YavscApiUrl + "/me";
        public static readonly string BlogUrl = YavscApiUrl + "/blogs";
        public static readonly string FsUrl = YavscApiUrl + "/fs";
        public static readonly string SignalRHubsUrl = YavscHomeUrl + "/api/signalr/hubs";

        public static int AllowBeATarget = 1;

        public static int CloudTimeout = 400;
    }
}
