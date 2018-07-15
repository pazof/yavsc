﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Yavsc.WebApi.Controllers
{
    using Models;
    using Models.Account;
    using ViewModels.Account;
    using Models.Auth;
    using Yavsc.Helpers;
    using System.Linq;
    using Microsoft.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;

    [Authorize(),Route("~/api/account")]
    public class ApiAccountController : Controller
    {
        
        private UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        ApplicationDbContext _dbContext;
        private ILogger _logger;

        public ApiAccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger("ApiAuth");
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

        // POST api/Account/ChangePassword
        [Authorize]
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
        [Authorize]
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

        [HttpGet("~/api/me"),Authorize]
        public async Task<IActionResult> Me ()
        {
            if (User==null) 
            return new BadRequestObjectResult(
                    new { error = "user not found" });
            var uid = User.GetUserId();

            var userData = await _dbContext.Users
                .Include(u=>u.PostalAddress)
                .Include(u=>u.AccountBalance)
                .Include(u=>u.Roles)
                .FirstAsync(u=>u.Id == uid);

            var user = new Me(userData.Id, userData.UserName, userData.Email, 
            userData.Avatar , 
            userData.PostalAddress, userData.DedicatedGoogleCalendar );

            var userRoles = _dbContext.UserRoles.Where(u=>u.UserId == uid).ToArray();

            IdentityRole [] roles = _dbContext.Roles.Where(r=>userRoles.Any(ur=>ur.RoleId==r.Id)).ToArray();
            
            user.Roles = roles.Select(r=>r.Name).ToArray();

            return Ok(user);
        }

        [HttpGet("~/api/myip"),Authorize]
        public IActionResult MyIp ()
        {
            string ip = null;

            ip = Request.Headers["X-Forwarded-For"];

            if (string.IsNullOrEmpty(ip)) {
                ip = Request.Host.Value;
            } else { // Using X-Forwarded-For last address
                ip = ip.Split(',')
                    .Last()
                    .Trim();
            }

            return Ok(ip);
        }

        /// <summary>
        /// Actually only updates the user's name.
        /// </summary>
        /// <param name="me">MyUpdate containing the new user name </param>
        /// <returns>Ok when all is ok.</returns>
        [HttpPut("~/api/me"),Authorize]
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
        [HttpPost("~/api/setavatar"),Authorize]
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
    }
}
