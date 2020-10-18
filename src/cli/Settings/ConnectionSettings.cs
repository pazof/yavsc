namespace cli
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Yavsc;

    public class ConnectionSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string SiteAccessSheme { get; set; } = "http";
        public int Port { get; set; }
        public string Scope { get; set; } = "profile";

        [NotMapped]
        [JsonIgnore]
        public string AuthorizeUrl {get {
            return Port==0 ? $"{SiteAccessSheme}://{Authority}/authorize" :
            $"{SiteAccessSheme}://{Authority}:{Port}/authorize" ;
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string RedirectUrl  {get {
            return Port==0 ? $"{SiteAccessSheme}://{Authority}/oauth/success" :
            $"{SiteAccessSheme}://{Authority}:{Port}/oauth/success" ;
        } }
        
        [NotMapped]
        [JsonIgnore]
        public string AccessTokenUrl  { get {
            return Port==0 ? $"{SiteAccessSheme}://{Authority}/token":
            $"{SiteAccessSheme}://{Authority}:{Port}/token";
        } }

        [NotMapped]
        [JsonIgnore]
        public string StreamingUrl { get {
            return Port==0 ? $"ws://{Authority}"+Constants.LivePath:
            $"ws://{Authority}:{Port}"+Constants.LivePath;
        } }
    }
}
