using System.Net.Http;
using PostIt.ViewModels;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class EstimateListPageViewModelTests
{
    [Fact]
    public async Task Client_perspective_calls_asclient_endpoint()
    {
        var api = new StubEstimateListApi();
        var client = new EstimateApiClient(api, "https://business.example/api/v1/");
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateListPageViewModel(client, EstimateListPerspective.Client, front);

        await vm.InitializeAsync();

        Assert.Contains("https://business.example/api/v1/estimate/asclient", api.Paths);
        Assert.Equal("Mes devis à valider", vm.Title);
        Assert.Equal(2, vm.Estimates.Count);
        // Most recent first
        Assert.Equal(2, vm.Estimates[0].Id);
        Assert.Equal(1, vm.Estimates[1].Id);
    }

    [Fact]
    public async Task Provider_perspective_calls_asprovider_endpoint()
    {
        var api = new StubEstimateListApi();
        var client = new EstimateApiClient(api, "https://business.example/api/v1/");
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateListPageViewModel(client, EstimateListPerspective.Provider, front);

        await vm.InitializeAsync();

        Assert.Contains("https://business.example/api/v1/estimate/asprovider", api.Paths);
        Assert.Equal("Mes devis en attente", vm.Title);
        Assert.Equal(2, vm.Estimates.Count);
    }

    [Fact]
    public async Task FilterText_filters_by_title_and_counterpart()
    {
        var api = new StubEstimateListApi();
        var client = new EstimateApiClient(api, "https://business.example/api/v1/");
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateListPageViewModel(client, EstimateListPerspective.Provider, front);

        await vm.InitializeAsync();

        vm.FilterText = "plomberie";
        Assert.Single(vm.Estimates);
        Assert.Equal(1, vm.Estimates[0].Id);

        vm.FilterText = "BOB";
        Assert.Single(vm.Estimates);
        Assert.Equal("bob", vm.Estimates[0].ClientId);

        vm.FilterText = string.Empty;
        Assert.Equal(2, vm.Estimates.Count);
    }

    [Fact]
    public async Task Empty_result_sets_an_info_status()
    {
        var api = new StubEstimateListApi { Empty = true };
        var client = new EstimateApiClient(api, "https://business.example/api/v1/");
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateListPageViewModel(client, EstimateListPerspective.Client, front);

        await vm.InitializeAsync();

        Assert.Empty(vm.Estimates);
        Assert.Contains("Aucun devis", vm.StatusMessage);
    }

    private sealed class StubEstimateListApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public List<string> Paths { get; } = new();
        public bool Empty { get; init; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Paths.Add(path);

            if (typeof(T) == typeof(List<EstimateDto>))
            {
                var items = Empty
                    ? new List<EstimateDto>()
                    : new List<EstimateDto>
                    {
                        new()
                        {
                            Id = 1,
                            Title = "Devis plomberie",
                            Description = "Réparation fuite",
                            ClientId = "bob",
                            OwnerId = "alice",
                            CommandType = "Rdv",
                            Bill = new List<EstimateLineDto>
                            {
                                new() { Name = "Main d'œuvre", Description = "Intervention", Count = 1, UnitaryCost = 80m },
                            },
                        },
                        new()
                        {
                            Id = 2,
                            Title = "Devis électricité",
                            Description = "Mise aux normes",
                            ClientId = "carol",
                            OwnerId = "alice",
                            CommandType = "Rdv",
                            Bill = new List<EstimateLineDto>(),
                        },
                    };
                return Task.FromResult((T)(object)items);
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
