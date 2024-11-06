
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;

namespace Yavsc.Services;

public class YavscClientStore : IClientStore
{
    ApplicationDbContext _context=null;
    public YavscClientStore(ApplicationDbContext context)
    {
        _context = context;
    }


    async Task<Client> IClientStore.FindClientByIdAsync(string clientId)
    {
        var app = await _context.Applications.FirstOrDefaultAsync(c=>c.Id == clientId);

        if (app == null) return null;
      
        Client client = new()
        {
            ClientId = app.Id,
            ClientName = app.DisplayName,
            AbsoluteRefreshTokenLifetime = app.RefreshTokenLifeTime,
            AccessTokenLifetime = app.AccessTokenLifetime,
            AllowedGrantTypes = 
            [ 
                GrantType.AuthorizationCode,
                GrantType.DeviceFlow,
                GrantType.ClientCredentials
            ],
            ClientSecrets = [
                new Secret(app.Secret),
            ]
        };


        switch(app.Type)
        {
            case Models.Auth.ApplicationTypes.NativeConfidential:
                client.AccessTokenType = AccessTokenType.Reference;
                client.AllowedGrantTypes = 
                    [ 
                        GrantType.DeviceFlow
                    ];
                client.AllowedScopes = [] ;
                break;
            case Models.Auth.ApplicationTypes.JavaScript:
            default:
                client.AccessTokenType = AccessTokenType.Jwt;
                client.AllowedGrantTypes = 
                    [ 
                        GrantType.AuthorizationCode,
                        GrantType.ClientCredentials
                    ];
                client.AllowedScopes = ["openid", "profile"];
                break;

        }
        return client;
    }
}
