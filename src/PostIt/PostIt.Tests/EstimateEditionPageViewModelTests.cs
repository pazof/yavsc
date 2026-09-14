using System.Net.Http;
using PostIt.ViewModels;
using Yavsc;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public class EstimateEditionPageViewModelTests
{
    private static BillingQuerySummaryDto SampleQuery() => new()
    {
        Id = 42,
        BillingCode = "Brush",
        ActivityCode = "hair",
        PerformerId = "perf-1",
        ClientId = "cli-1",
        Status = QueryStatus.InProgress,
        Description = "Coupe simple",
        EventDate = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc),
    };

    private static EstimateEditionPageViewModel CreateViewModel(StubEstimateApi api, BillingQuerySummaryDto? query = null)
    {
        var client = new EstimateApiClient(api, "https://business.example/api/v1/");
        return new EstimateEditionPageViewModel(query ?? SampleQuery(), client);
    }

    [Fact]
    public void Constructor_prefills_description_and_adds_a_first_line()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);

        Assert.Equal("Coupe simple", vm.EstimateDescription);
        Assert.Single(vm.Lines);
        Assert.Same(vm.Lines[0], vm.SelectedLine);
        Assert.Contains("#42", vm.ContextLabel);
        Assert.Contains("cli-1", vm.ContextLabel);
    }

    [Fact]
    public void AddLine_appends_and_selects_the_new_line()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);

        vm.AddLineCommand.Execute(null);

        Assert.Equal(2, vm.Lines.Count);
        Assert.Same(vm.Lines[1], vm.SelectedLine);
    }

    [Fact]
    public void RemoveLine_removes_the_selected_line()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        var first = vm.Lines[0];

        vm.RemoveLineCommand.Execute(null);

        Assert.Empty(vm.Lines);
        Assert.Null(vm.SelectedLine);
        Assert.False(vm.RemoveLineCommand.CanExecute(null));
        Assert.DoesNotContain(first, vm.Lines);
    }

    [Fact]
    public void Total_sums_line_totals_and_tracks_edits()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);

        vm.Lines[0].Count = 2;
        vm.Lines[0].UnitaryCost = 15.5m;

        Assert.Equal(31m, vm.Total);
        Assert.Equal($"{31m:0.00} EUR", vm.TotalLabel);

        vm.AddLineCommand.Execute(null);
        vm.Lines[1].Count = 1;
        vm.Lines[1].UnitaryCost = 9m;

        Assert.Equal(40m, vm.Total);
    }

    [Fact]
    public async Task Send_without_title_warns_and_does_not_post()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.Lines[0].Name = "Coupe";
        vm.Lines[0].Description = "Coupe simple";
        vm.Lines[0].UnitaryCost = 25m;

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(api.LastBody);
        Assert.Equal(StatusSeverity.Warning, vm.ActionStatus.Severity);
        Assert.Contains("titre", vm.ActionStatus.Message);
    }

    [Fact]
    public async Task Send_without_any_line_warns_and_does_not_post()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "Devis coupe";
        vm.Lines.Clear();

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(api.LastBody);
        Assert.Equal(StatusSeverity.Warning, vm.ActionStatus.Severity);
        Assert.Contains("ligne", vm.ActionStatus.Message);
    }

    [Fact]
    public async Task Send_with_a_blank_line_name_warns_and_does_not_post()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "Devis coupe";
        vm.Lines[0].Description = "Oubli du nom";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(api.LastBody);
        Assert.Equal(StatusSeverity.Warning, vm.ActionStatus.Severity);
        Assert.Contains("nom", vm.ActionStatus.Message);
    }

    [Fact]
    public async Task Send_posts_the_estimate_payload_to_the_estimate_route()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "  Devis coupe  ";
        vm.Lines[0].Name = "Coupe";
        vm.Lines[0].Description = "Coupe simple";
        vm.Lines[0].Count = 2.4m;
        vm.Lines[0].UnitaryCost = 25m;

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Equal("https://business.example/api/v1/estimate", api.LastPath);
        Assert.Equal(HttpMethod.Post, api.LastMethod);

        var payload = Assert.IsType<EstimateDto>(api.LastBody);
        Assert.Equal(42, payload.CommandId);
        Assert.Equal("cli-1", payload.ClientId);
        Assert.Equal("Brush", payload.CommandType);
        Assert.Equal("Devis coupe", payload.Title);
        Assert.Equal("Coupe simple", payload.Description);
        Assert.Empty(payload.AttachedFiles);
        Assert.Empty(payload.AttachedGraphics);

        var line = Assert.Single(payload.Bill);
        Assert.Equal("Coupe", line.Name);
        Assert.Equal(2, line.Count);
        Assert.Equal(25m, line.UnitaryCost);
        Assert.Equal("EUR", line.Currency);
    }

    [Fact]
    public async Task Send_marks_the_page_as_sent_and_disables_resend()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "Devis coupe";
        vm.Lines[0].Name = "Coupe";
        vm.Lines[0].Description = "Coupe simple";
        vm.Lines[0].UnitaryCost = 25m;

        await vm.SendCommand.ExecuteAsync(null);

        Assert.True(vm.HasSent);
        Assert.False(vm.SendCommand.CanExecute(null));
        Assert.Equal("Devis envoyé", vm.SendLabel);
        Assert.Equal(StatusSeverity.Info, vm.ActionStatus.Severity);
        Assert.Contains("#7", vm.ActionStatus.Message);
    }

    [Fact]
    public async Task Send_surfaces_server_errors_as_error_status()
    {
        var api = new StubEstimateApi { Failure = new HttpRequestException("boom", null, System.Net.HttpStatusCode.InternalServerError) };
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "Devis coupe";
        vm.Lines[0].Name = "Coupe";
        vm.Lines[0].Description = "Coupe simple";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.False(vm.HasSent);
        Assert.Equal(StatusSeverity.Error, vm.ActionStatus.Severity);
        Assert.True(vm.SendCommand.CanExecute(null));
    }

    [Fact]
    public async Task Send_accepts_negative_amounts_for_discount_lines()
    {
        var api = new StubEstimateApi();
        var vm = CreateViewModel(api);
        vm.EstimateTitle = "Devis avec remise";
        vm.Lines[0].Name = "Coupe";
        vm.Lines[0].Description = "Coupe simple";
        vm.Lines[0].UnitaryCost = 25m;

        vm.AddLineCommand.Execute(null);
        vm.Lines[1].Name = "Remise fidélité";
        vm.Lines[1].Description = "Remise client régulier";
        vm.Lines[1].UnitaryCost = -5m;

        Assert.Equal(20m, vm.Total);

        await vm.SendCommand.ExecuteAsync(null);

        var payload = Assert.IsType<EstimateDto>(api.LastBody);
        Assert.Equal(2, payload.Bill.Count);
        Assert.Equal(-5m, payload.Bill[1].UnitaryCost);
        Assert.True(vm.HasSent);
    }

    private sealed class StubEstimateApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public string? LastPath { get; private set; }
        public HttpMethod? LastMethod { get; private set; }
        public object? LastBody { get; private set; }
        public Exception? Failure { get; init; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;

            if (Failure is not null)
            {
                throw Failure;
            }

            if (typeof(T) == typeof(EstimateCreatedDto))
            {
                var payload = (EstimateDto)body!;
                var created = new EstimateCreatedDto { Id = 7, Bill = payload.Bill };
                return Task.FromResult((T)(object)created);
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

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<T>(method, path, (object?)null, ct);

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync(method, path, (object?)null, ct);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
