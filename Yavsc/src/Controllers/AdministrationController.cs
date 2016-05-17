
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)), Authorize()]
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdministrationController(UserManager<ApplicationUser> userManager,
         RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
            if (admins != null && admins.Count > 0) return HttpNotFound();
            var user = await _userManager.FindByIdAsync(User.GetUserId());

            IdentityRole adminRole;
            if (!await _roleManager.RoleExistsAsync(Constants.AdminGroupName))
            {
                adminRole = new IdentityRole { Name = Constants.AdminGroupName };
                var resultCreate = await _roleManager.CreateAsync(adminRole);
                if (!resultCreate.Succeeded)
                {
                    AddErrors(resultCreate);
                    return new BadRequestObjectResult(ModelState);
                }
            }
            else adminRole = await _roleManager.FindByNameAsync(Constants.AdminGroupName);
            var addToRoleResult = await _userManager.AddToRoleAsync(user, Constants.AdminGroupName);
            if (!addToRoleResult.Succeeded)
            {
                AddErrors(addToRoleResult);
                return new BadRequestObjectResult(ModelState);
            }
            return Ok(new {message="you owned it."});
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
