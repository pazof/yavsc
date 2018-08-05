namespace cli
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;

    public class ConnectionSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string SiteAccessSheme { get; set; } = "http";
        public string Scope { get; set; } = "profile";

        [NotMapped]
        [JsonIgnore]
        public string AuthorizeUrl {get {
            return $"{SiteAccessSheme}://{Authority}/authorize";
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string RedirectUrl  {get {
            return $"{SiteAccessSheme}://{Authority}/oauth/success";
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string AccessTokenUrl  {get {
            return $"{SiteAccessSheme}://{Authority}/token";
        } }
        
    }
}