

namespace  Yavsc
{
    /// <summary>
    /// OAuth2 client application sensitive settings.
    /// </summary>
    public class OAuth2AppSettings {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    /// <summary>
    /// Facebook's class, so class
    /// </summary>
    public class FacebookOAuth2AppSettings : OAuth2AppSettings  {
    }

}


