using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using Yavsc.Models;
    using Yavsc.Models.Auth;
    using Yavsc.Helpers;
    using System.Linq;
    using Microsoft.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;

namespace Yavsc.ApiControllers.accounting
{
    [Route("~/api/profile")]
    public class ProfileApiController: Controller
    {
        UserManager<ApplicationUser> _userManager;
        ApplicationDbContext _dbContext;
        public ProfileApiController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet("memail/{allow}")]
        public async Task<object> SetMonthlyEmail(bool allow)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            user.AllowMonthlyEmail = allow;
            _dbContext.SaveChanges(User.GetUserId());
            return Ok(new { monthlyEmailPrefSaved = allow });
        }
    }
}