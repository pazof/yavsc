
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;

namespace Mvc.Client.Controllers {
    
    public class AuthenticationController : Controller {
        
        [HttpGet("~/signin")]
        public ActionResult SignIn(string returnUrl) {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, properties);
        }

    }
}