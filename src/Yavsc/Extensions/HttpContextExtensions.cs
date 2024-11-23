
using System.Security.Claims;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Access;

namespace Yavsc.Extensions;

internal static class HttpContextExtensions
{
    public static async Task SignInAsync(this HttpContext context, 
        ApplicationUser user, RoleManager<IdentityRole> roleManager,
        bool rememberMe,
        ApplicationDbContext applicationDbContext)
        {
 AuthenticationProperties props = null;
            if (AccountOptions.AllowRememberLogin && rememberMe)
            {
                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration),
                    //   Parameters = 
                };
            };

            // roles
            var roles = applicationDbContext.UserRoles.Where(r => r.UserId == user.Id).ToArray();

            // issue authentication cookie with subject ID and username

            List<Claim> additionalClaims = new List<Claim>();

            foreach (var role in roles)
            {
                var idRole = await roleManager.Roles.SingleOrDefaultAsync(i => i.Id == role.RoleId);
                if (idRole != null)
                {
                    additionalClaims.Add(new Claim(ClaimTypes.Role, idRole.Name));
                }
            }
            additionalClaims.Add(new Claim(ClaimTypes.Name, user.UserName));
            var isUser = new IdentityServerUser(user.Id)
            {
                DisplayName = user.UserName,
                AdditionalClaims = additionalClaims.ToArray()
            };

            await context.SignInAsync(isUser, props);
        }
}
