using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ResourceUsagePageViewModel : ViewModelBase
{
    private readonly ResourceUsageApiClient _client;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsAdminView { get; set; }

    [ObservableProperty]
    public partial ResourceUsageSummary? CurrentSummary { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ResourceUsageSummary> Overview { get; set; } = new();

    public string Heading => IsAdminView ? "Vue d'administration des usages" : "Mon usage des ressources";

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public ResourceUsagePageViewModel(ResourceUsageApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            Overview.Clear();

            try
            {
                var adminOverview = await _client.GetAdminOverviewAsync().ConfigureAwait(true);
                if (adminOverview is { Count: > 0 })
                {
                    foreach (var item in adminOverview)
                    {
                        Overview.Add(item);
                    }

                    CurrentSummary = adminOverview[0];
                    IsAdminView = true;
                    return;
                }
            }
            catch (Exception)
            {
                // Non-admin users are not allowed on the admin route; fall back
                // to the current-user summary below.
            }

            var current = await _client.GetCurrentAsync().ConfigureAwait(true);
            CurrentSummary = current;
            if (current is not null)
            {
                Overview.Clear();
                Overview.Add(current);
            }

            IsAdminView = false;
        }
        catch (Exception ex)
        {
            CurrentSummary = null;
            Overview.Clear();
            IsAdminView = false;
            throw new InvalidOperationException($"Impossible de charger l'usage des ressources : {ex.Message}", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
