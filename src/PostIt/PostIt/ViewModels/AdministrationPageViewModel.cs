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

namespace PostIt.ViewModels;

public partial class AdministrationPageViewModel : ViewModelBase
{
    private readonly YavscApiClient? _api;

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

    public AdministrationPageViewModel(YavscApiClient? api)
    {
        _api = api;
    }

    public AdministrationPageViewModel() : this(null) { }

    public Task InitializeAsync() => RefreshAsync();

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredUsers));
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (_api is null)
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

            var users = await _api.CallAsync<List<UserInfo>>(HttpMethod.Get, usersEndpoint).ConfigureAwait(true);

            Users.Clear();
            foreach (var user in users ?? [])
            {
                Users.Add(user);
            }

            OnPropertyChanged(nameof(FilteredUsers));
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
