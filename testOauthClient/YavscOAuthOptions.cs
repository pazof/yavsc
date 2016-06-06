

using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http;

public class YavscOAuthOptions : OAuthOptions {
    public YavscOAuthOptions()
        {
            AuthenticationScheme = "yavsc";
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-yavsc");
            AuthorizationEndpoint = "http://dev.pschneider.fr/connect/authorize";
            TokenEndpoint = "http://dev.pschneider.fr/api/token/get";
            UserInformationEndpoint = "http://dev.pschneider.fr/api/userinfo";
            Scope.Add("openid");
            Scope.Add("profile");
            Scope.Add("email");
        }

        /// <summary>
        /// access_type. Set to 'offline' to request a refresh token.
        /// </summary>
        public string AccessType { get; set; }
}