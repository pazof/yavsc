using System.Net;
using System.Net.Sockets;
using PostIt.Services;

namespace PostIt.Tests;

/// <summary>
/// Regression coverage for the loopback browser that PostIt uses to
/// receive the OIDC authorization-code callback on a local port.
/// Specifically: the listener must always be released, even when the
/// flow is abandoned (timeout or caller cancellation). Without this,
/// the next PostIt launch fails with "Failed to listen on prefix
/// http://127.0.0.1:7890/ because it conflicts with an existing
/// registration on the machine."
/// </summary>
public class LoopbackBrowserTests
{
    [Fact]
    public async Task InvokeAsync_releases_listener_when_no_browser_responds_within_timeout()
    {
        // Pick a free port for this test (don't reuse 7890 — it could be
        // bound by a real PostIt running on the developer's machine).
        var port = GetFreePort();
        var prefix = $"http://127.0.0.1:{port}/";

        var browser = new LoopbackBrowser();
        var options = new IdentityModel.OidcClient.Browser.BrowserOptions(
    "http://127.0.0.1:1/", // never reached
            prefix);

        // The internal wait timeout is 5 minutes; we don't want the test
        // to actually wait that long. Instead we cancel via the outer
        // token and verify the listener is released.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var result = await browser.InvokeAsync(options, cts.Token);

        // The cancellation propagates as Timeout because the outer token
        // fires first (the test is faster than the 5-minute internal wait).
        // We don't care which BrowserResultType is returned here — only
        // that the port is free afterwards.
        Assert.NotNull(result);

        // Critical assertion: the port is free. If the listener leaked,
        // a TcpListener binding to the same port would throw.
        using var probe = new TcpListener(IPAddress.Loopback, port);
        probe.Start();
        probe.Stop();
    }

    [Fact]
    public async Task InvokeAsync_releases_listener_when_browser_actually_responds()
    {
        var port = GetFreePort();
        var prefix = $"http://127.0.0.1:{port}/";

        var browser = new LoopbackBrowser();
        var options = new IdentityModel.OidcClient.Browser.BrowserOptions(
            "http://127.0.0.1:1/", // never reached (we respond directly below)
            prefix);

        // Race the listener against a fake browser callback.
        var browserTask = browser.InvokeAsync(options);

        // Give the listener a moment to bind.
        await Task.Delay(50);

        // Simulate the browser returning the redirect with code + state.
        using var http = new HttpClient();
        var response = await http.GetAsync($"{prefix.TrimEnd('/')}/?code=***&state=***");
        // We don't care about the response body; just that the request
        // was accepted (otherwise the listener hadn't bound yet).
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await browserTask;
        Assert.Equal(IdentityModel.OidcClient.Browser.BrowserResultType.Success, result.ResultType);

        // Listener should be released now.
        using var probe = new TcpListener(IPAddress.Loopback, port);
        probe.Start();
        probe.Stop();
    }

    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
}
