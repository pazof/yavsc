namespace Yavsc.Server
{
    using Microsoft.AspNet.Http;
    using Yavsc.Models.Auth;

    public static class ServerConstants
    {

        public const string ApplicationName = "Yavsc";
        public const string CompanyInfoUrl = " https://societeinfo.com/app/rest/api/v1/company/json?registration_number={0}&key={1}";


        private static readonly string[] GoogleScopes = { "openid", "profile", "email" };
        public static readonly string[] GoogleCalendarScopes =
        { "openid", "profile", "email", "https://www.googleapis.com/auth/calendar" };


    }
}
