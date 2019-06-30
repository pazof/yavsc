namespace Yavsc
{
    using Microsoft.AspNet.Http;
    using Yavsc.Models.Auth;

    public static class Constants
    {
        public const string ApplicationName = "Yavsc",
            CompanyClaimType = "https://schemas.pschneider.fr/identity/claims/Company",
            UserNameRegExp = @"^[a-zA-Z][a-zA-Z0-9._-]*$",
            AuthorizePath = "~/authorize",
        	TokenPath = "~/token",
            LoginPath = "~/signin",
            LogoutPath = "~/signout", UserInfoPath = "~/api/me",

            SignalRPath = "/api/signalr",
            LiveUserPath = "live",

            ApplicationAuthenticationSheme = "ServerCookie",
            ExternalAuthenticationSheme= "ExternalCookie",
            CompanyInfoUrl = " https://societeinfo.com/app/rest/api/v1/company/json?registration_number={0}&key={1}",
        	DefaultFactor =  "Default",
            MobileAppFactor =  "Mobile Application",
            EMailFactor = "Email",
            SMSFactor = "SMS",
            AdminGroupName = "Administrator",
            PerformerGroupName = "Performer",
            StarGroupName = "Star",
            StarHunterGroupName = "StarHunter",
            BlogModeratorGroupName = "Moderator",
            FrontOfficeGroupName = "FrontOffice",
            GCMNotificationUrl = "https://gcm-http.googleapis.com/gcm/send",
            UserFilesPath = "/files",
            AvatarsPath = "/avatars",
            GitPath = "/sources",
            DefaultAvatar = "/images/Users/icon_user.png",
            AnonAvatar = "/images/Users/icon_anon_user.png",
            YavscConnectionStringEnvName = "YAVSC_DB_CONNECTION";

        // at the end, let 4*4 bytes in peace
        public const int WebSocketsMaxBufLen = 4*1020;

        public static readonly long DefaultFSQ = 1024*1024*500;

        public static readonly Scope[] SiteScopes = { 
            new Scope { Id = "profile", Description = "Your profile informations" },  
            new Scope { Id = "book" , Description ="Your booking interface"},  
            new Scope { Id = "blog" , Description ="Your blogging interface"},  
            new Scope { Id = "estimate" , Description ="Your estimation interface"},  
            new Scope { Id = "contract" , Description ="Your contract signature access"}, 
            new Scope { Id = "admin" , Description ="Your administration rights on this site"}, 
            new Scope { Id = "moderation" , Description ="Your moderator interface"}, 
            new Scope { Id = "frontoffice" , Description ="Your front office interface" }
        };

        public const string SshHeaderKey = "SSH";

        private static readonly string[] GoogleScopes = { "openid", "profile", "email" };
        public static readonly string[] GoogleCalendarScopes =
        { "openid", "profile", "email", "https://www.googleapis.com/auth/calendar" };

        public static readonly string NoneCode = "none";

        public const int MaxUserNameLength = 26;

        public const string LivePath = "/live/cast";
    }
}
