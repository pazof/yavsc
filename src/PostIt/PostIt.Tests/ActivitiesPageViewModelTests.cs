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

        await client.GetCatalogAsync("brush", TestContext.Current.CancellationToken);
        await client.GetPerformersAsync("brush-pro", TestContext.Current.CancellationToken);

        Assert.Equal("https://business.example/api/v1/activity/catalog?parentCode=brush", api.Paths[0]);
        Assert.Equal("https://business.example/api/v1/activity/brush-pro/performers", api.Paths[1]);
    }

    [Fact]
    public async Task RefreshAsync_loads_first_activity_then_specialization_performers()
    {
        var api = new StubActivityApi();
        var client = new ActivityApiClient(api, "https://business.example/api/v1/");
        var vm = new ActivitiesPageViewModel(client);

        await vm.RefreshAsync();

        Assert.Equal("brush", vm.SelectedActivity?.Code);
        Assert.Single(vm.Specializations);
        Assert.Equal("brush", vm.CurrentActivity?.Code);
        Assert.Single(vm.Performers);
        Assert.Equal("Alice", vm.Performers[0].UserName);

        await vm.ShowSpecializationAsync(vm.Specializations[0]);

        Assert.Equal("brush-pro", vm.CurrentActivity?.Code);
        Assert.Single(vm.Performers);
        Assert.Equal("Bob", vm.Performers[0].UserName);
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
                        Children = new List<ActivityBrowseItemDto>
                        {
                            new()
                            {
                                Code = "brush-pro",
                                Name = "Brush Pro",
                                Description = "Spécialisation premium",
                                ParentCode = "brush",
                                PerformerCount = 1,
                            }
                        }
                    }
                };
                return Task.FromResult((T)(object)activities);
            }

            if (typeof(T) == typeof(List<ActivityPerformerDto>))
            {
                var performers = path.EndsWith("brush-pro/performers", StringComparison.Ordinal)
                    ? new List<ActivityPerformerDto>
                    {
                        new() { PerformerId = "pro-2", UserName = "Bob", ActivityCode = "brush-pro", ActivityName = "Brush Pro" }
                    }
                    : new List<ActivityPerformerDto>
                    {
                        new() { PerformerId = "pro-1", UserName = "Alice", ActivityCode = "brush", ActivityName = "Brush" }
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
