using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class AdministrationPageViewModel : ViewModelBase
{
    private readonly YavscApiClient? _api;
    private readonly ResourceUsageApiClient? _usageClient;
    private readonly SessionStatusViewModel? _session;

    public ObservableCollection<ResourceUsageSummary> UsageOverview { get; } = new();

    public ResourceUsageSummary? SelectedUsage =>
        UsageOverview.FirstOrDefault(usage => usage.UserId == SelectedUser?.UserId);

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<UserInfo> Users { get; set; } = new();

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Chargement…";

    [ObservableProperty]
    public partial UserInfo? SelectedUser { get; set; }

    public IReadOnlyList<UserInfo> FilteredUsers =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Users.ToList()
            : Users.Where(user =>
                    (user.UserName ?? string.Empty).Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                 || (user.FullName ?? string.Empty).Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                 || (user.Email ?? string.Empty).Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                 || (user.UserId ?? string.Empty).Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public AdministrationPageViewModel(YavscApiClient? api,
        ResourceUsageApiClient? usageClient = null, SessionStatusViewModel? session = null)
    {
        _api = api;
        _usageClient = usageClient;
        _session = session;
    }

    public AdministrationPageViewModel() : this(null) { }

    public Task InitializeAsync() => RefreshAsync();

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredUsers));
    }

    partial void OnSelectedUserChanged(UserInfo? value) => OnPropertyChanged(nameof(SelectedUsage));

    [RelayCommand]
    public async Task RefreshAsync()
    {
        Users.Clear();
        UsageOverview.Clear();
        SelectedUser = null;
        OnPropertyChanged(nameof(FilteredUsers));
        OnPropertyChanged(nameof(SelectedUsage));
        if (_session?.IsModerator != true)
        {
            StatusMessage = "Le rôle Moderator est requis.";
            return;
        }
        if (_api is null || _usageClient is null)
        {
            StatusMessage = "Client API d’administration indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var query = SearchText.Trim();
            var usersEndpoint = string.IsNullOrWhiteSpace(query)
                ? new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "users").ToString()
                : new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), $"users/search/{Uri.EscapeDataString(query)}").ToString();

            var users = await _api.CallAsync<List<UserInfo>>(HttpMethod.Get, usersEndpoint).ConfigureAwait(true)
                ?? throw new InvalidOperationException("Le serveur a renvoyé une liste d’utilisateurs vide.");
            var usage = await _usageClient.GetAdminOverviewAsync().ConfigureAwait(true)
                ?? throw new InvalidOperationException("Le serveur a renvoyé une liste d’usages vide.");

            if (!_session.IsModerator)
            {
                StatusMessage = "Le rôle Moderator est requis.";
                return;
            }

            foreach (var user in users)
            {
                Users.Add(user);
            }
            foreach (var item in usage)
                UsageOverview.Add(item);

            OnPropertyChanged(nameof(FilteredUsers));
            OnPropertyChanged(nameof(SelectedUsage));
            StatusMessage = Users.Count > 0 ? $"{Users.Count} utilisateur(s) chargé(s)." : "Aucun utilisateur trouvé.";
        }
        catch (Exception ex)
        {
            Users.Clear();
            OnPropertyChanged(nameof(FilteredUsers));
            StatusMessage = $"Accès administration refusé ou indisponible : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
