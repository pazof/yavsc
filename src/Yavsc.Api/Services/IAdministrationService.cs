using Yavsc.Abstract.Administration;

namespace Yavsc.Api.Services;

public interface IAdministrationService
{
    Task<AdministrationSummary> GetSummaryAsync(CancellationToken ct);
    Task<IReadOnlyList<AdministrationRole>> GetRolesAsync(CancellationToken ct);
    Task EnsureRolesAsync(CancellationToken ct);
    Task<IReadOnlyList<RoleMember>> GetRoleMembersAsync(string roleId, CancellationToken ct);
    Task SetRoleMembershipAsync(string roleId, string userId, bool member, CancellationToken ct);
    Task<IReadOnlyList<AdministrationClient>> GetClientsAsync(CancellationToken ct);
    Task<AdministrationClient?> GetClientAsync(int id, CancellationToken ct);
    Task<AdministrationClient> SaveClientAsync(int? id, ClientSettings settings, CancellationToken ct);
    Task DeleteClientAsync(int id, CancellationToken ct);
    Task AddClientEntryAsync(int id, ClientCollection collection, ClientEntry entry, CancellationToken ct);
    Task DeleteClientEntryAsync(int id, ClientCollection collection, int rowId, CancellationToken ct);
    Task<NewClientSecret> CreateSecretAsync(int id, ClientSecretRequest request, bool replace, CancellationToken ct);
    Task DeleteSecretAsync(int id, int secretId, CancellationToken ct);
    Task<IReadOnlyList<AdministrationScope>> GetScopesAsync(CancellationToken ct);
    Task<AdministrationScope> SaveScopeAsync(int? id, ApiScopeSettings settings, CancellationToken ct);
    Task DeleteScopeAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<MailingTemplateSettings>> GetMailingTemplatesAsync(CancellationToken ct);
    IReadOnlyList<string> GetMailingCriteria();
    Task<MailingTemplateSettings> SaveMailingTemplateAsync(string? id, MailingTemplateSettings settings, CancellationToken ct);
    Task DeleteMailingTemplateAsync(string id, CancellationToken ct);
    Task<AdministrationTransfer> ExportAsync(CancellationToken ct);
    Task<ImportResult> ImportAsync(AdministrationTransfer transfer, CancellationToken ct);
}

public sealed class AdministrationOperationException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
