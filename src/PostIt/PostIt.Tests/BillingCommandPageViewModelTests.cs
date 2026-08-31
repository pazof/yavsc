using System.Net.Http;
using System.Text.Json;
using PostIt.ViewModels;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;
using Yavsc.Models.Haircut;

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
            new ActivityBrowseItemDto { Code = "book", Name = "Book" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            new CommandFormSummaryDto { Id = 13, ActionName = "Book", Title = "Réservation" },
            client);

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Null(api.LastPath);
        Assert.Contains("n'est pas encore pris en charge", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_loads_prestations_for_brush_and_submit_posts_selected_prestation()
    {
        var api = new RecordingApi
        {
            HairPrestations = new List<HairPrestationDto>
            {
                new() { Id = 10, Title = "Femme · Cheveux mi-longs", Details = "Coupe · Brushing" },
                new() { Id = 11, Title = "Homme · Cheveux courts", Details = "Coupe · Coiffage" },
            }
        };
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingCommandPageViewModel(
            new ActivityBrowseItemDto { Code = "brush", Name = "Brush" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            new CommandFormSummaryDto { Id = 13, ActionName = "Brush", Title = "Coupe" },
            client)
        {
            EventDateText = "2026-09-02 14:30",
            Address = "1 rue du Test",
            LatitudeText = "48.8566",
            LongitudeText = "2.3522",
            Consent = true,
            AdditionalInfo = "Prévoir shampoing",
        };

        await vm.InitializeAsync();
        vm.SelectedPrestation = vm.AvailablePrestations[1];
        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("https://business.example/api/v1/billing/Brush", api.LastPath);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(api.LastBody));
        Assert.Equal(11, json.RootElement.GetProperty("PrestationId").GetInt32());
        Assert.Equal("Prévoir shampoing", json.RootElement.GetProperty("AdditionalInfo").GetString());
    }

    [Fact]
    public async Task InitializeAsync_loads_prestations_for_mbrush_and_submit_posts_selected_prestations()
    {
        var api = new RecordingApi
        {
            HairPrestations = new List<HairPrestationDto>
            {
                new() { Id = 21, Title = "Femme · Cheveux longs", Details = "Coupe · Couleur" },
                new() { Id = 22, Title = "Enfant · Cheveux courts", Details = "Coupe · Sans technique" },
            }
        };
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingCommandPageViewModel(
            new ActivityBrowseItemDto { Code = "mbrush", Name = "MBrush" },
            new ActivityUserDisplayItem { PerformerId = "perf-3", UserName = "Cara" },
            new CommandFormSummaryDto { Id = 14, ActionName = "MBrush", Title = "Coupe groupée" },
            client)
        {
            EventDateText = "2026-09-03 10:00",
            Address = "2 rue du Test",
            LatitudeText = "48.8567",
            LongitudeText = "2.3523",
            Consent = true,
        };

        await vm.InitializeAsync();
        vm.MultiPrestations[0].IsSelected = true;
        vm.MultiPrestations[1].IsSelected = true;
        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("https://business.example/api/v1/billing/MBrush", api.LastPath);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(api.LastBody));
        var prestations = json.RootElement.GetProperty("Prestations");
        Assert.Equal(2, prestations.GetArrayLength());
        Assert.Equal(21, prestations[0].GetProperty("PrestationId").GetInt32());
        Assert.Equal(22, prestations[1].GetProperty("PrestationId").GetInt32());
    }

    [Fact]
    public async Task InitializeAsync_with_existing_brush_query_prefills_and_submit_updates_query()
    {
        var api = new RecordingApi
        {
            HairPrestations = new List<HairPrestationDto>
            {
                new() { Id = 30, Title = "Femme · Cheveux longs", Details = "Coupe · Brushing" },
                new() { Id = 31, Title = "Homme · Cheveux courts", Details = "Coupe" },
            }
        };
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm = new BillingCommandPageViewModel(
            new ActivityBrowseItemDto { Code = "brush", Name = "Brush" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            new CommandFormSummaryDto { Id = 13, ActionName = "Brush", Title = "Coupe" },
            client);

        await vm.InitializeAsync(new BillingQueryDetailsDto
        {
            Id = 77,
            BillingCode = "Brush",
            ActivityCode = "brush",
            PerformerId = "perf-2",
            ClientId = "cli-1",
            EventDate = new DateTime(2026, 9, 2, 14, 30, 0, DateTimeKind.Utc),
            Consent = true,
            Status = QueryStatus.Accepted,
            PrestationId = 30,
            AdditionalInfo = "Ancienne note",
            Location = new BillingLocationDto
            {
                Address = "1 rue du Test",
                Latitude = 48.8566,
                Longitude = 2.3522,
            }
        });

        vm.SelectedPrestation = vm.AvailablePrestations[1];
        vm.AdditionalInfo = "Note mise à jour";
        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(HttpMethod.Put, api.LastMethod);
        Assert.Equal("https://business.example/api/v1/billing/Brush/77", api.LastPath);
        Assert.True(vm.IsEditingExisting);
        Assert.Equal("Mettre à jour la commande", vm.SubmitLabel);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(api.LastBody));
        Assert.Equal(77, json.RootElement.GetProperty("Id").GetInt32());
        Assert.Equal(31, json.RootElement.GetProperty("PrestationId").GetInt32());
        Assert.Equal("Note mise à jour", json.RootElement.GetProperty("AdditionalInfo").GetString());
        Assert.Equal((int)QueryStatus.Accepted, json.RootElement.GetProperty("Status").GetInt32());
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? LastMethod { get; private set; }
        public string? LastPath { get; private set; }
        public object? LastBody { get; private set; }
        public List<HairPrestationDto>? HairPrestations { get; init; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;
            if (typeof(T) == typeof(List<HairPrestationDto>))
            {
                return Task.FromResult((T)(object)(HairPrestations ?? new List<HairPrestationDto>()));
            }
            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}