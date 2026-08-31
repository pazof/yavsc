using System.Net.Http;
using PostIt.ViewModels;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class ActivitiesPageViewModelTests
{
    [Fact]
    public async Task ActivityApiClient_uses_business_absolute_paths()
    {
        var api = new StubActivityApi();
        var client = new ActivityApiClient(api, "https://business.example/api/v1/");
        var billingClient = new BillingApiClient(api, "https://business.example/api/v1/");

        await client.GetCatalogAsync("brush", TestContext.Current.CancellationToken);
        await client.GetUsersAsync("brush-pro", TestContext.Current.CancellationToken);
        await billingClient.CreateAsync("Rdv", new { Foo = "Bar" }, TestContext.Current.CancellationToken);
        await billingClient.GetQuerySummariesAsync("Rdv", TestContext.Current.CancellationToken);

        Assert.Equal("https://business.example/api/v1/activity/catalog?parentCode=brush", api.Paths[0]);
        Assert.Equal("https://business.example/api/v1/activity/brush-pro/users", api.Paths[1]);
        Assert.Equal("https://business.example/api/v1/billing/Rdv", api.Paths[2]);
        Assert.Equal("https://business.example/api/v1/billing/Rdv", api.Paths[3]);
    }

    [Fact]
    public async Task RefreshAsync_loads_first_activity_then_specialization_performers()
    {
        var api = new StubActivityApi();
        var client = new ActivityApiClient(api, "https://business.example/api/v1/");
        var billingClient = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new ActivitiesPageViewModel(client, billingClient);

        await vm.RefreshAsync();

        Assert.Equal("brush", vm.SelectedActivity?.Code);
        Assert.Single(vm.Specializations);
        Assert.Equal("brush", vm.CurrentActivity?.Code);
        Assert.Single(vm.Performers);
        Assert.Equal("Alice", vm.Performers[0].UserName);
        Assert.True(vm.Performers[0].HasPerformerProfile);
        Assert.True(vm.Performers[0].IsPerformerActive);
        Assert.Equal("Actif", vm.Performers[0].PerformerStatusBadgeLabel);
        Assert.Equal("Pas d'autre activité", vm.Performers[0].ExtraActivityLabel);

        await vm.ShowSpecializationAsync(vm.Specializations[0]);

        Assert.Equal("brush-pro", vm.CurrentActivity?.Code);
        Assert.Single(vm.Performers);
        Assert.Equal("Bob", vm.Performers[0].UserName);
        Assert.True(vm.Performers[0].HasPerformerProfile);
        Assert.False(vm.Performers[0].IsPerformerActive);
        Assert.Equal("Inactif", vm.Performers[0].PerformerStatusBadgeLabel);
        Assert.Equal("Autres spécialisations: 2", vm.Performers[0].ExtraActivityLabel);
        Assert.Contains("brush pro", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);

        await vm.ShowSpecializationAsync(null);

        Assert.Equal("brush", vm.CurrentActivity?.Code);
        Assert.Single(vm.Performers);
        Assert.Equal("Alice", vm.Performers[0].UserName);
    }

    private sealed class StubActivityApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public List<string> Paths { get; } = new();

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add(path);

            if (typeof(T) == typeof(List<ActivityBrowseItemDto>))
            {
                var activities = new List<ActivityBrowseItemDto>
                {
                    new()
                    {
                        Code = "brush",
                        Name = "Brush",
                        Description = "Coiffure à domicile",
                        PerformerCount = 1,
                        Forms = new List<CommandFormSummaryDto>
                        {
                            new() { Id = 1, ActionName = "Rdv", Title = "Rendez-vous" }
                        },
                        Children = new List<ActivityBrowseItemDto>
                        {
                            new()
                            {
                                Code = "brush-pro",
                                Name = "Brush Pro",
                                Description = "Spécialisation premium",
                                ParentCode = "brush",
                                PerformerCount = 1,
                                Forms = new List<CommandFormSummaryDto>
                                {
                                    new() { Id = 2, ActionName = "Rdv", Title = "Rendez-vous premium" }
                                }
                            }
                        }
                    }
                };
                return Task.FromResult((T)(object)activities);
            }

            if (typeof(T) == typeof(List<ActivityPerformerDto>))
            {
                var performers = path.EndsWith("brush-pro/users", StringComparison.Ordinal)
                    ? new List<ActivityPerformerDto>
                    {
                        new() { PerformerId = "pro-2", HasPerformerProfile = true, Active = false, UserName = "Bob", ActivityCode = "brush-pro", ActivityName = "Brush Pro", ExtraActivityCount = 2 }
                    }
                    : new List<ActivityPerformerDto>
                    {
                        new() { PerformerId = "pro-1", HasPerformerProfile = true, Active = true, UserName = "Alice", ActivityCode = "brush", ActivityName = "Brush", ExtraActivityCount = 0 }
                    };

                return Task.FromResult((T)(object)performers);
            }

            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add(path);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
