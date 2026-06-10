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

                var context = await listener.GetContextAsync().ConfigureAwait(false);
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
            catch (Exception ex)
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }
            finally
            {
                try { listener.Stop(); } catch { }
            }
        }
    }
