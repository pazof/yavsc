using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Abstract.Administration;

namespace Yavsc.Api.Client;

public sealed class AdministrationApiClient(IYavscApiClient api, Func<string> baseAddress)
{
    private string Path(string relative) => new Uri(new Uri(baseAddress().TrimEnd('/') + "/", UriKind.Absolute),
        "administration/" + relative).ToString();
    private static string Escape(string value) => Uri.EscapeDataString(value);

    public Task<AdministrationSummary> GetSummaryAsync(CancellationToken ct = default)
        => api.CallAsync<AdministrationSummary>(HttpMethod.Get, Path("summary"), ct: ct);
    public Task<List<AdministrationRole>> GetRolesAsync(CancellationToken ct = default)
        => api.CallAsync<List<AdministrationRole>>(HttpMethod.Get, Path("roles"), ct: ct);
    public Task EnsureRolesAsync(CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Post, Path("roles/ensure"), ct: ct);
    public Task<List<RoleMember>> GetRoleMembersAsync(string roleId, CancellationToken ct = default)
        => api.CallAsync<List<RoleMember>>(HttpMethod.Get, Path($"roles/{Escape(roleId)}/members"), ct: ct);
    public Task SetRoleMembershipAsync(string roleId, string userId, bool member, CancellationToken ct = default)
        => api.CallAsync(member ? HttpMethod.Put : HttpMethod.Delete,
            Path($"roles/{Escape(roleId)}/members/{Escape(userId)}"), ct: ct);
    public Task<List<AdministrationClient>> GetClientsAsync(CancellationToken ct = default)
        => api.CallAsync<List<AdministrationClient>>(HttpMethod.Get, Path("clients"), ct: ct);
    public Task<AdministrationClient> SaveClientAsync(int? id, ClientSettings settings, CancellationToken ct = default)
        => api.CallAsync<AdministrationClient>(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            Path(id.HasValue ? $"clients/{id}" : "clients"), settings, ct);
    public Task DeleteClientAsync(int id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Path($"clients/{id}"), ct: ct);
    public Task AddClientEntryAsync(int id, ClientCollection collection, ClientEntry entry, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Post, Path($"clients/{id}/collections/{collection}"), entry, ct);
    public Task DeleteClientEntryAsync(int id, ClientCollection collection, int rowId, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Path($"clients/{id}/collections/{collection}/{rowId}"), ct: ct);
    public Task<NewClientSecret> CreateSecretAsync(int id, ClientSecretRequest request, bool replace, CancellationToken ct = default)
        => api.CallAsync<NewClientSecret>(HttpMethod.Post, Path($"clients/{id}/secrets{(replace ? "/regenerate" : "")}"), request, ct);
    public Task DeleteSecretAsync(int id, int secretId, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Path($"clients/{id}/secrets/{secretId}"), ct: ct);
    public Task<List<AdministrationScope>> GetScopesAsync(CancellationToken ct = default)
        => api.CallAsync<List<AdministrationScope>>(HttpMethod.Get, Path("scopes"), ct: ct);
    public Task<AdministrationScope> SaveScopeAsync(int? id, ApiScopeSettings settings, CancellationToken ct = default)
        => api.CallAsync<AdministrationScope>(id.HasValue ? HttpMethod.Put : HttpMethod.Post,
            Path(id.HasValue ? $"scopes/{id}" : "scopes"), settings, ct);
    public Task DeleteScopeAsync(int id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Path($"scopes/{id}"), ct: ct);
    public Task<List<MailingTemplateSettings>> GetMailingTemplatesAsync(CancellationToken ct = default)
        => api.CallAsync<List<MailingTemplateSettings>>(HttpMethod.Get, Path("mailing-templates"), ct: ct);
    public Task<List<string>> GetMailingCriteriaAsync(CancellationToken ct = default)
        => api.CallAsync<List<string>>(HttpMethod.Get, Path("mailing-criteria"), ct: ct);
    public Task<MailingTemplateSettings> SaveMailingTemplateAsync(string? id, MailingTemplateSettings settings, CancellationToken ct = default)
        => api.CallAsync<MailingTemplateSettings>(id is not null ? HttpMethod.Put : HttpMethod.Post,
            Path(id is not null ? $"mailing-templates/{Escape(id)}" : "mailing-templates"), settings, ct);
    public Task DeleteMailingTemplateAsync(string id, CancellationToken ct = default)
        => api.CallAsync(HttpMethod.Delete, Path($"mailing-templates/{Escape(id)}"), ct: ct);
    public Task<AdministrationTransfer> ExportAsync(CancellationToken ct = default)
        => api.CallAsync<AdministrationTransfer>(HttpMethod.Get, Path("transfer"), ct: ct);
    public Task<ImportResult> ImportAsync(AdministrationTransfer transfer, CancellationToken ct = default)
        => api.CallAsync<ImportResult>(HttpMethod.Post, Path("transfer"), transfer, ct);
}
