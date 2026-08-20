using Yavsc.Blogspot;
using PostIt.Services;
using PostIt.ViewModels;
using Yavsc.Models;

namespace PostIt.Tests;

/// <summary>Per-call ledger shared between the test and the
/// recording fake, so the assertion can inspect what the VM
/// actually sent on the wire without coupling to the fake's
/// internals.</summary>
internal sealed class CallRecorder
{
    public (HttpMethod method, string path, object? body) FirstCall =>
        Calls[0];
    public List<(HttpMethod method, string path, object? body)> Calls { get; } = new();
}

/// <summary>Test fake that records every CallAsync invocation
/// and answers them with a canned sequence: the first call gets
/// a server-issued BlogPostDto (Id=42), the second call gets a
/// single-element list containing that post. Used by the ViewModel
/// tests and the headless UI test to capture exactly what the
/// Save button posts to the server.</summary>
internal sealed class RecordingYavscApiClient : YavscApiClient
{
    private readonly CallRecorder _recorder;
    public RecordingYavscApiClient(CallRecorder recorder)
        : base(
            new Settings
            {
                Authentication = new AuthenticationSettings
                {
                    Authority = "https://stub.invalid",
                    ClientId = "stub",
                    Scopes = new[] { "openid" },
                },
            },
            new TokenStore(System.IO.Path.GetTempFileName()))
    {
        _recorder = recorder;
    }

    public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
    {
        _recorder.Calls.Add((method, path, body));
        // BlogPostDto? boxes to BlogPostDto at runtime, so we test the
        // non-nullable type — typeof(BlogPostDto?) is a C# error
        // (CS8639: "typeof cannot be used on a nullable reference
        // type").
        if (typeof(T) == typeof(BlogPostDto))
            return Task.FromResult((T)(object)new BlogPostDto
            {
                Id = 42,
                Title = "Mon premier billet",
                AuthorId = "tester",
                Article = "Contenu du billet de test.",
            });
        if (typeof(T) == typeof(List<BlogPostDto>))
            return Task.FromResult((T)(object)new List<BlogPostDto>
            {
                new() { Id = 42, Title = "Mon premier billet" }
            });
        return Task.FromResult(default(T)!);
    }
}

/// <summary>
/// <see cref="YavscApiClient"/> stand-in whose constructor
/// points at <c>https://stub.invalid</c> so any HTTP traffic
/// that escapes a test (misconfigured command, missing fake
/// handler) raises a clear <see cref="System.Net.Http.HttpRequestException"/>
/// instead of silently hitting a real endpoint. Used by tests
/// that don't actually exercise the API client (they click a
/// button, assert on the nav stack, end of story) but whose
/// VMs require one in their constructor.
/// </summary>
internal sealed class ThrowingApi : YavscApiClient
{
    public ThrowingApi() : base(
        new Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://stub.invalid",
                ClientId = "stub",
                Scopes = new[] { "openid" },
            },
        },
        new TokenStore(System.IO.Path.GetTempFileName()))
    { }
}
