using System;
using System.Net.Http;
using PostIt.Controls;
using PostIt.ViewModels;
using Yavsc.Api.Client;

namespace PostIt.Tests;

/// <summary>
/// Verifies <see cref="EstimateValidationPageViewModel"/> wires the
/// signature pad + perspective into the right
/// <see cref="FrontOfficeApiClient"/> call. The view (XAML, rendering)
/// is not tested here; the VM is driven through the same
/// <see cref="SignaturePadControl"/> test hooks the signature tests use,
/// and the API client through a recording fake
/// <see cref="IYavscApiClient"/>. The test host's
/// <c>TestApp</c> makes <c>GoBackAsync</c> a no-op, so the success paths
/// run to completion without navigation side effects.
/// </summary>
public class EstimateValidationPageViewModelTests
{
    private static EstimateDto SampleEstimate(long? commandId = 42) => new()
    {
        Id = 7,
        CommandId = commandId,
        CommandType = "Rdv",
        Title = "Devis plomberie",
        Description = "Réparation",
        ClientId = "bob",
        OwnerId = "alice",
    };

    private static void DrawOneStroke(SignaturePadControl pad)
    {
        pad.AppendPointForTest(100, 200);
        pad.AppendPointForTest(300, 400);
        pad.SealStrokeForTest();
    }

    [Fact]
    public async Task Validate_calls_accept_with_strokes_and_command_id()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);

        var pad = new SignaturePadControl();
        vm.Attach(pad);
        DrawOneStroke(pad);

        await vm.ValidateAsync();

        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Contains("front/query/accept", api.LastPath);
        Assert.Contains("billingCode=Rdv", api.LastPath);
        Assert.Contains("queryId=42", api.LastPath);
        var body = Assert.IsType<QueryAcceptanceRequestDto>(api.LastBody);
        Assert.NotNull(body.Strokes);
        Assert.NotEmpty(body.Strokes);
        Assert.Equal(10_000, body.CoordinateMax);
    }

    [Fact]
    public async Task Reject_calls_reject_endpoint_with_no_body()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Client, front);

        vm.Attach(new SignaturePadControl());

        await vm.RejectAsync();

        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Contains("front/query/reject", api.LastPath);
        Assert.Contains("queryId=42", api.LastPath);
        Assert.Null(api.LastBody);
    }

    [Fact]
    public async Task Validate_without_signature_warns_and_does_not_call()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);

        vm.Attach(new SignaturePadControl()); // empty pad

        await vm.ValidateAsync(); // no throw: returns before GoBack

        Assert.Null(api.LastMethod);
        Assert.Contains("signer", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_command_gates_on_signature_and_command()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(commandId: null), EstimateListPerspective.Provider, front);

        var pad = new SignaturePadControl();
        vm.Attach(pad);

        // No CommandId ⇒ cannot validate even after signing.
        DrawOneStroke(pad);
        Assert.False(vm.ValidateCommand.CanExecute(null));

        var vm2 = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);
        var pad2 = new SignaturePadControl();
        vm2.Attach(pad2);
        Assert.False(vm2.ValidateCommand.CanExecute(null)); // no signature yet
        DrawOneStroke(pad2);
        Assert.True(vm2.ValidateCommand.CanExecute(null));
    }

    [Fact]
    public void RoleLabel_reflects_perspective()
    {
        var front = new FrontOfficeApiClient(new RecordingApi(), "https://business.example/api/v1/");
        var pro = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);
        var cli = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Client, front);

        Assert.Contains("prestataire", pro.RoleLabel, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("client", cli.RoleLabel, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? LastMethod { get; private set; }
        public string? LastPath { get; private set; }
        public object? LastBody { get; private set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;
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