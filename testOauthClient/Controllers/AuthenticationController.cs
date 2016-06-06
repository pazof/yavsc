
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;

namespace Mvc.Client.Controllers {
    public class AuthenticationController : Controller {
        
        [HttpGet("~/signin")]
        public ActionResult SignIn() {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            var properties = new AuthenticationProperties { RedirectUri = "http://localhost:5002/signin-yavsc" };
            return new ChallengeResult("yavsc", properties);
        }

    }
}