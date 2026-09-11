using System.Net.Http;
using PostIt.ViewModels;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class BillingQueriesPageViewModelTests
{
    [Fact]
    public async Task RefreshAsync_filters_queries_by_selected_activity_and_performer()
    {
        var api = new StubBillingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingQueriesPageViewModel(
            new ActivityInfo { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            new CommandFormSummary { Id = 1, ActionName = "Rdv", Title = "Rendez-vous" },
            client);

        await vm.InitializeAsync();

        Assert.Equal("https://business.example/api/v1/billing/Rdv", api.Paths.Single());
        Assert.Equal(3, vm.Queries.Count);
        Assert.Contains(vm.Queries, q => q.Description == "Rendez-vous #1");
        Assert.Contains("3 commande", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshAsync_in_readonly_ongoing_mode_keeps_only_ongoing_statuses_and_disables_open()
    {
        var api = new StubBillingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingQueriesPageViewModel(
            new ActivityInfo { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            new CommandFormSummary { Id = 1, ActionName = "Rdv", Title = "Rendez-vous" },
            client,
            isReadOnly: true,
            ongoingOnly: true);

        await vm.InitializeAsync();

        Assert.Equal(2, vm.Queries.Count);
        Assert.All(vm.Queries, q => Assert.DoesNotContain("Rejected", q.StatusLabel, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("lecture seule", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);

        Assert.True(vm.Queries.Count > 0);
    }

    private sealed class StubBillingApi : IYavscApiClient
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
                        Id = 11,
                        ActivityCode = "dev",
                        PerformerId = "perf-1",
                        ClientId = "cli-1",
                        Status = QueryStatus.Inserted,
                        Description = "Rendez-vous #1",
                        Reason = "Point de cadrage",
                        EventDate = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 12,
                        ActivityCode = "other",
                        PerformerId = "perf-1",
                        ClientId = "cli-1",
                        Status = QueryStatus.Accepted,
                        Description = "Autre activité",
                        EventDate = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 13,
                        ActivityCode = "dev",
                        PerformerId = "perf-2",
                        ClientId = "cli-1",
                        Status = QueryStatus.Accepted,
                        Description = "Autre performer",
                        EventDate = new DateTime(2026, 9, 3, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 14,
                        ActivityCode = "dev",
                        PerformerId = "perf-1",
                        ClientId = "cli-1",
                        Status = QueryStatus.InProgress,
                        Description = "En cours",
                        EventDate = new DateTime(2026, 9, 4, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        Id = 15,
                        ActivityCode = "dev",
                        PerformerId = "perf-1",
                        ClientId = "cli-1",
                        Status = QueryStatus.Rejected,
                        Description = "Rejetée",
                        EventDate = new DateTime(2026, 9, 5, 10, 0, 0, DateTimeKind.Utc),
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
