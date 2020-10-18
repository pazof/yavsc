using Yavsc.Models.Auth;

namespace Yavsc
{

    public static class Constants
    {
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

        public const string CompanyClaimType = "https://schemas.pschneider.fr/identity/claims/Company";
        public const string UserNameRegExp = @"^[a-zA-Z][a-zA-Z0-9._-]*$";
        public const string UserFileNamePatternRegExp = @"^([a-zA-Z0-9._-]*/)*[a-zA-Z0-9._-]+$";
        public const string AuthorizePath = "~/authorize";
        public const string TokenPath = "~/token";
        public const string LoginPath = "~/signin";
        public const string LogoutPath = "~/signout";


        public const string SignalRPath = "/api/signalr";
        public const string UserFilesPath = "/files";
        public const string AvatarsPath = "/avatars";
        public const string GitPath = "/sources";
        public const string LiveUserPath = "live";

        public const string ApplicationAuthenticationSheme = "ServerCookie";
        public const string ExternalAuthenticationSheme = "ExternalCookie";
        public const string DefaultFactor = "Default";
        public const string MobileAppFactor = "Mobile Application";
        public const string EMailFactor = "Email";
        public const string SMSFactor = "SMS";
        public const string AdminGroupName = "Administrator";
        public const string PerformerGroupName = "Performer";
        public const string StarGroupName = "Star";
        public const string StarHunterGroupName = "StarHunter";
        public const string BlogModeratorGroupName = "Moderator";
        public const string FrontOfficeGroupName = "FrontOffice";
        public const string DefaultAvatar = "/images/Users/icon_user.png";
        public const string AnonAvatar = "/images/Users/icon_anon_user.png";
        public const string YavscConnectionStringEnvName = "YAVSC_DB_CONNECTION";

        // at the end, let 4*4 bytes in peace
        public const int WebSocketsMaxBufLen = 4096;

        public static readonly long DefaultFSQ = 1024 * 1024 * 500;


        public const string SshHeaderKey = "SSH";

        public static readonly string NoneCode = "none";

        public const int MaxUserNameLength = 26;

        public const string LivePath = "/live/cast";
        
        public const string StreamingPath = "/api/stream/Put";
    }
}
