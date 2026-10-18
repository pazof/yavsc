using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using IdentityServer8.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Administration;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Server.Models.Calendar;
using Yavsc.Server.Models.EMailing;
using Yavsc.Server.Settings;

namespace Yavsc.Api.Services;

public sealed partial class AdministrationService(
    ApplicationDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles,
    IHttpContextAccessor httpContext) : IAdministrationService
{
    private string Actor => httpContext.HttpContext?.User.GetUserId()
        ?? throw new AdministrationOperationException(401, "Session administrateur indisponible.");

    private static AdministrationOperationException Missing() => new(404, "Ressource introuvable.");
    private static AdministrationOperationException Invalid(string message) => new(400, message);
    private static AdministrationOperationException Conflict(string message) => new(409, message);

    private static void CheckIdentity(IdentityResult result)
    {
        if (!result.Succeeded)
            throw Invalid(string.Join("; ", result.Errors.Select(error => error.Description)));
    }

    public async Task<AdministrationSummary> GetSummaryAsync(CancellationToken ct)
        => new(await db.Users.CountAsync(ct),
            (await users.GetUsersInRoleAsync(Constants.AdminGroupName)).Count,
            typeof(AdministrationService).Assembly.GetName().ToString(), System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

    public async Task<IReadOnlyList<AdministrationRole>> GetRolesAsync(CancellationToken ct)
    {
        var all = await roles.Roles.OrderBy(role => role.Name).ToListAsync(ct);
        var result = new List<AdministrationRole>();
        foreach (var role in all)
            result.Add(new(role.Id, role.Name!, await db.UserRoles.CountAsync(row => row.RoleId == role.Id, ct)));
        return result;
    }

    public async Task EnsureRolesAsync(CancellationToken ct)
    {
        foreach (var name in new[] { Constants.AdminGroupName, Constants.StarGroupName, Constants.PerformerGroupName,
                     Constants.FrontOfficeGroupName, Constants.StarHunterGroupName, Constants.BlogModeratorGroupName })
        {
            ct.ThrowIfCancellationRequested();
            if (!await roles.RoleExistsAsync(name))
                CheckIdentity(await roles.CreateAsync(new IdentityRole(name)));
        }
    }

    public async Task<IReadOnlyList<RoleMember>> GetRoleMembersAsync(string roleId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var role = await roles.FindByIdAsync(roleId) ?? throw Missing();
        return (await users.GetUsersInRoleAsync(role.Name!))
            .Select(user => new RoleMember(user.Id, user.UserName!, user.Email)).ToArray();
    }

    public async Task SetRoleMembershipAsync(string roleId, string userId, bool member, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var role = await roles.FindByIdAsync(roleId) ?? throw Missing();
        var user = await users.FindByIdAsync(userId) ?? throw Missing();
        var alreadyMember = await users.IsInRoleAsync(user, role.Name!);
        if (member && !alreadyMember)
            CheckIdentity(await users.AddToRoleAsync(user, role.Name!));
        else if (!member && alreadyMember)
        {
            if (role.Name == Constants.AdminGroupName &&
                (await users.GetUsersInRoleAsync(role.Name)).Count <= 1)
                throw Conflict("Impossible de retirer le dernier administrateur.");
            CheckIdentity(await users.RemoveFromRoleAsync(user, role.Name!));
        }
        await transaction.CommitAsync(ct);
    }

    private IQueryable<Client> ClientQuery => db.Clients
        .Include(client => client.RedirectUris).Include(client => client.PostLogoutRedirectUris)
        .Include(client => client.AllowedScopes).Include(client => client.AllowedGrantTypes)
        .Include(client => client.AllowedCorsOrigins).Include(client => client.IdentityProviderRestrictions)
        .Include(client => client.Claims).Include(client => client.Properties).Include(client => client.ClientSecrets)
        .AsSplitQuery();

    public async Task<IReadOnlyList<AdministrationClient>> GetClientsAsync(CancellationToken ct)
        => (await ClientQuery.AsNoTracking().OrderBy(client => client.ClientId).ToListAsync(ct)).Select(ToClient).ToArray();

    public async Task<AdministrationClient?> GetClientAsync(int id, CancellationToken ct)
    {
        var client = await ClientQuery.AsNoTracking().SingleOrDefaultAsync(client => client.Id == id, ct);
        return client is null ? null : ToClient(client);
    }

    public async Task<AdministrationClient> SaveClientAsync(int? id, ClientSettings settings, CancellationToken ct)
    {
        if (await db.Clients.AnyAsync(client => client.ClientId == settings.ClientId && client.Id != id, ct))
            throw Conflict("Cet identifiant client existe déjà.");
        var client = id.HasValue
            ? await ClientQuery.SingleOrDefaultAsync(client => client.Id == id, ct) ?? throw Missing()
            : new Client { Created = DateTime.UtcNow };
        ApplySettings(client, settings);
        client.Updated = DateTime.UtcNow;
        if (!id.HasValue)
        {
            client.AllowedGrantTypes = [new ClientGrantType { GrantType = "authorization_code" }];
            client.AllowedScopes = [new ClientScope { Scope = "openid" }, new ClientScope { Scope = "profile" }];
            db.Clients.Add(client);
        }
        await db.SaveChangesAsync(Actor, ct);
        return await GetClientAsync(client.Id, ct) ?? throw Missing();
    }

    public async Task DeleteClientAsync(int id, CancellationToken ct)
    {
        var client = await ClientQuery.SingleOrDefaultAsync(client => client.Id == id, ct) ?? throw Missing();
        db.Clients.Remove(client);
        await db.SaveChangesAsync(Actor, ct);
    }

    public async Task AddClientEntryAsync(int id, ClientCollection collection, ClientEntry entry, CancellationToken ct)
    {
        var client = await ClientQuery.SingleOrDefaultAsync(client => client.Id == id, ct) ?? throw Missing();
        if (ToClient(client).Entries.Any(row => row.Collection == collection && row.Key == entry.Key &&
                (row.Value == entry.Value || collection == ClientCollection.Properties)))
            throw Conflict("Cette entrée existe déjà.");
        if (collection is ClientCollection.Claims or ClientCollection.Properties && string.IsNullOrWhiteSpace(entry.Key))
            throw Invalid("Une clé est requise.");
        if (collection is ClientCollection.RedirectUris or ClientCollection.PostLogoutRedirectUris or ClientCollection.CorsOrigins)
        {
            if (!Uri.TryCreate(entry.Value, UriKind.Absolute, out var uri) ||
                uri.Scheme is "javascript" or "data" || !string.IsNullOrEmpty(uri.Fragment))
                throw Invalid("Une URI absolue sans fragment est requise.");
            if (collection == ClientCollection.CorsOrigins &&
                (uri.Scheme is not ("http" or "https") || uri.AbsolutePath != "/" || uri.Query.Length != 0))
                throw Invalid("Une origine HTTP(S) sans chemin est requise.");
        }
        object row = collection switch
        {
            ClientCollection.RedirectUris => new ClientRedirectUri { ClientId = id, RedirectUri = entry.Value },
            ClientCollection.PostLogoutRedirectUris => new ClientPostLogoutRedirectUri { ClientId = id, PostLogoutRedirectUri = entry.Value },
            ClientCollection.Scopes => new ClientScope { ClientId = id, Scope = entry.Value },
            ClientCollection.GrantTypes => new ClientGrantType { ClientId = id, GrantType = entry.Value },
            ClientCollection.CorsOrigins => new ClientCorsOrigin { ClientId = id, Origin = entry.Value },
            ClientCollection.IdentityProviders => new ClientIdPRestriction { ClientId = id, Provider = entry.Value },
            ClientCollection.Claims => new ClientClaim { ClientId = id, Type = entry.Key!, Value = entry.Value },
            ClientCollection.Properties => new ClientProperty { ClientId = id, Key = entry.Key!, Value = entry.Value },
            _ => throw Invalid("Collection inconnue.")
        };
        db.Add(row);
        await db.SaveChangesAsync(Actor, ct);
    }

    public async Task DeleteClientEntryAsync(int id, ClientCollection collection, int rowId, CancellationToken ct)
    {
        var client = await ClientQuery.SingleOrDefaultAsync(client => client.Id == id, ct) ?? throw Missing();
        object? row = collection switch
        {
            ClientCollection.RedirectUris => client.RedirectUris.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.PostLogoutRedirectUris => client.PostLogoutRedirectUris.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.Scopes => client.AllowedScopes.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.GrantTypes => client.AllowedGrantTypes.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.CorsOrigins => client.AllowedCorsOrigins.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.IdentityProviders => client.IdentityProviderRestrictions.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.Claims => client.Claims.SingleOrDefault(row => row.Id == rowId),
            ClientCollection.Properties => client.Properties.SingleOrDefault(row => row.Id == rowId),
            _ => throw Invalid("Collection inconnue.")
        };
        db.Remove(row ?? throw Missing());
        await db.SaveChangesAsync(Actor, ct);
    }

    public async Task<NewClientSecret> CreateSecretAsync(int id, ClientSecretRequest request, bool replace, CancellationToken ct)
    {
        var client = await db.Clients.Include(client => client.ClientSecrets).SingleOrDefaultAsync(client => client.Id == id, ct)
            ?? throw Missing();
        if (request.Expiration <= DateTimeOffset.UtcNow)
            throw Invalid("L’expiration doit être dans le futur.");
        var expiration = request.Expiration ?? (replace ? DateTimeOffset.UtcNow.AddDays(90) : null);
        var raw = replace || string.IsNullOrWhiteSpace(request.Value)
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
            : request.Value;
        if (replace)
            db.ClientSecrets.RemoveRange(client.ClientSecrets);
        var secret = new ClientSecret
        {
            ClientId = id, Type = "SharedSecret",
            Value = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw))),
            Description = request.Description, Created = DateTime.UtcNow, Expiration = expiration?.UtcDateTime
        };
        db.ClientSecrets.Add(secret);
        await db.SaveChangesAsync(Actor, ct);
        return new(secret.Id, raw, expiration);
    }

    public async Task DeleteSecretAsync(int id, int secretId, CancellationToken ct)
    {
        var secret = await db.ClientSecrets.SingleOrDefaultAsync(secret => secret.ClientId == id && secret.Id == secretId, ct)
            ?? throw Missing();
        db.ClientSecrets.Remove(secret);
        await db.SaveChangesAsync(Actor, ct);
    }

    public async Task<IReadOnlyList<AdministrationScope>> GetScopesAsync(CancellationToken ct)
        => (await db.ApiScopes.AsNoTracking().OrderBy(scope => scope.Name).ToListAsync(ct)).Select(ToScope).ToArray();

    public async Task<AdministrationScope> SaveScopeAsync(int? id, ApiScopeSettings settings, CancellationToken ct)
    {
        if (await db.ApiScopes.AnyAsync(scope => scope.Name == settings.Name && scope.Id != id, ct))
            throw Conflict("Ce scope existe déjà.");
        var scope = id.HasValue ? await db.ApiScopes.FindAsync([id.Value], ct) ?? throw Missing() : new ApiScope();
        scope.Name = settings.Name;
        scope.DisplayName = settings.DisplayName;
        scope.Description = settings.Description;
        scope.Enabled = settings.Enabled;
        scope.Required = settings.Required;
        scope.Emphasize = settings.Emphasize;
        scope.ShowInDiscoveryDocument = settings.ShowInDiscoveryDocument;
        if (!id.HasValue) db.ApiScopes.Add(scope);
        await db.SaveChangesAsync(Actor, ct);
        return ToScope(scope);
    }

    public async Task DeleteScopeAsync(int id, CancellationToken ct)
    {
        var scope = await db.ApiScopes.FindAsync([id], ct) ?? throw Missing();
        db.ApiScopes.Remove(scope);
        await db.SaveChangesAsync(Actor, ct);
    }

    public async Task<IReadOnlyList<MailingTemplateSettings>> GetMailingTemplatesAsync(CancellationToken ct)
        => (await db.MailingTemplate.AsNoTracking().OrderBy(template => template.Id).ToListAsync(ct))
            .Select(template => new MailingTemplateSettings { Id = template.Id, Topic = template.Topic,
                Body = template.Body, ReplyToAddress = template.ReplyToAddress, ToSend = (MailPeriodicity)template.ToSend }).ToArray();

    public IReadOnlyList<string> GetMailingCriteria() => UserPolicies.Criterias.Keys.Order().ToArray();

    public async Task<MailingTemplateSettings> SaveMailingTemplateAsync(string? id, MailingTemplateSettings settings, CancellationToken ct)
    {
        if (id is not null && id != settings.Id) throw Invalid("L’identifiant du modèle ne peut pas changer.");
        if (id is null && await db.MailingTemplate.AnyAsync(template => template.Id == settings.Id, ct))
            throw Conflict("Ce modèle existe déjà.");
        if (!UserPolicies.Criterias.ContainsKey(settings.Id))
            throw Invalid("Le modèle doit correspondre à un critère de diffusion connu.");
        var template = id is null ? new MailingTemplate { Id = settings.Id }
            : await db.MailingTemplate.FindAsync([id], ct) ?? throw Missing();
        template.Topic = settings.Topic;
        template.Body = settings.Body;
        template.ReplyToAddress = settings.ReplyToAddress;
        template.ToSend = (Periodicity)settings.ToSend;
        if (id is null) db.MailingTemplate.Add(template);
        await db.SaveChangesAsync(Actor, ct);
        return settings;
    }

    public async Task DeleteMailingTemplateAsync(string id, CancellationToken ct)
    {
        var template = await db.MailingTemplate.FindAsync([id], ct) ?? throw Missing();
        db.MailingTemplate.Remove(template);
        await db.SaveChangesAsync(Actor, ct);
    }

    private static DateTimeOffset Utc(DateTime date) => new(DateTime.SpecifyKind(date, DateTimeKind.Utc));

    private static AdministrationScope ToScope(ApiScope scope) => new(scope.Id, new ApiScopeSettings
    {
        Name = scope.Name, DisplayName = scope.DisplayName, Description = scope.Description, Enabled = scope.Enabled,
        Required = scope.Required, Emphasize = scope.Emphasize, ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument
    });

    private static AdministrationClient ToClient(Client client)
    {
        var entries = new List<ClientEntryResponse>();
        entries.AddRange(client.RedirectUris.Select(row => new ClientEntryResponse(row.Id, ClientCollection.RedirectUris, null, row.RedirectUri)));
        entries.AddRange(client.PostLogoutRedirectUris.Select(row => new ClientEntryResponse(row.Id, ClientCollection.PostLogoutRedirectUris, null, row.PostLogoutRedirectUri)));
        entries.AddRange(client.AllowedScopes.Select(row => new ClientEntryResponse(row.Id, ClientCollection.Scopes, null, row.Scope)));
        entries.AddRange(client.AllowedGrantTypes.Select(row => new ClientEntryResponse(row.Id, ClientCollection.GrantTypes, null, row.GrantType)));
        entries.AddRange(client.AllowedCorsOrigins.Select(row => new ClientEntryResponse(row.Id, ClientCollection.CorsOrigins, null, row.Origin)));
        entries.AddRange(client.IdentityProviderRestrictions.Select(row => new ClientEntryResponse(row.Id, ClientCollection.IdentityProviders, null, row.Provider)));
        entries.AddRange(client.Claims.Select(row => new ClientEntryResponse(row.Id, ClientCollection.Claims, row.Type, row.Value)));
        entries.AddRange(client.Properties.Select(row => new ClientEntryResponse(row.Id, ClientCollection.Properties, row.Key, row.Value)));
        return new(client.Id, ReadSettings(client), entries,
            client.ClientSecrets.Select(secret => new ClientSecretInfo(secret.Id, secret.Description,
                Utc(secret.Created), secret.Expiration.HasValue ? Utc(secret.Expiration.Value) : null)).ToArray());
    }

    private static ClientSettings ReadSettings(Client client) => new()
    {
        ClientId = client.ClientId, ClientName = client.ClientName, Enabled = client.Enabled, Description = client.Description,
        ClientUri = client.ClientUri, LogoUri = client.LogoUri, ProtocolType = client.ProtocolType,
        RequireClientSecret = client.RequireClientSecret, RequireConsent = client.RequireConsent,
        AllowRememberConsent = client.AllowRememberConsent, AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken,
        RequirePkce = client.RequirePkce, AllowPlainTextPkce = client.AllowPlainTextPkce, RequireRequestObject = client.RequireRequestObject,
        AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser, FrontChannelLogoutUri = client.FrontChannelLogoutUri,
        FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired, BackChannelLogoutUri = client.BackChannelLogoutUri,
        BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired, AllowOfflineAccess = client.AllowOfflineAccess,
        IdentityTokenLifetime = client.IdentityTokenLifetime, AccessTokenLifetime = client.AccessTokenLifetime,
        AuthorizationCodeLifetime = client.AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
        SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime, ConsentLifetime = client.ConsentLifetime,
        AccessTokenType = (ClientAccessTokenType)client.AccessTokenType,
        RefreshTokenExpiration = (ClientRefreshTokenExpiration)client.RefreshTokenExpiration,
        RefreshTokenUsage = (ClientRefreshTokenUsage)client.RefreshTokenUsage, UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
        EnableLocalLogin = client.EnableLocalLogin, IncludeJwtId = client.IncludeJwtId, AlwaysSendClientClaims = client.AlwaysSendClientClaims,
        ClientClaimsPrefix = client.ClientClaimsPrefix, PairWiseSubjectSalt = client.PairWiseSubjectSalt,
        UserSsoLifetime = client.UserSsoLifetime, UserCodeType = client.UserCodeType, DeviceCodeLifetime = client.DeviceCodeLifetime,
        AllowedIdentityTokenSigningAlgorithms = client.AllowedIdentityTokenSigningAlgorithms
    };

    private static void ApplySettings(Client client, ClientSettings settings)
    {
        client.ClientId = settings.ClientId; client.ClientName = settings.ClientName; client.Enabled = settings.Enabled;
        client.Description = settings.Description; client.ClientUri = settings.ClientUri; client.LogoUri = settings.LogoUri;
        client.ProtocolType = settings.ProtocolType; client.RequireClientSecret = settings.RequireClientSecret;
        client.RequireConsent = settings.RequireConsent; client.AllowRememberConsent = settings.AllowRememberConsent;
        client.AlwaysIncludeUserClaimsInIdToken = settings.AlwaysIncludeUserClaimsInIdToken;
        client.RequirePkce = settings.RequirePkce; client.AllowPlainTextPkce = settings.AllowPlainTextPkce;
        client.RequireRequestObject = settings.RequireRequestObject; client.AllowAccessTokensViaBrowser = settings.AllowAccessTokensViaBrowser;
        client.FrontChannelLogoutUri = settings.FrontChannelLogoutUri; client.FrontChannelLogoutSessionRequired = settings.FrontChannelLogoutSessionRequired;
        client.BackChannelLogoutUri = settings.BackChannelLogoutUri; client.BackChannelLogoutSessionRequired = settings.BackChannelLogoutSessionRequired;
        client.AllowOfflineAccess = settings.AllowOfflineAccess; client.IdentityTokenLifetime = settings.IdentityTokenLifetime;
        client.AccessTokenLifetime = settings.AccessTokenLifetime; client.AuthorizationCodeLifetime = settings.AuthorizationCodeLifetime;
        client.AbsoluteRefreshTokenLifetime = settings.AbsoluteRefreshTokenLifetime; client.SlidingRefreshTokenLifetime = settings.SlidingRefreshTokenLifetime;
        client.ConsentLifetime = settings.ConsentLifetime; client.AccessTokenType = (int)settings.AccessTokenType;
        client.RefreshTokenExpiration = (int)settings.RefreshTokenExpiration; client.RefreshTokenUsage = (int)settings.RefreshTokenUsage;
        client.UpdateAccessTokenClaimsOnRefresh = settings.UpdateAccessTokenClaimsOnRefresh; client.EnableLocalLogin = settings.EnableLocalLogin;
        client.IncludeJwtId = settings.IncludeJwtId; client.AlwaysSendClientClaims = settings.AlwaysSendClientClaims;
        client.ClientClaimsPrefix = settings.ClientClaimsPrefix; client.PairWiseSubjectSalt = settings.PairWiseSubjectSalt;
        client.UserSsoLifetime = settings.UserSsoLifetime; client.UserCodeType = settings.UserCodeType;
        client.DeviceCodeLifetime = settings.DeviceCodeLifetime; client.AllowedIdentityTokenSigningAlgorithms = settings.AllowedIdentityTokenSigningAlgorithms;
    }
}
