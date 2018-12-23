

namespace Yavsc
{
    public class GoogleAuthSettings
    {
        public string ApiKey { get; set; }
        public string BrowserApiKey { get; set; }
        public class Account
        {
            public string type { get; set; }
            public string project_id { get; set; }
            public string private_key_id { get; set; }
            public string private_key { get; set; }
            public string client_email { get; set; }
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string auth_uri { get; set; }
            public string token_uri { get; set; }
            public string auth_provider_x509_cert_url { get; set; }
            public string client_x509_cert_url { get; set; }

        } 
        public Account ServiceAccount { get; set; }
    }
}
