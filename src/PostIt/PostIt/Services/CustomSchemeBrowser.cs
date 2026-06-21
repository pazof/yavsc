using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace PostIt.Services;

/// <summary>
/// Browser implementation that uses an OS-registered custom URI
/// scheme (e.g. <c>postit://callback</c>) instead of a loopback
/// HTTP listener. The redirect URI never has to be served: when the
/// browser hits the scheme, the OS launches a fresh PostIt process
/// with the URL on the command line. That process hands the URL off
/// to the running instance via <see cref="SingleInstance"/> and
/// exits.
///
/// Reference: RFC 8252 §7.1 (OAuth 2.0 for Native Apps — Custom URI
/// Scheme Redirect).
/// </summary>
public class CustomSchemeBrowser : IBrowser
{
    private readonly string _customScheme;
    private readonly TimeSpan _timeout;

    public CustomSchemeBrowser(string customScheme, TimeSpan? timeout = null)
    {
        _customScheme = customScheme;
        _timeout = timeout ?? TimeSpan.FromMinutes(5);
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        // The browser is told to come back to our scheme with the
        // auth code. We do not need a network listener.
        var endScheme = options.EndUrl;
        if (string.IsNullOrEmpty(endScheme) || !endScheme.StartsWith(_customScheme, StringComparison.OrdinalIgnoreCase))
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = $"EndUrl must use the {_customScheme}:// scheme; got '{endScheme}'.",
            };
        }

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        // Spawn the server loop that waits for a 2nd-instance hand-off.
        // When the callback URL arrives, complete the TCS so InvokeAsync
        // returns it as the BrowserResult.Response.
        using var serverCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
        var serverTask = SingleInstance.StartServerAsync(
            url => tcs.TrySetResult(url),
            serverCts.Token);

        try
        {
            // Open the system browser. UseShellExecute=true is the
            // right shape for both Windows and Linux: the OS resolves
            // the custom scheme by launching our app.
            Process.Start(new ProcessStartInfo(options.StartUrl) { UseShellExecute = true });

            // Race the callback against the timeout/caller
            // cancellation. When the TCS completes the result IS
            // the URL we want; when the timer fires first, we treat
            // the wait as cancelled.
            var delay = Task.Delay(Timeout.Infinite, cts.Token);
            var completed = await Task.WhenAny(tcs.Task, delay).ConfigureAwait(false);
            if (completed != tcs.Task)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.Timeout,
                    Error = "Timed out waiting for the custom-scheme callback.",
                };
            }

            var url = await tcs.Task.ConfigureAwait(false);
            return new BrowserResult
            {
                ResultType = BrowserResultType.Success,
                Response = url,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel,
            };
        }
        catch (Exception ex)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = ex.Message,
            };
        }
        finally
        {
            serverCts.Cancel();
            try { await serverTask.ConfigureAwait(false); } catch { }
        }
    }
}
