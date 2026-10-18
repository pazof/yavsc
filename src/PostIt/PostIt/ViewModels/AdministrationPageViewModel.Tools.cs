using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Abstract.Administration;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class AdministrationPageViewModel
{
    [ObservableProperty] public partial AdministrationSummary? Summary { get; set; }
    [ObservableProperty] public partial AdministrationRole? SelectedRole { get; set; }
    [ObservableProperty] public partial RoleMember? SelectedMember { get; set; }
    [ObservableProperty] public partial string EnrollUserId { get; set; } = "";
    [ObservableProperty] public partial AdministrationClient? SelectedClient { get; set; }
    [ObservableProperty] public partial AdministrationScope? SelectedScope { get; set; }
    [ObservableProperty] public partial MailingTemplateSettings? SelectedMailingTemplate { get; set; }
    [ObservableProperty] public partial ClientCollection SelectedCollection { get; set; }
    [ObservableProperty] public partial ClientEntryResponse? SelectedEntry { get; set; }
    [ObservableProperty] public partial ClientSecretInfo? SelectedSecret { get; set; }
    [ObservableProperty] public partial string EntryKey { get; set; } = "";
    [ObservableProperty] public partial string EntryValue { get; set; } = "";
    [ObservableProperty] public partial string SecretInput { get; set; } = "";
    [ObservableProperty] public partial string SecretDescription { get; set; } = "";
    [ObservableProperty] public partial string SecretExpiration { get; set; } = "";
    [ObservableProperty] public partial string? NewSecretValue { get; set; }
    [ObservableProperty] public partial bool ConfirmDestructiveAction { get; set; }
    [ObservableProperty] public partial IReadOnlyList<AdministrationFieldViewModel> ClientFields { get; set; } = [];
    [ObservableProperty] public partial IReadOnlyList<AdministrationFieldViewModel> ScopeFields { get; set; } = [];
    [ObservableProperty] public partial IReadOnlyList<AdministrationFieldViewModel> MailingFields { get; set; } = [];

    public ObservableCollection<AdministrationRole> Roles { get; } = [];
    public ObservableCollection<RoleMember> RoleMembers { get; } = [];
    public ObservableCollection<AdministrationClient> Clients { get; } = [];
    public ObservableCollection<AdministrationScope> Scopes { get; } = [];
    public ObservableCollection<MailingTemplateSettings> MailingTemplates { get; } = [];
    public ObservableCollection<string> MailingCriteria { get; } = [];
    public ClientCollection[] Collections { get; } = Enum.GetValues<ClientCollection>();

    private AdministrationApiClient RequireAccess()
    {
        if (_session?.IsAdministrator != true)
            throw new UnauthorizedAccessException("Le rôle Administrator est requis.");
        return _administrationClient ?? throw new InvalidOperationException("Client API d’administration indisponible.");
    }

    private async Task LoadToolsAsync()
    {
        var client = RequireAccess();
        var summary = await client.GetSummaryAsync();
        var roles = await client.GetRolesAsync();
        var clients = await client.GetClientsAsync();
        var scopes = await client.GetScopesAsync();
        var templates = await client.GetMailingTemplatesAsync();
        var criteria = await client.GetMailingCriteriaAsync();
        RequireAccess();
        Summary = summary;
        Replace(Roles, roles);
        Replace(Clients, clients);
        Replace(Scopes, scopes);
        Replace(MailingTemplates, templates);
        Replace(MailingCriteria, criteria);
        SelectedRole = null;
        RoleMembers.Clear();
        SelectedClient = null;
        SelectedScope = null;
        SelectedMailingTemplate = null;
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }

    private async Task RunOperationAsync(Func<AdministrationApiClient, Task> operation)
    {
        if (IsBusy)
        {
            StatusMessage = "Une opération d’administration est déjà en cours.";
            return;
        }
        IsBusy = true;
        NewSecretValue = null;
        try
        {
            var client = RequireAccess();
            await operation(client);
            RequireAccess();
            StatusMessage = "Opération effectuée.";
        }
        catch (Exception ex)
        {
            if (_session?.IsAdministrator != true)
                ClearAdministrationData();
            StatusMessage = $"Administration : {ex.Message}";
        }
        finally
        {
            SecretInput = "";
            ConfirmDestructiveAction = false;
            IsBusy = false;
        }
    }

    private void RequireConfirmation()
    {
        if (!ConfirmDestructiveAction)
            throw new InvalidOperationException("Cochez la confirmation avant cette opération destructive.");
    }

    partial void OnSelectedClientChanged(AdministrationClient? value)
    {
        ClientFields = AdministrationFieldViewModel.From(value?.Settings ?? new ClientSettings());
        SelectedEntry = null;
        SelectedSecret = null;
        NewSecretValue = null;
        SecretInput = "";
        ConfirmDestructiveAction = false;
    }

    partial void OnSelectedScopeChanged(AdministrationScope? value)
        => ScopeFields = AdministrationFieldViewModel.From(value?.Settings ?? new ApiScopeSettings());

    partial void OnSelectedMailingTemplateChanged(MailingTemplateSettings? value)
        => MailingFields = AdministrationFieldViewModel.From(value ?? new MailingTemplateSettings());

    [RelayCommand]
    private void NewClient()
    {
        SelectedClient = null;
        ClientFields = AdministrationFieldViewModel.From(new ClientSettings());
    }

    [RelayCommand]
    private void NewScope()
    {
        SelectedScope = null;
        ScopeFields = AdministrationFieldViewModel.From(new ApiScopeSettings());
    }

    [RelayCommand]
    private void NewMailingTemplate()
    {
        SelectedMailingTemplate = null;
        MailingFields = AdministrationFieldViewModel.From(new MailingTemplateSettings());
    }

    [RelayCommand]
    private Task EnsureRolesAsync() => RunOperationAsync(async client =>
    {
        await client.EnsureRolesAsync();
        await ReloadRolesAsync(client);
    });

    private async Task ReloadRolesAsync(AdministrationApiClient client)
    {
        var selectedId = SelectedRole?.Id;
        Replace(Roles, await client.GetRolesAsync());
        SelectedRole = Roles.FirstOrDefault(role => role.Id == selectedId);
    }

    [RelayCommand]
    private Task LoadRoleMembersAsync() => RunOperationAsync(async client =>
    {
        var role = SelectedRole ?? throw new InvalidOperationException("Choisissez un rôle.");
        var members = await client.GetRoleMembersAsync(role.Id);
        RequireAccess();
        Replace(RoleMembers, members);
    });

    [RelayCommand]
    private Task EnrollAsync() => RunOperationAsync(async client =>
    {
        var role = SelectedRole ?? throw new InvalidOperationException("Choisissez un rôle.");
        var id = string.IsNullOrWhiteSpace(EnrollUserId) ? SelectedUser?.UserId : EnrollUserId.Trim();
        if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Sélectionnez un utilisateur ou indiquez son identifiant.");
        await client.SetRoleMembershipAsync(role.Id, id, true);
        Replace(RoleMembers, await client.GetRoleMembersAsync(role.Id));
        await ReloadRolesAsync(client);
    });

    [RelayCommand]
    private Task FireAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var role = SelectedRole ?? throw new InvalidOperationException("Choisissez un rôle.");
        var member = SelectedMember ?? throw new InvalidOperationException("Choisissez un membre.");
        await client.SetRoleMembershipAsync(role.Id, member.Id, false);
        Replace(RoleMembers, await client.GetRoleMembersAsync(role.Id));
        await ReloadRolesAsync(client);
    });

    [RelayCommand]
    private Task SaveClientAsync() => RunOperationAsync(async client =>
    {
        var saved = await client.SaveClientAsync(SelectedClient?.Id, AdministrationFieldViewModel.Build<ClientSettings>(ClientFields));
        Replace(Clients, await client.GetClientsAsync());
        SelectedClient = Clients.Single(item => item.Id == saved.Id);
    });

    [RelayCommand]
    private Task DeleteClientAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var selected = SelectedClient ?? throw new InvalidOperationException("Choisissez un client.");
        await client.DeleteClientAsync(selected.Id);
        Replace(Clients, await client.GetClientsAsync());
        NewClient();
    });

    private async Task ReloadClientAsync(AdministrationApiClient client, int id)
    {
        var clients = await client.GetClientsAsync();
        RequireAccess();
        Replace(Clients, clients);
        SelectedClient = Clients.Single(item => item.Id == id);
    }

    [RelayCommand]
    private Task AddEntryAsync() => RunOperationAsync(async client =>
    {
        var selected = SelectedClient ?? throw new InvalidOperationException("Enregistrez d’abord le client.");
        if (string.IsNullOrWhiteSpace(EntryValue)) throw new InvalidOperationException("La valeur est requise.");
        await client.AddClientEntryAsync(selected.Id, SelectedCollection,
            new ClientEntry { Key = string.IsNullOrWhiteSpace(EntryKey) ? null : EntryKey, Value = EntryValue });
        await ReloadClientAsync(client, selected.Id);
        EntryValue = "";
        EntryKey = "";
    });

    [RelayCommand]
    private Task DeleteEntryAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var selected = SelectedClient ?? throw new InvalidOperationException("Choisissez un client.");
        var entry = SelectedEntry ?? throw new InvalidOperationException("Choisissez une entrée.");
        await client.DeleteClientEntryAsync(selected.Id, entry.Collection, entry.Id);
        await ReloadClientAsync(client, selected.Id);
    });

    private ClientSecretRequest SecretRequest()
    {
        DateTimeOffset? expiration = null;
        if (!string.IsNullOrWhiteSpace(SecretExpiration))
        {
            if (!DateTimeOffset.TryParse(SecretExpiration, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed))
                throw new InvalidOperationException("Expiration invalide : utilisez une date ISO 8601 avec fuseau.");
            expiration = parsed;
        }
        return new() { Value = SecretInput, Description = SecretDescription, Expiration = expiration };
    }

    private Task CreateSecretAsync(bool replace) => RunOperationAsync(async client =>
    {
        if (replace) RequireConfirmation();
        var selected = SelectedClient ?? throw new InvalidOperationException("Choisissez un client.");
        var secret = await client.CreateSecretAsync(selected.Id, SecretRequest(), replace);
        RequireAccess();
        await ReloadClientAsync(client, selected.Id);
        NewSecretValue = secret.Value;
    });

    [RelayCommand] private Task AddSecretAsync() => CreateSecretAsync(false);
    [RelayCommand] private Task RegenerateSecretAsync() => CreateSecretAsync(true);
    [RelayCommand] private void ClearSecret() => NewSecretValue = null;

    [RelayCommand]
    private Task DeleteSecretAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var selected = SelectedClient ?? throw new InvalidOperationException("Choisissez un client.");
        var secret = SelectedSecret ?? throw new InvalidOperationException("Choisissez un secret.");
        await client.DeleteSecretAsync(selected.Id, secret.Id);
        await ReloadClientAsync(client, selected.Id);
    });

    [RelayCommand]
    private Task SaveScopeAsync() => RunOperationAsync(async client =>
    {
        var saved = await client.SaveScopeAsync(SelectedScope?.Id, AdministrationFieldViewModel.Build<ApiScopeSettings>(ScopeFields));
        Replace(Scopes, await client.GetScopesAsync());
        SelectedScope = Scopes.Single(item => item.Id == saved.Id);
    });

    [RelayCommand]
    private Task DeleteScopeAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var selected = SelectedScope ?? throw new InvalidOperationException("Choisissez un scope.");
        await client.DeleteScopeAsync(selected.Id);
        Replace(Scopes, await client.GetScopesAsync());
        NewScope();
    });

    [RelayCommand]
    private Task SaveMailingTemplateAsync() => RunOperationAsync(async client =>
    {
        var saved = await client.SaveMailingTemplateAsync(SelectedMailingTemplate?.Id,
            AdministrationFieldViewModel.Build<MailingTemplateSettings>(MailingFields));
        Replace(MailingTemplates, await client.GetMailingTemplatesAsync());
        SelectedMailingTemplate = MailingTemplates.Single(item => item.Id == saved.Id);
    });

    [RelayCommand]
    private Task DeleteMailingTemplateAsync() => RunOperationAsync(async client =>
    {
        RequireConfirmation();
        var selected = SelectedMailingTemplate ?? throw new InvalidOperationException("Choisissez un modèle.");
        await client.DeleteMailingTemplateAsync(selected.Id);
        Replace(MailingTemplates, await client.GetMailingTemplatesAsync());
        NewMailingTemplate();
    });

    public async Task<AdministrationTransfer> ExportTransferAsync()
    {
        var data = await RequireAccess().ExportAsync();
        RequireAccess();
        return data;
    }

    public async Task<ImportResult> ImportTransferAsync(AdministrationTransfer transfer)
    {
        RequireConfirmation();
        var result = await RequireAccess().ImportAsync(transfer);
        ConfirmDestructiveAction = false;
        return result;
    }
}
