using System.Security.Claims;
using IdentityModel;
using IdentityServer8.Models;
using IdentityServer8.Services;
using IdentityServer8.Stores;
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
            List<string> claimAdds = new List<string>();

            foreach (var scope in context.RequestedResources.ParsedScopes)
            {
                if (context.Client.AllowedScopes.Contains(scope.ParsedName))
                {
                    claims.Add(new Claim(JwtClaimTypes.Scope, scope.ParsedName));
                    claimAdds.Add(scope.ParsedName);
                }
            }

            if (claimAdds.Contains(JwtClaimTypes.Profile))
            {
                claimAdds.Remove("profile");
                claimAdds.Add(JwtClaimTypes.Name);
                claimAdds.Add(JwtClaimTypes.Email);
                claimAdds.Add(YavscConstants.RoleClaimType);
            }

            if (claimAdds.Contains(JwtClaimTypes.Name))
                claims.Add(new Claim(JwtClaimTypes.Name, user.FullName));

            if (claimAdds.Contains(JwtClaimTypes.Email))
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            
            if (claimAdds.Contains(YavscConstants.RoleClaimType))
            {
                var roles = await this._userManager.GetRolesAsync(user);
                if (roles.Count()>0)
                {
                    claims.AddRange(roles.Select(r => new Claim(YavscConstants.RoleClaimType, r)));
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
