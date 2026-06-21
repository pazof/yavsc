using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace PostIt.Services;

    public class LoopbackBrowser : IBrowser
    {
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(options.EndUrl, UriKind.Absolute, out var endUri))
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Invalid end URL" };
            }

            var prefix = endUri.GetLeftPart(UriPartial.Path);
            if (!prefix.EndsWith("/")) prefix += "/";

            using var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            try
            {
                Process.Start(new ProcessStartInfo(options.StartUrl) { UseShellExecute = true });

                // Bound the wait so the port is released even if the user
                // closes the browser without completing the flow. Without
                // this, a crashed/abandoned login keeps the HttpListener
                // bound and the next PostIt launch fails with
                // "Failed to listen on prefix … because it conflicts with
                // an existing registration".
                using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                waitCts.CancelAfter(TimeSpan.FromMinutes(5));

                var context = await listener.GetContextAsync().WaitAsync(waitCts.Token).ConfigureAwait(false);
                var response = context.Response;
                var responseString = "<html><body>Authentication complete. You can close this window.</body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                response.OutputStream.Close();

                var raw = context.Request.Url!.ToString();
                return new BrowserResult
                {
                    ResultType = BrowserResultType.Success,
                    Response = raw
                };
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Bound hit: user did not complete the flow within 5 minutes.
                return new BrowserResult
                {
                    ResultType = BrowserResultType.Timeout,
                    Error = "Timed out waiting for the browser to return the authorization code.",
                };
            }
            catch (Exception ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }
            finally
            {
                // Stop() aborts GetContextAsync (releases the bound port);
                // Close() disposes the underlying socket. Both are idempotent
                // and safe to call after Stop() already succeeded, so calling
                // both covers cases where one path throws before the other
                // gets a chance (e.g. process-level socket cleanup on Linux).
                try { listener.Stop(); } catch { }
                try { listener.Close(); } catch { }
            }
        }
    }
