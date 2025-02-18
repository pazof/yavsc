using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Yavsc.Models;
using Yavsc.Api.Helpers;
using Yavsc.Server.Helpers;

namespace Yavsc.WebApi.Controllers
{
    [Route("~/api/account")]
    [Authorize("ApiScope")]
    public class ApiAccountController : Controller
    {
        readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ApiAccountController(
        ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger(nameof(ApiAccountController));
            _dbContext = dbContext;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User == null)
                return new BadRequestObjectResult(
                        new { error = "user not found" });
            var uid = User.GetUserId();

            var userData = await GetUserData(uid);

            var user = new Yavsc.Models.Auth.Me(userData.Id, userData.UserName, userData.Email,
            userData.Avatar,
            userData.PostalAddress, userData.DedicatedGoogleCalendar);

            var userRoles = _dbContext.UserRoles.Where(u => u.UserId == uid).Select(r => r.RoleId).ToArray();

            IdentityRole[] roles = _dbContext.Roles.Where(r => userRoles.Contains(r.Id)).ToArray();

            user.Roles = roles.Select(r => r.Name).ToArray();

            return Ok(user);
        }

        private async Task<ApplicationUser> GetUserData(string uid)
        {
            return await _dbContext.Users
                            .Include(u => u.PostalAddress)
                            .Include(u => u.AccountBalance)
                            .FirstAsync(u => u.Id == uid);
        }

        [HttpGet("myhost")]
        public IActionResult MyHost ()
        {
            return Ok(new { host = Request.ForHost() });
        }

      
        /// <summary>
        /// Updates the avatar
        /// </summary>
        /// <returns></returns>
        [HttpPost("~/api/setavatar")]
        public async Task<IActionResult> SetAvatar()
        {
            var root = User.InitPostToFileSystem(null);
            var user =  await GetUserData(User.GetUserId());
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            var info = user.ReceiveAvatar(Request.Form.Files[0]);
            await _dbContext.SaveChangesAsync();
            return Ok(info);
        }

        [HttpGet("identity")]
        public async Task<IActionResult> Identity()
        {
            return Json(User.Claims.Select(c=>new {c.Type, c.Value}));
        }
    }
}
