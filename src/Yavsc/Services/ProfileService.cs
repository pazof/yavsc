using System.Security.Claims;
using IdentityModel;
using IdentityServer8.Models;
using IdentityServer8.Services;
using IdentityServer8.Stores;
using Microsoft.AspNetCore.Identity;
using Yavsc.Models;

namespace Yavsc.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<Claim>> GetClaimsFromUserAsync(
            ProfileDataRequestContext context,
            ApplicationUser user)
        {

            var allowedScopes = context.Client.AllowedScopes
            .Where(s => s != JwtClaimTypes.Subject)
            .ToList();
            if (allowedScopes.Contains("profile"))
            {
                allowedScopes.Remove("profile");
                allowedScopes.Add(JwtClaimTypes.Name);
                allowedScopes.Add(JwtClaimTypes.FamilyName);
                allowedScopes.Add(JwtClaimTypes.Email);
                allowedScopes.Add(JwtClaimTypes.PreferredUserName);
                allowedScopes.Add("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            }

            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
            };

            foreach (var subClaim in context.Subject.Claims)
            {
                if (allowedScopes.Contains(subClaim.Type))
                    claims.Add(subClaim);
            }

            AddClaims(allowedScopes, claims, JwtClaimTypes.Email, user.Email);
            AddClaims(allowedScopes, claims, JwtClaimTypes.PreferredUserName, user.FullName);

            foreach (var scope in context.Client.AllowedScopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            return claims;
        }

        private static void AddClaims(List<string> allowedScopes, List<Claim> claims,
            string claimType, string claimValue
        )
        {
            if (allowedScopes.Contains(claimType))
                if (!claims.Any(c => c.Type == claimType))
                    claims.Add(new Claim(claimType, claimValue));
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IssuedClaims = await GetClaimsFromUserAsync(context, user);
        }


        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null;
        }

    }
}
