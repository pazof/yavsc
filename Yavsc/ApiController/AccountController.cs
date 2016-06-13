using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;
using Yavsc.Models.Account;
using Microsoft.AspNet.Mvc;
using Yavsc.ViewModels.Account;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Yavsc.Models.Auth;

namespace Yavsc.WebApi.Controllers
{

    [Authorize,Route("~/api/account")]
    public class ApiAccountController : Controller
    {
        
        private UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private ILogger _logger;

        public ApiAccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory)
        {
            UserManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger("ApiAuth");
        }

        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager;
            }
            private set
            {
                _userManager = value;
            }
        }

        // POST api/Account/ChangePassword
		public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) {
                IdentityResult result = await UserManager.ChangePasswordAsync(user, model.OldPassword,
                    model.NewPassword);

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return new BadRequestObjectResult(ModelState);
                }
            }
            return Ok();
        }

        // POST api/Account/SetPassword
		public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) {
                IdentityResult result = await UserManager.AddPasswordAsync(user, model.NewPassword);
                if (!result.Succeeded)
                {
                    AddErrors (result);
                    return new BadRequestObjectResult(ModelState);
                }
            }
            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                AddErrors (result);
                return new BadRequestObjectResult(ModelState);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok();
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
            }

            base.Dispose(disposing);
        }

        [HttpGet("~/api/me"),Produces("application/json")]
        public async Task<IActionResult> Me ()
        {
            if (User==null) 
            return new BadRequestObjectResult(
                    new { error = "user not found" });
            var uid = User.GetUserId();
            if (uid == null) 
                return new BadRequestObjectResult(
                    new { error = "user not identified" });
            return Ok(new Me(await UserManager.FindByIdAsync(uid)));
        }

    }
}
