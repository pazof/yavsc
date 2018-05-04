
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Auth;
using Yavsc.ViewModels.Administration;

namespace Yavsc.Controllers
{
    [Authorize()]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext context;

        public AdministrationController(UserManager<ApplicationUser> userManager,
         RoleManager<IdentityRole> roleManager,
         ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this.context = context;
        }

        private async Task<bool> CreateRoles () {
            // ensure all roles existence
            foreach (string roleName in new string[] {
                Constants.AdminGroupName,
                Constants.StarGroupName, 
                Constants.PerformerGroupName,
                Constants.FrontOfficeGroupName,
                Constants.StarHunterGroupName,
                Constants.BlogModeratorGroupName
                })
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole { Name = roleName };
                    var resultCreate = await _roleManager.CreateAsync(role);
                    if (!resultCreate.Succeeded)
                    {
                        AddErrors(resultCreate);
                        return false;
                    }
                }
                return true;
        
        }
        /// <summary>
        /// Gives the (new if was not existing) administrator role
        /// to current authenticated user, when no existing
        /// administrator was found.
        /// When nothing is to do, it returns a 404.
        /// </summary>
        /// <returns></returns>
        [Produces("application/json")]
        public async Task<IActionResult> Take()
        {
            // If some amdin already exists, make this method disapear
            var admins = await _userManager.GetUsersInRoleAsync(Constants.AdminGroupName);
            if (admins != null && admins.Count > 0) 
            {
                if (User.IsInRole(Constants.AdminGroupName))
                {
                    // check all user groups exist
                    if (!await CreateRoles())
                        return new BadRequestObjectResult(ModelState);
                    return Ok(new { message = "you checked the role list." });
                }
                return HttpNotFound();
            }
            
            var user = await _userManager.FindByIdAsync(User.GetUserId());

            IdentityRole adminRole;
            adminRole = await _roleManager.FindByNameAsync(Constants.AdminGroupName);
            var addToRoleResult = await _userManager.AddToRoleAsync(user, Constants.AdminGroupName);
            if (!addToRoleResult.Succeeded)
            {
                AddErrors(addToRoleResult);
                return new BadRequestObjectResult(ModelState);
            }

            return Ok(new { message = "you owned it." });
        }

        [Authorize(Roles = Constants.AdminGroupName)]
        [Produces("application/json")]
        public async Task<IActionResult> Index()
        {
            var adminCount = await _userManager.GetUsersInRoleAsync(
                Constants.AdminGroupName);
            var userCount = await context.Users.CountAsync();
            var youAreAdmin = await _userManager.IsInRoleAsync(
                await _userManager.FindByIdAsync(User.GetUserId()),
                Constants.AdminGroupName);
            var roles = _roleManager.Roles.Include(
                x => x.Users
            ).Select(x => new RoleInfo {
                Id = x.Id,
                Name = x.Name,
                Users = x.Users.Select(u=>u.UserId).ToArray()
            });
            var assembly = GetType().Assembly;
           ViewBag.ThisAssembly = assembly.FullName;
           ViewBag.RunTimeVersion = assembly.ImageRuntimeVersion;
            ViewBag.HostContextFullName = Startup.HostingFullName;
            return View(new AdminViewModel
            {
                Roles = roles.ToArray(),
                AdminCount = adminCount.Count,
                YouAreAdmin = youAreAdmin,
                UserCount = userCount
            });
        }

        public IActionResult Role(string id)
        {
            IdentityRole role = _roleManager.Roles
            .Include(r=>r.Users).FirstOrDefault
                ( r=> r.Id == id );
            var ri = GetRoleUserCollection(role);
            return View("Role",ri);
        }

        public RoleUserCollection GetRoleUserCollection(IdentityRole role)
        {
            var result = new RoleUserCollection {
                Id = role.Id,
                Name = role.Name,
                Users = context.Users.Where(u=>role.Users.Any(ru => u.Id == ru.UserId))
                .Select( u => new UserInfo { UserName = u.UserName, Avatar = u.Avatar, UserId = u.Id } )
                .ToArray()
            };
            return result;
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
