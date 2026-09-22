using System;
using System.Collections.Generic;
using System.Net.Http;
using PostIt.Controls;
using PostIt.ViewModels;
using Yavsc.Api.Client;
using Yavsc.Models.Billing;

namespace PostIt.Tests;

/// <summary>
/// Verifies <see cref="EstimateValidationPageViewModel"/> wires the two
/// signature pads (provider + client) into the right
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

        var proPad = new SignaturePadControl();
        var clientPad = new SignaturePadControl();
        vm.Attach(proPad, clientPad);
        DrawOneStroke(proPad); // provider signs on their writable pad

        await vm.ValidateAsync();

        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Contains("front/query/accept", api.LastPath);
        Assert.Contains("billingCode=Rdv", api.LastPath);
        Assert.Contains("queryId=42", api.LastPath);
        var body = Assert.IsType<QueryAcceptanceRequestDto>(api.LastBody);
        Assert.NotNull(body.Strokes);
        Assert.NotEmpty(body.Strokes);
        Assert.Equal(10_000, body.CoordinateMax);
        // The provider perspective declares the Pro side, so the
        // server stores the signature under the provider's author
        // regardless of role inference.
        Assert.Equal(SignatureType.Pro, body.SignatureType);
    }

    [Fact]
    public async Task Validate_from_client_perspective_declares_client_side()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Client, front);

        var cliPad = new SignaturePadControl();
        vm.Attach(new SignaturePadControl(), cliPad);
        DrawOneStroke(cliPad); // client signs on their writable pad

        await vm.ValidateAsync();

        var body = Assert.IsType<QueryAcceptanceRequestDto>(api.LastBody);
        Assert.Equal(SignatureType.Client, body.SignatureType); // Client side
    }

    [Fact]
    public async Task Reject_calls_reject_endpoint_with_no_body()
    {
        var api = new RecordingApi();
        var front = new FrontOfficeApiClient(api, "https://business.example/api/v1/");
        var vm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Client, front);

        vm.Attach(new SignaturePadControl(), new SignaturePadControl());

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

        vm.Attach(new SignaturePadControl(), new SignaturePadControl()); // empty pads

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

        var proPad = new SignaturePadControl();
        vm.Attach(proPad, new SignaturePadControl());

        // No CommandId ⇒ cannot validate even after signing.
        DrawOneStroke(proPad);
        Assert.False(vm.ValidateCommand.CanExecute(null));

        var vm2 = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);
        var proPad2 = new SignaturePadControl();
        vm2.Attach(proPad2, new SignaturePadControl());
        Assert.False(vm2.ValidateCommand.CanExecute(null)); // no signature yet
        DrawOneStroke(proPad2);
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

    [Fact]
    public void Only_the_author_pad_is_writable()
    {
        var front = new FrontOfficeApiClient(new RecordingApi(), "https://business.example/api/v1/");

        var proVm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Provider, front);
        var proPad = new SignaturePadControl();
        var cliPad = new SignaturePadControl();
        proVm.Attach(proPad, cliPad);
        Assert.False(proPad.IsReadOnly);  // provider writes the Pro pad
        Assert.True(cliPad.IsReadOnly);

        var cliVm = new EstimateValidationPageViewModel(
            SampleEstimate(), EstimateListPerspective.Client, front);
        var proPad2 = new SignaturePadControl();
        var cliPad2 = new SignaturePadControl();
        cliVm.Attach(proPad2, cliPad2);
        Assert.True(proPad2.IsReadOnly);
        Assert.False(cliPad2.IsReadOnly); // client writes the Client pad
    }

    [Fact]
    public void Load_existing_signatures_renders_both_sides_in_their_pads()
    {
        var proStrokes = new[] { 1, 1000, 1000 };
        var cliStrokes = new[] { 1, 9000, 9000 };
        var front = new FrontOfficeApiClient(new RecordingApi(), "https://business.example/api/v1/");
        var estimate = SampleEstimate();
        estimate.SignaturePro = new EstimateSignatureDto { Strokes = proStrokes };
        estimate.SignatureClient = new EstimateSignatureDto { Strokes = cliStrokes };

        // Client perspective: both pads show the stored signatures, but
        // only the client pad is writable.
        var vm = new EstimateValidationPageViewModel(estimate, EstimateListPerspective.Client, front);
        var proPad = new SignaturePadControl();
        var cliPad = new SignaturePadControl();
        vm.Attach(proPad, cliPad);

        Assert.Equal(proStrokes, proPad.Snapshot().Strokes);
        Assert.Equal(cliStrokes, cliPad.Snapshot().Strokes);
        Assert.True(vm.HasProSignature);
        Assert.True(vm.HasClientSignature);
    }

    [Fact]
    public void Load_existing_pro_signature_renders_in_pro_pad_only()
    {
        var strokes = new[] { 1, 5000, 5000 };
        var front = new FrontOfficeApiClient(new RecordingApi(), "https://business.example/api/v1/");
        var estimate = SampleEstimate();
        estimate.SignaturePro = new EstimateSignatureDto { Strokes = strokes, CoordinateMax = 10_000 };
        var vm = new EstimateValidationPageViewModel(estimate, EstimateListPerspective.Provider, front);

        var proPad = new SignaturePadControl();
        var cliPad = new SignaturePadControl();
        vm.Attach(proPad, cliPad);

        Assert.Equal(strokes, proPad.Snapshot().Strokes);
        Assert.True(cliPad.Snapshot().IsEmpty);
        Assert.True(vm.HasProSignature);
        Assert.False(vm.HasClientSignature);
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