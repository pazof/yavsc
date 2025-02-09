
using IdentityServer8;
using IdentityServer8.Models;
using Yavsc.Settings;

namespace Yavsc;

public static class Config
{
        public static string Authority { get;  set; }

        public static IConfigurationRoot? GoogleWebClientConfiguration { get;  set; }
        public static GoogleServiceAccount? GServiceAccount { get;  set; }

        public static SiteSettings SiteSetup { get;  set; }
        public static FileServerOptions UserFilesOptions { get; set; }
        public static FileServerOptions GitOptions { get; set; }
        public static string AvatarsDirName {  set; get; }
        public static string GitDirName {  set; get; }

    public static GoogleAuthSettings GoogleSettings { get;  set; }
    public static SmtpSettings SmtpSetup { get;  set; }
    public static string Temp { get;  set; }
        public static FileServerOptions AvatarsOptions { get; set; }
        public static string UserBillsDirName {  set; get; }
        public static string UserFilesDirName {  set; get; }



        /// <summary>
        /// Lists Available user profile classes,
        /// populated at startup, using reflexion.
        /// </summary>
        public static List<Type> ProfileTypes = new List<Type>();


    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "mvc",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5003/signin-oidc",
                "http://localhost:5002/signin-oidc"  },
                PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email }
            },
        };

    public static PayPalSettings PayPalSettings { get; set; }
}
