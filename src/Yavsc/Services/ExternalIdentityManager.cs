using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Yavsc.Interfaces;
using Yavsc.Models;

public class ExternalIdentityManager : IExternalIdentityManager
{
    private ApplicationDbContext _applicationDbContext;

    public ExternalIdentityManager(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    public ApplicationUser AutoProvisionUser(string provider, string providerUserId, List<Claim> claims)
    {
        throw new NotImplementedException();
    }

    public async Task<ApplicationUser?> FindByExternaleProviderAsync(string provider, string providerUserId)
    {
        var user = await _applicationDbContext.AspNetUserLogins
         .FirstOrDefaultAsync(
             i => (i.LoginProvider == provider) && (i.ProviderKey == providerUserId)
         );
         if (user == null) return null;
        return await _applicationDbContext.Users.FirstOrDefaultAsync(u=>u.Id == user.UserId);
    }
}
