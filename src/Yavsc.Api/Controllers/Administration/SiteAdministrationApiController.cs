using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Yavsc.Abstract.Administration;
using Yavsc.Api.Services;

namespace Yavsc.ApiControllers.Administration;

[ApiController]
[ApiExplorerSettings(GroupName = Constants.Administration)]
[Authorize("AdministratorOnly")]
[Route(Constants.APIPrefix + "/administration")]
[Produces("application/json")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[ProducesResponseType<ProblemDetails>(400)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
[ProducesResponseType(401)]
[ProducesResponseType(403)]
[ServiceFilter(typeof(AdministrationExceptionFilter))]
public sealed class SiteAdministrationApiController(IAdministrationService service) : ControllerBase
{
    [HttpGet("summary"), EndpointSummary("Statistiques du site et informations du logiciel")]
    [ProducesResponseType<AdministrationSummary>(200)]
    public async Task<ActionResult<AdministrationSummary>> Summary(CancellationToken ct)
        => Ok(await service.GetSummaryAsync(ct));

    [HttpGet("roles"), EndpointSummary("Lister les rôles et leurs effectifs")]
    public async Task<ActionResult<IReadOnlyList<AdministrationRole>>> Roles(CancellationToken ct)
        => Ok(await service.GetRolesAsync(ct));

    [HttpPost("roles/ensure"), EndpointSummary("Initialiser les rôles standards"), ProducesResponseType(204)]
    public async Task<IActionResult> EnsureRoles(CancellationToken ct)
    {
        await service.EnsureRolesAsync(ct);
        return NoContent();
    }

    [HttpGet("roles/{id}/members"), EndpointSummary("Lister les membres d’un rôle")]
    public async Task<ActionResult<IReadOnlyList<RoleMember>>> RoleMembers(string id, CancellationToken ct)
        => Ok(await service.GetRoleMembersAsync(id, ct));

    [HttpPut("roles/{id}/members/{userId}"), EndpointSummary("Inscrire un utilisateur dans un rôle"), ProducesResponseType(204)]
    public async Task<IActionResult> Enroll(string id, string userId, CancellationToken ct)
    {
        await service.SetRoleMembershipAsync(id, userId, true, ct);
        return NoContent();
    }

    [HttpDelete("roles/{id}/members/{userId}"), EndpointSummary("Retirer un membre, sauf le dernier administrateur"), ProducesResponseType(204)]
    public async Task<IActionResult> Fire(string id, string userId, CancellationToken ct)
    {
        await service.SetRoleMembershipAsync(id, userId, false, ct);
        return NoContent();
    }

    [HttpGet("clients"), EndpointSummary("Lister les clients OAuth/OIDC sans exposer leurs secrets")]
    public async Task<ActionResult<IReadOnlyList<AdministrationClient>>> Clients(CancellationToken ct)
        => Ok(await service.GetClientsAsync(ct));

    [HttpGet("clients/{id:int}"), EndpointSummary("Consulter la configuration d’un client")]
    [ProducesResponseType<AdministrationClient>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AdministrationClient>> Client(int id, CancellationToken ct)
    {
        var client = await service.GetClientAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost("clients"), EndpointSummary("Créer un client avec les valeurs par défaut PKCE")]
    [ProducesResponseType<AdministrationClient>(201)]
    public async Task<ActionResult<AdministrationClient>> CreateClient(ClientSettings settings, CancellationToken ct)
    {
        var client = await service.SaveClientAsync(null, settings, ct);
        return CreatedAtAction(nameof(Client), new { id = client.Id }, client);
    }

    [HttpPut("clients/{id:int}"), EndpointSummary("Remplacer les paramètres scalaires d’un client")]
    public async Task<ActionResult<AdministrationClient>> UpdateClient(int id, ClientSettings settings, CancellationToken ct)
        => Ok(await service.SaveClientAsync(id, settings, ct));

    [HttpDelete("clients/{id:int}"), EndpointSummary("Supprimer un client et ses collections"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken ct)
    {
        await service.DeleteClientAsync(id, ct);
        return NoContent();
    }

    [HttpPost("clients/{id:int}/collections/{collection}"), EndpointSummary("Ajouter une entrée à une collection du client"), ProducesResponseType(204)]
    public async Task<IActionResult> AddEntry(int id, ClientCollection collection, ClientEntry entry, CancellationToken ct)
    {
        await service.AddClientEntryAsync(id, collection, entry, ct);
        return NoContent();
    }

    [HttpDelete("clients/{id:int}/collections/{collection}/{rowId:int}"), EndpointSummary("Supprimer une entrée du client"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteEntry(int id, ClientCollection collection, int rowId, CancellationToken ct)
    {
        await service.DeleteClientEntryAsync(id, collection, rowId, ct);
        return NoContent();
    }

    [HttpPost("clients/{id:int}/secrets"), EndpointSummary("Ajouter un secret et le restituer une seule fois")]
    [ProducesResponseType<NewClientSecret>(201)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<NewClientSecret>> AddSecret(int id, ClientSecretRequest request, CancellationToken ct)
    {
        var secret = await service.CreateSecretAsync(id, request, false, ct);
        return CreatedAtAction(nameof(Client), new { id }, secret);
    }

    [HttpPost("clients/{id:int}/secrets/regenerate"), EndpointSummary("Générer un secret remplaçant tous les précédents")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<NewClientSecret>> RegenerateSecret(int id, ClientSecretRequest request, CancellationToken ct)
        => Ok(await service.CreateSecretAsync(id, request, true, ct));

    [HttpDelete("clients/{id:int}/secrets/{secretId:int}"), EndpointSummary("Supprimer un secret du client"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteSecret(int id, int secretId, CancellationToken ct)
    {
        await service.DeleteSecretAsync(id, secretId, ct);
        return NoContent();
    }

    [HttpGet("scopes"), EndpointSummary("Lister les scopes API")]
    public async Task<ActionResult<IReadOnlyList<AdministrationScope>>> Scopes(CancellationToken ct)
        => Ok(await service.GetScopesAsync(ct));

    [HttpGet("scopes/{id:int}"), EndpointSummary("Consulter un scope API")]
    public async Task<ActionResult<AdministrationScope>> Scope(int id, CancellationToken ct)
    {
        var scope = (await service.GetScopesAsync(ct)).SingleOrDefault(scope => scope.Id == id);
        return scope is null ? NotFound() : Ok(scope);
    }

    [HttpPost("scopes"), EndpointSummary("Créer un scope API")]
    [ProducesResponseType<AdministrationScope>(201)]
    public async Task<ActionResult<AdministrationScope>> CreateScope(ApiScopeSettings settings, CancellationToken ct)
    {
        var scope = await service.SaveScopeAsync(null, settings, ct);
        return CreatedAtAction(nameof(Scope), new { id = scope.Id }, scope);
    }

    [HttpPut("scopes/{id:int}"), EndpointSummary("Modifier un scope API")]
    public async Task<ActionResult<AdministrationScope>> UpdateScope(int id, ApiScopeSettings settings, CancellationToken ct)
        => Ok(await service.SaveScopeAsync(id, settings, ct));

    [HttpDelete("scopes/{id:int}"), EndpointSummary("Supprimer un scope API"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteScope(int id, CancellationToken ct)
    {
        await service.DeleteScopeAsync(id, ct);
        return NoContent();
    }

    [HttpGet("mailing-criteria"), EndpointSummary("Lister les critères de diffusion utilisables dans les modèles")]
    public ActionResult<IReadOnlyList<string>> MailingCriteria(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Ok(service.GetMailingCriteria());
    }

    [HttpGet("mailing-templates"), EndpointSummary("Lister les modèles de courriels")]
    public async Task<ActionResult<IReadOnlyList<MailingTemplateSettings>>> MailingTemplates(CancellationToken ct)
        => Ok(await service.GetMailingTemplatesAsync(ct));

    [HttpGet("mailing-templates/{id}"), EndpointSummary("Consulter un modèle de courriel")]
    public async Task<ActionResult<MailingTemplateSettings>> MailingTemplate(string id, CancellationToken ct)
    {
        var template = (await service.GetMailingTemplatesAsync(ct)).SingleOrDefault(template => template.Id == id);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost("mailing-templates"), EndpointSummary("Créer un modèle de courriel")]
    [ProducesResponseType<MailingTemplateSettings>(201)]
    public async Task<ActionResult<MailingTemplateSettings>> CreateMailingTemplate(MailingTemplateSettings settings, CancellationToken ct)
    {
        var template = await service.SaveMailingTemplateAsync(null, settings, ct);
        return CreatedAtAction(nameof(MailingTemplate), new { id = template.Id }, template);
    }

    [HttpPut("mailing-templates/{id}"), EndpointSummary("Modifier un modèle de courriel")]
    public async Task<ActionResult<MailingTemplateSettings>> UpdateMailingTemplate(string id, MailingTemplateSettings settings, CancellationToken ct)
        => Ok(await service.SaveMailingTemplateAsync(id, settings, ct));

    [HttpDelete("mailing-templates/{id}"), EndpointSummary("Supprimer un modèle de courriel"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteMailingTemplate(string id, CancellationToken ct)
    {
        await service.DeleteMailingTemplateAsync(id, ct);
        return NoContent();
    }

    [HttpGet("transfer"), EndpointSummary("Exporter les données métier sans matériel d’authentification")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<AdministrationTransfer>> Export(CancellationToken ct)
        => Ok(await service.ExportAsync(ct));

    [HttpPost("transfer"), EndpointSummary("Importer les données métier en une transaction, limite 16 Mio")]
    [RequestSizeLimit(16 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> Import(AdministrationTransfer transfer, CancellationToken ct)
        => Ok(await service.ImportAsync(transfer, ct));
}

public sealed class AdministrationExceptionFilter(ILogger<AdministrationExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AdministrationOperationException exception)
            return;
        logger.LogWarning("Administration request rejected with status {Status}: {Reason}", exception.StatusCode, exception.Message);
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = exception.StatusCode, Title = exception.Message
        }) { StatusCode = exception.StatusCode };
        context.ExceptionHandled = true;
    }
}
