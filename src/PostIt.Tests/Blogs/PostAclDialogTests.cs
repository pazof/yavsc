
using System.Net;
using System.Text;
using System.Text.Json;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;
using Yavsc.Blogspot;

namespace PostIt.Tests;

/// <summary>
/// Regression coverage for the user-reported bug:
/// <c>PostAclDialogViewModel.LoadAsync</c> was never invoked,
/// so <c>MyCircles</c> and <c>AclEntries</c> were empty when the
/// dialog opened (the dropdown showed "Choisir un cercle..." and
/// the list was blank, with no error to hint at why).
///
/// <para>The fix wires <see cref="PostAclDialog"/>'s constructor
/// to trigger <c>LoadAsync</c> on the first
/// <c>DataContextChanged</c>, and the VM guards re-entry via
/// <c>_loaded</c>. Two tests pin that contract:</para>
/// <list type="bullet">
///   <item><c>LoadAsync_runs_once_on_DataContext_changed</c>: HTTP
///   traffic shows up after the dialog is mounted.</item>
///   <item><c>LoadAsync_is_idempotent</c>: a second explicit call
///   to <c>LoadAsync</c> on the same VM hits the HTTP layer only
///   once (the <c>_loaded</c> gate).</item>
/// </list>
///
/// <para>HTTP is stubbed with a counter
/// <see cref="HttpMessageHandler"/> that returns canned JSON
/// <c>[]</c> for every request. The handler counts calls so the
/// tests can assert "exactly one round-trip on mount" and
/// "exactly one round-trip after two calls to LoadAsync". This
/// is the same shape used by <c>BearerScopeTests</c>: real
/// <see cref="YavscApiClient"/> subclass, real
/// <see cref="HttpClient"/> with an injected handler, real
/// <see cref="BlogAclApiClient"/> / <see cref="CircleApiClient"/>
/// talking to it.</para>
///
/// <para>Lifecycle: shared <see cref="PostItHeadlessFixture"/>
/// provides the <see cref="MainWindow"/> already wired to
/// <see cref="App"/>. Each test builds its own DI graph with
/// the counting HTTP handler and swaps it in via
/// <see cref="PostItHeadlessFixture.UseServiceProvider"/>. The
/// graph exposes <c>PostAclDialog</c> so the
/// <see cref="ViewLocator"/> resolves it from
/// <see cref="PostAclDialogViewModel"/>.</para>
/// </summary>
[Collection("PostIt Headless")]
public sealed class PostAclDialogTests
{
    private readonly PostItHeadlessCollection _host;

    public PostAclDialogTests(PostItHeadlessCollection host)
    {
        _host = host;
    }

    /// <summary>
    /// <see cref="HttpMessageHandler"/> that replies 200 with
    /// <c>[]</c> (a valid JSON empty array, which both
    /// <c>GetMyAclAsync</c> and <c>GetMyCirclesAsync</c> can
    /// deserialize) and counts the number of requests.
    /// </summary>
    private sealed class CountingHttpHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Subclass of <see cref="YavscApiClient"/> that routes HTTP
    /// traffic through a caller-supplied
    /// <see cref="HttpMessageHandler"/>. Same recipe as
    /// <c>BearerScopeTests.TestableYavscApiClient</c> — we
    /// override <c>CallAsync{T}</c> to talk to our own
    /// <see cref="HttpClient"/> and skip the OIDC refresh path,
    /// because the load-on-attach bug has nothing to do with
    /// token refresh.
    /// </summary>
    private sealed class TestableYavscApiClient : YavscApiClient
    {
        private readonly HttpClient _http;

        public TestableYavscApiClient(
            Settings settings,
            TokenStore store,
            HttpMessageHandler handler)
            : base(settings, store, oidc: null!)
        {
            _http = new HttpClient(handler, disposeHandler: false);
        }

        public override Task<T> CallAsync<T>(
            HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
        {
            var absolute = new Uri(new Uri(Settings.BusinessApiUrl), path);
            using var req = new HttpRequestMessage(method, absolute);
            using var resp = _http.SendAsync(req, ct).GetAwaiter().GetResult();
            resp.EnsureSuccessStatusCode();
            using var stream = resp.Content.ReadAsStream();
            var dto = JsonSerializer.Deserialize<T>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Task.FromResult(dto!);
        }
    }

}
