using IdentityServer8.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers;

/// <summary>
/// Partial class that adds the per-collection edit pages for an OAuth2
/// client. See <c>ClientController.cs</c> for the scalar edit flow and
/// the seed/secret management. The collection pages are deliberately
/// factored into a separate file so the controller stays navigable.
///
/// Each collection has three actions:
/// <list type="bullet">
///   <item><c>GET Edit{Collection}(int id)</c> — render the page</item>
///   <item><c>POST Add{Collection}(int id, …)</c> — append a row</item>
///   <item><c>POST Remove{Collection}(int id, int rowId)</c> — delete a row</item>
/// </list>
/// </summary>
[Authorize("AdministratorOnly")]
public partial class ClientController
{
    // ---- Redirect URIs ------------------------------------------------

    readonly IHtmlLocalizer _localizer;

    public ClientController(
        IHtmlLocalizer<ClientController> localizer
        )
         {
            _localizer = localizer;
        }


    [HttpGet]
    public async Task<IActionResult> EditRedirectUris(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.RedirectUris.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRedirectUri(int id, string redirectUri)
    {
        return await AddCollectionRowAsync<ClientRedirectUri>(
            id, redirectUri,
            (client, uri) => new ClientRedirectUri { ClientId = client.Id, RedirectUri = uri });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRedirectUri(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientRedirectUri>(id, rowId, "EditRedirectUris");

    // ---- Post-logout Redirect URIs -----------------------------------

    [HttpGet]
    public async Task<IActionResult> EditPostLogoutRedirectUris(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.PostLogoutRedirectUris.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPostLogoutRedirectUri(int id, string postLogoutRedirectUri)
    {
        return await AddCollectionRowAsync<ClientPostLogoutRedirectUri>(
            id, postLogoutRedirectUri,
            (client, uri) => new ClientPostLogoutRedirectUri { ClientId = client.Id, PostLogoutRedirectUri = uri });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePostLogoutRedirectUri(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientPostLogoutRedirectUri>(id, rowId, "EditPostLogoutRedirectUris");

    // ---- Allowed Scopes ----------------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditScopes(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.AllowedScopes.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddScope(int id, string scope)
    {
        return await AddCollectionRowAsync<ClientScope>(
            id, scope,
            (client, s) => new ClientScope { ClientId = client.Id, Scope = s });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveScope(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientScope>(id, rowId, "EditScopes");

    // ---- Allowed Grant Types -----------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditGrantTypes(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.AllowedGrantTypes.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGrantType(int id, string grantType)
    {
        return await AddCollectionRowAsync<ClientGrantType>(
            id, grantType,
            (client, g) => new ClientGrantType { ClientId = client.Id, GrantType = g });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveGrantType(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientGrantType>(id, rowId, "EditGrantTypes");

    // ---- Allowed CORS Origins ----------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditCorsOrigins(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.AllowedCorsOrigins.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCorsOrigin(int id, string origin)
    {
        return await AddCollectionRowAsync<ClientCorsOrigin>(
            id, origin,
            (client, o) => new ClientCorsOrigin { ClientId = client.Id, Origin = o });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCorsOrigin(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientCorsOrigin>(id, rowId, "EditCorsOrigins");

    // ---- IdentityProvider Restrictions -------------------------------

    [HttpGet]
    public async Task<IActionResult> EditIdPRestrictions(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.IdentityProviderRestrictions.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddIdPRestriction(int id, string provider)
    {
        return await AddCollectionRowAsync<ClientIdPRestriction>(
            id, provider,
            (client, p) => new ClientIdPRestriction { ClientId = client.Id, Provider = p });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveIdPRestriction(int id, int rowId)
        => await RemoveCollectionRowAsync<ClientIdPRestriction>(id, rowId, "EditIdPRestrictions");

    // ---- Claims ------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditClaims(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.Claims.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaim(int id, string type, string value)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
        {
            TempData["Error"] = _localizer["BothTypeAndValueRequired"].Value;
            return RedirectToAction("EditClaims", new { id });
        }
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        dbContext.Set<ClientClaim>().Add(new ClientClaim { ClientId = client.Id, Type = type, Value = value });
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditClaims", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClaim(int id, int rowId)
    {
        var row = await dbContext.Set<ClientClaim>().FindAsync(rowId);
        if (row is null || row.ClientId != id) return NotFound();
        dbContext.Set<ClientClaim>().Remove(row);
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditClaims", new { id });
    }

    // ---- Properties (key/value) --------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditProperties(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.Properties.ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProperty(int id, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            TempData["Error"] = _localizer["KeyRequired"].Value;
            return RedirectToAction("EditProperties", new { id });
        }
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        dbContext.Set<ClientProperty>().Add(new ClientProperty { ClientId = client.Id, Key = key, Value = value });
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditProperties", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProperty(int id, int rowId)
    {
        var row = await dbContext.Set<ClientProperty>().FindAsync(rowId);
        if (row is null || row.ClientId != id) return NotFound();
        dbContext.Set<ClientProperty>().Remove(row);
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditProperties", new { id });
    }

    // ---- Secrets -----------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> EditSecrets(int id)
    {
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        SetAppTypesInputValues();
        return View(client.ClientSecrets?.ToList() ?? new List<ClientSecret>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSecret(int id, string value, string description, DateTime? expiration)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            TempData["Error"] = _localizer["SecretValueRequired"].Value;
            return RedirectToAction("EditSecrets", new { id });
        }
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        dbContext.ClientSecrets.Add(new ClientSecret
        {
            ClientId = client.Id,
            Type = "SharedSecret",
            Value = value,
            Description = description,
            Created = DateTime.UtcNow,
            Expiration = expiration
        });
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditSecrets", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSecret(int id, int rowId)
    {
        var row = await dbContext.ClientSecrets.FindAsync(rowId);
        if (row is null || row.ClientId != id) return NotFound();
        dbContext.ClientSecrets.Remove(row);
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction("EditSecrets", new { id });
    }

    // ---- Helpers -----------------------------------------------------

    private async Task<Client?> LoadClientAsync(int id)
        => await dbContext.Clients
            .Include(c => c.RedirectUris)
            .Include(c => c.PostLogoutRedirectUris)
            .Include(c => c.AllowedScopes)
            .Include(c => c.AllowedGrantTypes)
            .Include(c => c.AllowedCorsOrigins)
            .Include(c => c.IdentityProviderRestrictions)
            .Include(c => c.Claims)
            .Include(c => c.Properties)
            .Include(c => c.ClientSecrets)
            .SingleOrDefaultAsync(c => c.Id == id);

    private async Task<IActionResult> AddCollectionRowAsync<TEntity>(
        int id,
        string value,
        Func<Client, string, TEntity> factory)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            TempData["Error"] = _localizer["ValueRequired"].Value;
            return RedirectToAction(RedirectTargetFor<TEntity>(), new { id });
        }
        var client = await LoadClientAsync(id);
        if (client is null) return NotFound();
        dbContext.Add(factory(client, value));
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction(RedirectTargetFor<TEntity>(), new { id });
    }

    private async Task<IActionResult> RemoveCollectionRowAsync<TEntity>(int id, int rowId, string redirectAction)
        where TEntity : class
    {
        var row = await dbContext.FindAsync<TEntity>(rowId);
        if (row is null) return NotFound();
        // IdentityServer8 navigation properties are not always populated
        // by FindAsync; rely on the FK check on the caller side.
        var fk = (row as dynamic).ClientId as int?;
        if (fk is null || fk != id) return NotFound();
        dbContext.Remove(row);
        await dbContext.SaveChangesAsync(User.GetUserId());
        return RedirectToAction(redirectAction, new { id });
    }

    private static string RedirectTargetFor<TEntity>() => typeof(TEntity).Name switch
    {
        nameof(ClientRedirectUri) => nameof(EditRedirectUris),
        nameof(ClientPostLogoutRedirectUri) => nameof(EditPostLogoutRedirectUris),
        nameof(ClientScope) => nameof(EditScopes),
        nameof(ClientGrantType) => nameof(EditGrantTypes),
        nameof(ClientCorsOrigin) => nameof(EditCorsOrigins),
        nameof(ClientIdPRestriction) => nameof(EditIdPRestrictions),
        _ => "Edit",
    };
}
