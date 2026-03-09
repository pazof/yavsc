namespace Yavsc.Server
{

    public static class ServerConstants
    {
        public const string CompanyInfoUrlFormat = " https://societeinfo.com/app/rest/api/v1/company/json?registration_number={0}&key={1}";

        public static readonly string[] GoogleCalendarScopes =
        { "openid", "profile", "email", "https://www.googleapis.com/auth/calendar" };


    }
}
