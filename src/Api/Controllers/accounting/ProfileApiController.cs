using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Yavsc.Models;
using Yavsc.Abstract.Identity;
using Yavsc.Helpers;

namespace Yavsc.ApiControllers.accounting
{
    [Route("~/api/profile")]
    public class ProfileApiController: Controller
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly ApplicationDbContext _dbContext;
        public ProfileApiController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet("{allow}",Name ="setmonthlyemail")]
        public async Task<object> SetMonthlyEmail(bool allow)
        {
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            user.AllowMonthlyEmail = allow;
            _dbContext.SaveChanges(User.GetUserId());
            return Ok(new { monthlyEmailPrefSaved = allow });
        }

        [HttpGet("userhint/{name}")]
        public UserInfo[] GetUserHint(string name)
        {
            return _dbContext.Users.Where(u=>u.UserName.IndexOf(name)>0)
            .Select(u=>new UserInfo(u.Id, u.UserName, u.Avatar))
            .Take(10).ToArray();
        }
    }
}
