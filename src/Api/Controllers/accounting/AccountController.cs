using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

using Yavsc.Models;
using Yavsc.Models.Account;
using Yavsc.ViewModels.Account;
using Yavsc.Helpers;
using Yavsc.Abstract.Identity;
using System.Diagnostics;

namespace Yavsc.WebApi.Controllers
{
    [Route("~/api/account")]
    [Authorize("ApiScope")]
    public class ApiAccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ApiAccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            this.roleManager = roleManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger(nameof(ApiAccountController));
            _dbContext = dbContext;
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

        private readonly RoleManager<IdentityRole> roleManager;

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
                    AddErrors("NewPassword",result);
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
                    AddErrors ("NewPassword",result);
                    return new BadRequestObjectResult(ModelState);
                }
            }
            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
		public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                AddErrors ("Register",result);
                return new BadRequestObjectResult(ModelState);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok();
        }
        private void AddErrors(string key, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(key, error.Description);
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

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User==null) 
            return new BadRequestObjectResult(
                    new { error = "user not found" });
            var uid = User.GetUserId();
            
            var userData = await _dbContext.Users
                .Include(u=>u.PostalAddress)
                .Include(u=>u.AccountBalance)
                .FirstAsync(u=>u.Id == uid);

            var user = new Yavsc.Models.Auth.Me(userData.Id, userData.UserName, userData.Email, 
            userData.Avatar , 
            userData.PostalAddress, userData.DedicatedGoogleCalendar );

            var userRoles = _dbContext.UserRoles.Where(u=>u.UserId == uid).Select(r => r.RoleId).ToArray();

            IdentityRole [] roles = _dbContext.Roles.Where(r=>userRoles.Contains(r.Id)).ToArray();
            
            user.Roles = roles.Select(r=>r.Name).ToArray();

            return Ok(user);
        }

        [HttpGet("myhost")]
        public IActionResult MyHost ()
        {
            return Ok(new { host = Request.ForHost() });
        }

        /// <summary>
        /// Actually only updates the user's name.
        /// </summary>
        /// <param name="me">MyUpdate containing the new user name </param>
        /// <returns>Ok when all is ok.</returns>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UserInfo me)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(
                    new { error = "Specify some valid user update request." });
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            var result = await _userManager.SetUserNameAsync(user, me.UserName);
            if (result.Succeeded)
                return Ok();
            else return new BadRequestObjectResult(result);
        }
        /// <summary>
        /// Updates the avatar
        /// </summary>
        /// <returns></returns>
        [HttpPost("~/api/setavatar")]
        public async Task<IActionResult> SetAvatar()
        {
            var root = User.InitPostToFileSystem(null);
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            var info = user.ReceiveAvatar(Request.Form.Files[0]);
            await _userManager.UpdateAsync(user);
            return Ok(info);
        }

        [HttpGet("identity")]
        public async Task<IActionResult> Identity()
        {
            return Json(User.Claims.Select(c=>new {c.Type, c.Value}));
        }
    }
}
