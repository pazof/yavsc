namespace Yavsc
{
    using Yavsc.Models.Auth;

     public static class Constants
    {
        public const string RememberMeCookieName = "Berme";
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

        public const string CompanyInfoUrl = " https://societeinfo.com/app/rest/api/v1/company/json?registration_number={0}&key={1}";
        public const string DefaultFactor =  "Default";
        public const string MobileAppFactor =  "Google.clood";
        public const string EMailFactor = "Email";
        public const string SMSFactor = "Phone";
        public const string AdminGroupName = "Administrator";
        public const string BlogModeratorGroupName = "Moderator";
        public const string FrontOfficeGroupName = "FrontOffice";
        public const string UserBillsFilesDir= "Bills";
        public const string UserFilesDir = "UserFiles";

        public const string GCMNotificationUrl = "https://gcm-http.googleapis.com/gcm/send";
        private static readonly string[] GoogleScopes = { "openid", "profile", "email" };

        public static readonly string[] GoogleCalendarScopes =
        { "openid", "profile", "email", "https://www.googleapis.com/auth/calendar" };
        public const string ApplicationName = "Yavsc";

        public const string Issuer = "https://dev.pschneider.fr";

        public const string CompanyClaimType = "https://schemas.pschneider.fr/identity/claims/Company";

        public const string UserNameRegExp = @"^[a-zA-Z][a-zA-Z0-9 ]*$";

        public const string AuthenticationEndPath = "/signin";
        public const string TokenEndPath = "/token";

        public const string KeyProtectorPurpose = "OAuth.AspNet.AuthServer"; 

    }
}
