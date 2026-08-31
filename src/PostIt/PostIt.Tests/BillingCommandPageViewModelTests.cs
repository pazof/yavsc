using System.Net.Http;
using System.Text.Json;
using PostIt.ViewModels;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class BillingCommandPageViewModelTests
{
    [Fact]
    public async Task SubmitAsync_posts_rdv_payload_to_selected_billing_route()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingCommandPageViewModel(
            new ActivityBrowseItemDto { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            new CommandFormSummaryDto { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" },
            client)
        {
            EventDateText = "2026-09-02 14:30",
            Reason = "Point de cadrage",
            Address = "1 rue du Test",
            LatitudeText = "48.8566",
            LongitudeText = "2.3522",
            Consent = true,
        };

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("https://business.example/api/v1/billing/Rdv", api.LastPath);
        Assert.NotNull(api.LastBody);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(api.LastBody));
        Assert.Equal("dev", json.RootElement.GetProperty("ActivityCode").GetString());
        Assert.Equal("perf-1", json.RootElement.GetProperty("PerformerId").GetString());
        Assert.Equal("Point de cadrage", json.RootElement.GetProperty("Reason").GetString());
        Assert.Equal((int)QueryStatus.Inserted, json.RootElement.GetProperty("Status").GetInt32());
    }

    [Fact]
    public async Task SubmitAsync_refuses_unsupported_billing_code()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingCommandPageViewModel(
            new ActivityBrowseItemDto { Code = "brush", Name = "Brush" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            new CommandFormSummaryDto { Id = 13, ActionName = "Brush", Title = "Coupe" },
            client);

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Null(api.LastPath);
        Assert.Contains("n'est pas encore pris en charge", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public string? LastPath { get; private set; }
        public object? LastBody { get; private set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastPath = path;
            LastBody = body;
            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastPath = path;
            LastBody = body;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}