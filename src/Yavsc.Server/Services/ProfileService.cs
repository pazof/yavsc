#nullable enable annotations

using System.Security.Claims;
using IdentityModel;
using IdentityServer8.Models;
using IdentityServer8.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileService(
            UserManager<ApplicationUser> userManager,
            ILogger<DefaultProfileService> logger)
        {
            _userManager = userManager;
        }

        private async Task<List<Claim>> GetClaimsFromUserAsync(
            ProfileDataRequestContext context,
            ApplicationUser user)
        {

            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
            };

            foreach (var scope in context.RequestedResources.ParsedScopes)
            {
                if (context.Client.AllowedScopes.Contains(scope.ParsedName))
                {
                    claims.Add(new Claim(JwtClaimTypes.Scope, scope.ParsedName));
                }
            }

            // name / email / roles are gated on the client being granted
            // the "profile" identity scope. They MUST land in the *access
            // token*: the Blogs/Api bearer resource hosts read the login
            // via GetUserName() (FindFirstValue("name")) and roles via
            // IsInMsRole() from the access token. But "profile" is an
            // identity scope, which only appears in the id-token caller's
            // ParsedScopes — never in the access-token caller's — so the
            // previous gate (claimAdds.Contains("profile")) left name,
            // email and roles out of every access token, and GetUserName()
            // resolved to null on every bearer host (the MyFiles upload
            // 500). Gate on the client's AllowedScopes instead, which is
            // caller-independent, so both the access token and the id
            // token carry these claims for any user grant whose client is
            // allowed "profile".
            if (context.Client.AllowedScopes.Contains(JwtClaimTypes.Profile))
            {
                // "name" carries the login (UserName), not the display
                // FullName: GetUserName() / User.Identity.Name feed
                // filesystem paths (user files root, avatar file name),
                // and FullName can contain spaces. No bearer-host
                // consumer displays this claim as a human name today.
                claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
                var roles = await this._userManager.GetRolesAsync(user);
                if (roles.Count() > 0)
                {
                    claims.AddRange(roles.Select(r => new Claim(Constants.RoleClaimType, r)));
                }
            }
            return claims;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = GetSubjectId(context.Subject);
            if (subjectId == null) return;
            var user = await _userManager.FindByIdAsync(subjectId);
            if (user == null) return;
            context.IssuedClaims = await GetClaimsFromUserAsync(context, user);
        }

         public async Task IsActiveAsync(IsActiveContext context)
        {
            string? subjectId = GetSubjectId(context.Subject);
            if (subjectId == null)
            {
                context.IsActive = false;
                return;
            }
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null;
        }

        private static string? GetSubjectId(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }
    }
}
