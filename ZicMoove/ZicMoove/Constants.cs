﻿using System;
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