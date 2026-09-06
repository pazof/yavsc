using System.Text.Json;
using PostIt.Helpers;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.ViewModels.Commands;
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
        var vm =
          new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
          .CreateCommandPageViewModel(
            new ActivityInfo { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            client) as RdvViewModel;

            vm!.EventDate = DateTime.Parse("2026-09-02 14:30");
        vm!.Reason = "Point de cadrage";
        vm!.Address = "1 rue du Test";
        vm!.Latitude = 48.8566;
        vm!.Longitude = 2.3522;
        vm!.Consent = true;

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

        var vm =
         new CommandFormSummary { Id = 13, ActionName = "Book", Title = "Réservation" }
         .CreateCommandPageViewModel(
            new ActivityInfo { Code = "book", Name = "Book" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            client);

        Assert.Null(vm);
    }

    [Fact]
    public async Task SubmitAsync_allows_missing_coordinates_and_omits_them_from_payload()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm =

        new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
        .CreateCommandPageViewModel(
            new ActivityInfo { Code = "dev", Name = "Développement" },
            new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
            client) as RdvViewModel;
        vm!.EventDate = DateTime.Parse("2026-09-02 14:30");
        vm!.Reason = "Point de cadrage";
        vm!.Address = "1 rue du Test";
        vm!.Latitude = null;
        vm!.Longitude = null;
        vm!.Consent = true;

        await vm.SubmitCommand.ExecuteAsync(null);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(api.LastBody));
        var location = json.RootElement.GetProperty("Location");
        Assert.Equal("1 rue du Test", location.GetProperty("Address").GetString());
        Assert.False(location.TryGetProperty("Latitude", out _));
        Assert.False(location.TryGetProperty("Longitude", out _));
    }

    [Fact]
    public async Task UseCurrentLocationAsync_prefills_coordinates_from_platform_provider()
    {
        var original = Platform.TryGetCurrentLocationAsync;
        try
        {
            Platform.TryGetCurrentLocationAsync = _ => Task.FromResult(CurrentLocationResult.Success(48.8566, 2.3522));

            var api = new RecordingApi();
            var client = new BillingApiClient(api, "https://business.example/api/v1/");
            var vm =
             new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
             .CreateCommandPageViewModel(
                new ActivityInfo { Code = "dev", Name = "Développement" },
                new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
                client) as RdvViewModel;

            await vm!.UseCurrentLocationCommand.ExecuteAsync(null);

            Assert.Equal(48.8566, vm!.Latitude);
            Assert.Equal(2.3522, vm!.Longitude);
        }
        finally
        {
            Platform.TryGetCurrentLocationAsync = original;
        }
    }

    [Fact]
    public void ApplyLocationFromMap_sets_coordinates_and_updates_status()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm =
            new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
            .CreateCommandPageViewModel(
                new ActivityInfo { Code = "dev", Name = "Développement" },
                new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
                client) as RdvViewModel;

        vm!.Address = string.Empty;
        vm.ApplyLocationFromMap(48.85661234, 2.35224567);

        Assert.Equal(48.856612, vm.Latitude);
        Assert.Equal(2.352246, vm.Longitude);
        Assert.Contains("Position sélectionnée", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EventDateSelection_round_trips_with_EventDate_for_DatePicker_binding()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm =
            new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
            .CreateCommandPageViewModel(
                new ActivityInfo { Code = "dev", Name = "Développement" },
                new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
                client) as RdvViewModel;

        var selected = new DateTimeOffset(2026, 9, 7, 14, 30, 0, TimeSpan.FromHours(2));
        vm!.EventDateSelection = selected;

        Assert.Equal(selected.LocalDateTime, vm.EventDate);
        Assert.Equal(vm.EventDate, vm.EventDateSelection!.Value.LocalDateTime);
    }

    [Fact]
    public void ApplyResolvedAddress_populates_empty_address_directly()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm =
            new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
            .CreateCommandPageViewModel(
                new ActivityInfo { Code = "dev", Name = "Développement" },
                new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
                client) as RdvViewModel;

        vm!.Address = string.Empty;
        vm.ApplyResolvedAddress("10 rue de Rivoli, 75001 Paris");

        Assert.Equal("10 rue de Rivoli, 75001 Paris", vm.Address);
        Assert.False(vm.HasSuggestedAddress);
    }

    [Fact]
    public void ApplyResolvedAddress_preserves_manual_address_and_exposes_suggestion()
    {
        var api = new RecordingApi();
        var client = new BillingApiClient(api, "https://business.example/api/v1/");
        var vm =
            new CommandFormSummary { Id = 12, ActionName = "Rdv", Title = "Rendez-vous" }
            .CreateCommandPageViewModel(
                new ActivityInfo { Code = "dev", Name = "Développement" },
                new ActivityUserDisplayItem { PerformerId = "perf-1", UserName = "Alice" },
                client) as RdvViewModel;

        vm!.Address = "Saisie manuelle";
        vm.ApplyResolvedAddress("10 rue de Rivoli, 75001 Paris");

        Assert.Equal("Saisie manuelle", vm.Address);
        Assert.True(vm.HasSuggestedAddress);
        Assert.Equal("10 rue de Rivoli, 75001 Paris", vm.SuggestedAddress);

        vm.ApplySuggestedAddressCommand.Execute(null);

        Assert.Equal("10 rue de Rivoli, 75001 Paris", vm.Address);
        Assert.False(vm.HasSuggestedAddress);
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
        var vm =
        new CommandFormSummary { Id = 13, ActionName = "Brush", Title = "Coupe" }
        .CreateCommandPageViewModel(
            new ActivityInfo { Code = "brush", Name = "Brush" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            client) as BrushViewModel;
        vm!.EventDate = DateTime.Parse("2026-09-02 14:30");
        vm!.Address = "1 rue du Test";
        vm!.Latitude = 48.8566;
        vm!.Longitude = 2.3522;
        vm!.Consent = true;
        vm!.AdditionalInfo = "Prévoir shampoing";

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
        var vm =
        new CommandFormSummary { Id = 14, ActionName = "MBrush", Title = "Coupe groupée" }
        .CreateCommandPageViewModel(
            new ActivityInfo { Code = "mbrush", Name = "MBrush" },
            new ActivityUserDisplayItem { PerformerId = "perf-3", UserName = "Cara" },
            client) as MBrushViewModel;
        vm!.EventDate = DateTime.Parse("2026-09-03 10:00");
        vm!.Address = "2 rue du Test";
        vm!.Latitude = 48.8567;
        vm!.Longitude = 2.3523;
        vm!.Consent = true;

        await vm.InitializeAsync();
        vm!.MultiPrestations[0].IsSelected = true;
        vm!.MultiPrestations[1].IsSelected = true;
        await vm!.SubmitCommand.ExecuteAsync(null);

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
        var vm =
           new CommandFormSummary { Id = 13, ActionName = "Brush", Title = "Coupe" }
         .CreateCommandPageViewModel(
            new ActivityInfo { Code = "brush", Name = "Brush" },
            new ActivityUserDisplayItem { PerformerId = "perf-2", UserName = "Bob" },
            client) as BrushViewModel;
        await vm!.InitializeAsync(new BillingQueryDetailsDto
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

        vm!.SelectedPrestation = vm!.AvailablePrestations[1];
        vm!.AdditionalInfo = "Note mise à jour";
        await vm!.SubmitCommand.ExecuteAsync(null);

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
