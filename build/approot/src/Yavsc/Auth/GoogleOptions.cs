using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http;

namespace Yavsc.Auth
{
    public static class YavscGoogleDefaults
    {
        public const string AuthenticationScheme = "Google";

        public static readonly string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";

        public static readonly string TokenEndpoint = "https://www.googleapis.com/oauth2/v3/token";

        public static readonly string UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me";
    }

    /// <summary>
    /// Configuration options for <see cref="GoogleMiddleware"/>.
    /// </summary>
    public class YavscGoogleOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="YavscGoogleOptions"/>.
        /// </summary>
        public YavscGoogleOptions()
        {
            AuthenticationScheme = YavscGoogleDefaults.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-google");
            AuthorizationEndpoint = YavscGoogleDefaults.AuthorizationEndpoint;
            TokenEndpoint = YavscGoogleDefaults.TokenEndpoint;
            UserInformationEndpoint = YavscGoogleDefaults.UserInformationEndpoint;
            Scope.Add("openid");
            Scope.Add("profile");
            Scope.Add("email");
        }

        /// <summary>
        /// access_type. Set to 'offline' to request a refresh token.
        /// </summary>
        public string AccessType { get; set; }

    }
}
