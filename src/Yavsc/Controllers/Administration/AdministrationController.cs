
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Identity;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.ViewModels;
using Yavsc.ViewModels.Administration;

namespace Yavsc.Controllers
{
    [Authorize()]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _dbContext;

        public AdministrationController(UserManager<ApplicationUser> userManager,
         RoleManager<IdentityRole> roleManager,
         ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this._dbContext = context;
        }

        private async Task<bool> EnsureRoleList () {
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
                // All is ok, nothing to do here.
                if (User.IsInRole(Constants.AdminGroupName))
                {
                    
                    return Ok(new { message = "you already got it." });
                }
                return NotFound();
            }
            
            var user = await _userManager.FindByIdAsync(User.GetUserId());
            // check all user groups exist
            if (!await EnsureRoleList()) {
                ModelState.AddModelError(null, "Could not ensure role list existence. aborting.");
                return new BadRequestObjectResult(ModelState);
            }

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
            var userCount = await _dbContext.Users.CountAsync();
            var youAreAdmin = await _userManager.IsInRoleAsync(
                await _userManager.FindByIdAsync(User.GetUserId()),
                Constants.AdminGroupName);
           
            var roles = await _roleManager.Roles.Select(x => new RoleInfo {
                Id = x.Id,
                Name = x.Name            
                }).ToArrayAsync();
            foreach (var role in roles)
            {
                var uinrole = await _userManager.GetUsersInRoleAsync(role.Name);

                role.UserCount = uinrole.Count();
            }
            var assembly = GetType().Assembly;
           ViewBag.ThisAssembly = assembly.FullName;
           ViewBag.RunTimeVersion = assembly.ImageRuntimeVersion;
           var rolesArray = roles.ToArray();
            return View(new AdminViewModel
            {
                Roles = rolesArray,
                AdminCount = adminCount.Count,
                YouAreAdmin = youAreAdmin,
                UserCount = userCount
            });
        }

        [Authorize("AdministratorOnly")]
        public IActionResult Enroll(string roleName)
        {
            ViewBag.UserId = new SelectList(_dbContext.Users, "Id", "UserName");
            return View(new EnrolerViewModel{ RoleName = roleName });
        }

        [Authorize("AdministratorOnly")]
        [HttpPost()]
        public async Task<IActionResult> Enroll(EnrolerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newAdmin  = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id==model.EnroledUserId);
                if (newAdmin==null) return NotFound();
                var addToRoleResult = await _userManager.AddToRoleAsync(newAdmin, model.RoleName);
                if (addToRoleResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(addToRoleResult);
            }
            ViewBag.UserId = new SelectList(_dbContext.Users, "Id", "UserName");
            return View(model);
        }

        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> Fire(string roleName, string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id==userId);
            if (user == null) return NotFound();

            return View(new FireViewModel{ RoleName = roleName, EnroledUserId = userId, EnroledUserName = user.UserName });
        }

        [Authorize("AdministratorOnly")]
        [HttpPost()]
        public async Task<IActionResult> Fire(FireViewModel model)
        {
            if (ModelState.IsValid)
            {
                var oldEnroled  = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id==model.EnroledUserId);
                if (oldEnroled==null) return NotFound();
                var removeFromRole = await _userManager.RemoveFromRoleAsync(oldEnroled, model.RoleName);
                if (removeFromRole.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(removeFromRole);
            }
            ViewBag.UserId = new SelectList(_dbContext.Users, "Id", "UserName");
            return View(model);
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
