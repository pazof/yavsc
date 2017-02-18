using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove
{
    public static partial class Constants
    {

        public static readonly string AuthorizeUrl = YavscHomeUrl + "/authorize";
        public static readonly string AccessTokenUrl = YavscHomeUrl + "/token";
        public const string RedirectUrl = YavscHomeUrl + "/oauth/success";
        public static readonly string Scope = "profile";

        public static readonly string YavscApiUrl = YavscHomeUrl + "/api";
        public static readonly string MobileRegistrationUrl = YavscApiUrl + "/gcm/register";
        public static readonly string UserInfoUrl = YavscApiUrl + "/me";
        public static readonly string BlogUrl = YavscApiUrl + "/blogs";
        public static readonly string FsUrl = YavscApiUrl + "/fs";
        public static readonly string SignalRHubsUrl = YavscHomeUrl + "/api/signalr/hubs";

        public static int AllowBeATarget = 1;

        public static int CloudTimeout = 400;

        public  const string PermissionMapReceive = Constants.ApplicationName + ".permission.MAPS_RECEIVE";
        public  const string PermissionC2DMessage = Constants.ApplicationName + ".permission.C2D_MESSAGE";
        
    }
}
