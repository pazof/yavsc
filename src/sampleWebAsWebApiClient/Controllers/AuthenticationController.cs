
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Client.Controllers {
    public class AuthenticationController : Controller {
        
        [HttpGet("~/signin")]
        public ActionResult SignIn(string returnUrl="/") {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs.
            // But, this redirect URI doesn't need to match the OAuth parameter, it's serialized in the query state,
            // to be used once the identification ends.
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return new ChallengeResult("Yavsc", properties);
        }
        [HttpGet("~/signout")]
        public async Task<IActionResult> SignOut(string returnUrl="/") {
            await HttpContext.Authentication.SignOutAsync("Bearer");
            return Redirect(returnUrl);
        }


    }
}
