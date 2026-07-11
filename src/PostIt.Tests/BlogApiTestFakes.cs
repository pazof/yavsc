using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels;

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
/// a server-issued BlogPost (Id=42), the second call gets a
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
        // BlogPost? boxes to BlogPost at runtime, so we test the
        // non-nullable type — typeof(BlogPost?) is a C# error
        // (CS8639: "typeof cannot be used on a nullable reference
        // type").
        if (typeof(T) == typeof(BlogPost))
            return Task.FromResult((T)(object)new BlogPost
            {
                Id = 42,
                Title = "Mon premier billet",
                AuthorId = "tester",
                Article = "Contenu du billet de test.",
            });
        if (typeof(T) == typeof(List<BlogPost>))
            return Task.FromResult((T)(object)new List<BlogPost>
            {
                new() { Id = 42, Title = "Mon premier billet" }
            });
        return Task.FromResult(default(T)!);
    }
}
