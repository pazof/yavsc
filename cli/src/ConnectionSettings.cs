namespace cli
{
    using Yavsc.Abstract.Identity;
    using Yavsc;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;
    using Yavsc.Authentication;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ConnectionSettings
    {
        public TokenInfo UserToken { get; set; } 
        public SiteSettings Site { get; set; }
        public OAuth2AppSettings ServerApiKey { get; set; }
        public string SiteAccessSheme { get; set; } = "http";
        public string Scope { get; set; } = "profile";

        [NotMapped]
        [JsonIgnore]
        public string AuthorizeUrl {get {
            return $"{SiteAccessSheme}://{Site.Authority}/authorize";
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string RedirectUrl  {get {
            return $"{SiteAccessSheme}://{Site.Authority}/oauth/success";
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string AccessTokenUrl  {get {
            return $"{SiteAccessSheme}://{Site.Authority}/token";
        } }

        public async Task InitUserTokenFromLoginPass(string login, string pass)
        {
           var oauthor =new OAuthenticator( ServerApiKey.ClientId,  ServerApiKey.ClientSecret, Scope, 
           new Uri( AuthorizeUrl) , new Uri(RedirectUrl) , new Uri(AccessTokenUrl));
           var query = new Dictionary<string,string>();
           query["username"]=login;
           query["password"]=pass;
           query["grant_type"]="password";
           var result = await oauthor.RequestAccessTokenAsync(query);
           UserToken = new TokenInfo {
                AccessToken = result["access_token"],
                RefreshToken = result["refresh_token"],
                Received = DateTime.Now,
                ExpiresIn = int.Parse(result["expires_in"]),
                TokenType = result["token_type"]
                 };
        }
    }
}