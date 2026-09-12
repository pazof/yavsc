using System.Net.Http;
using PostIt.ViewModels;
using Yavsc;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class ProviderOngoingRequestsPageViewModelTests
{
    [Fact]
    public async Task RefreshAsync_calls_provider_endpoint_and_filters_out_unknown_billing_codes()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();

        Assert.Contains("https://business.example/api/v1/bill/provider/ongoing", api.Paths);
        Assert.Equal(3, vm.Queries.Count);
        Assert.Equal(12, vm.Queries[0].Id);
        Assert.Equal(11, vm.Queries[1].Id);
        Assert.Equal(10, vm.Queries[2].Id);
    }

    [Fact]
    public async Task FilterText_filters_by_activity_code_and_status()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();

        vm.FilterText = "mbrush";
        Assert.Single(vm.Queries);
        Assert.Equal("MBrush", vm.Queries[0].BillingCode);

        vm.FilterText = "accepted";
        Assert.Single(vm.Queries);
        Assert.Equal(QueryStatus.Accepted, vm.Queries[0].Status);
    }

    [Fact]
    public async Task OpenSelectedEditorCommand_can_execute_only_when_selection_exists()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();

        Assert.False(vm.OpenSelectedEditorCommand.CanExecute(null));

        vm.SelectedQuery = vm.Queries[0];

        Assert.True(vm.OpenSelectedEditorCommand.CanExecute(null));
    }

    [Fact]
    public async Task SelectedSortOption_date_keeps_most_recent_first()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();
        vm.SelectedSortOption = ProviderOngoingRequestsPageViewModel.SortByDate;

        Assert.Equal(new long[] { 12, 11, 10 }, vm.Queries.Select(q => q.Id).ToArray());
    }

    [Fact]
    public async Task SelectedSortOption_date_ascending_keeps_oldest_first()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();
        vm.SelectedSortOption = ProviderOngoingRequestsPageViewModel.SortByDateAsc;

        Assert.Equal(new long[] { 10, 11, 12 }, vm.Queries.Select(q => q.Id).ToArray());
    }

    [Fact]
    public async Task SelectedSortOption_status_prioritizes_inprogress_then_accepted_then_inserted()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ProviderOngoingRequestsPageViewModel(client);

        await vm.InitializeAsync();
        vm.SelectedSortOption = ProviderOngoingRequestsPageViewModel.SortByStatus;

        Assert.Equal(new long[] { 11, 12, 10 }, vm.Queries.Select(q => q.Id).ToArray());
    }

    [Fact]
    public void Constructor_reads_saved_sort_option_from_settings()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var settings = new Settings
        {
            ProviderOngoingRequestsSortOption = ProviderOngoingRequestsPageViewModel.SortByStatus,
        };

        var vm = new ProviderOngoingRequestsPageViewModel(client, settings);

        Assert.Equal(ProviderOngoingRequestsPageViewModel.SortByStatus, vm.SelectedSortOption);
    }

    [Fact]
    public void Constructor_falls_back_to_default_when_saved_sort_is_invalid()
    {
        var api = new StubProviderApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var settings = new Settings
        {
            ProviderOngoingRequestsSortOption = "invalide",
        };

        var vm = new ProviderOngoingRequestsPageViewModel(client, settings);

        Assert.Equal(ProviderOngoingRequestsPageViewModel.SortByDate, vm.SelectedSortOption);
    }

    private sealed class StubProviderApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public List<string> Paths { get; } = new();

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add(path);

            if (typeof(T) == typeof(List<BillingQuerySummaryDto>))
            {
                var data = new List<BillingQuerySummaryDto>
                {
                    new()
                    {
                        Id = 10,
                        BillingCode = "Rdv",
                        ActivityCode = "dev",
                        PerformerId = "perf-1",
                        ClientId = "cli-1",
                        Status = QueryStatus.Inserted,
                        Description = "Rendez-vous",
                        EventDate = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 11,
                        BillingCode = "MBrush",
                        ActivityCode = "hair",
                        PerformerId = "perf-1",
                        ClientId = "cli-2",
                        Status = QueryStatus.InProgress,
                        Description = "Coupe multiple",
                        EventDate = new DateTime(2026, 9, 11, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 12,
                        BillingCode = "Brush",
                        ActivityCode = "hair",
                        PerformerId = "perf-1",
                        ClientId = "cli-3",
                        Status = QueryStatus.Accepted,
                        Description = "Coupe simple",
                        EventDate = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 13,
                        BillingCode = "",
                        ActivityCode = "unknown",
                        PerformerId = "perf-1",
                        ClientId = "cli-4",
                        Status = QueryStatus.Accepted,
                        Description = "Doit être filtrée",
                        EventDate = new DateTime(2026, 9, 13, 10, 0, 0, DateTimeKind.Utc),
                    }
                };

                return Task.FromResult((T)(object)data);
            }

            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add(path);
            return Task.CompletedTask;
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<T>(method, path, (object?)null, ct);

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync(method, path, (object?)null, ct);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
