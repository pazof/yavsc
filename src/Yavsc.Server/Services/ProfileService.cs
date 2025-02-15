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
    public class ProfileService : DefaultProfileService, IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileService(
            UserManager<ApplicationUser> userManager, ILogger<DefaultProfileService> logger) : base(logger)
        {
            _userManager = userManager;
        }

        public async Task<List<Claim>> GetClaimsFromUserAsync(
            ProfileDataRequestContext context,
            ApplicationUser user)
        {
            var requestedApiResources = context.RequestedResources.Resources.ApiResources.Select(
                r => r.Name
            ).ToArray();
            var requestedApiScopes = context.RequestedResources.Resources.ApiScopes.Select(
                s => s.Name
            ).ToArray();

            var requestedScopes = context.Client.AllowedScopes
            .Where(s => s != JwtClaimTypes.Subject
            && requestedApiScopes.Contains(s))
            .ToList();

            if (context.RequestedClaimTypes.Contains("profile"))
            if (requestedScopes.Contains("profile"))
            {
                requestedScopes.Remove("profile");
                requestedScopes.Add(JwtClaimTypes.Name);
                requestedScopes.Add(JwtClaimTypes.FamilyName);
                requestedScopes.Add(JwtClaimTypes.Email);
                requestedScopes.Add(JwtClaimTypes.PreferredUserName);
                requestedScopes.Add(JwtClaimTypes.Role);
            }

            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
            };
            if (requestedScopes.Contains(JwtClaimTypes.Name)||
                requestedScopes.Contains(JwtClaimTypes.FamilyName))
            {
                claims.Add(new Claim(JwtClaimTypes.Name, user.FullName));
            }

            if (requestedScopes.Contains(JwtClaimTypes.PreferredUserName) )
            {
                claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));
            }
            if (requestedScopes.Contains(JwtClaimTypes.Email))
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            
            if (requestedScopes.Contains(JwtClaimTypes.Role))
            {
                var roles = await this._userManager.GetRolesAsync(user);
                if (roles.Count()>0)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role,String.Join(" ",roles)));
                }
            }
            return claims;
        }

        override public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IssuedClaims = await GetClaimsFromUserAsync(context, user);
        }

        override public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null;
        }

    }
}
