

using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Yavsc
{
    public class AuthorisationView { 
        public OpenIdConnectMessage Message { get; set; }
        public Application Application { get; set; }
        
    }
}