using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using IdentityServer8.Stores;
using IdentityServer8.Models;

namespace Yavsc.Services;

public class ClientStore : IClientStore
{
    public ClientStore(ApplicationDbContext applicationDbContext)
    {
        ApplicationDbContext = applicationDbContext;
    }

    public ApplicationDbContext ApplicationDbContext { get; }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        var clientFromDb = await ApplicationDbContext.Client.FirstAsync(c => c.Id == clientId);
        
        return new Client
        {
            ClientId = clientFromDb.Id,
            ClientName = clientFromDb.DisplayName,
            ClientSecrets = { new Secret(clientFromDb.Secret.Sha256()) },
            AllowedGrantTypes =[ GrantType.ClientCredentials, GrantType.DeviceFlow],
            AllowedScopes = ["openid", "profile", "scope1"]
        };
    }
}
