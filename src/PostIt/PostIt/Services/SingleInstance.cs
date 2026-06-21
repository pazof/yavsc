using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

/// <summary>
/// Single-instance coordinator for PostIt. The first process claims a
/// well-known named pipe and starts a server that listens for
/// callback URLs from any subsequent launch. Subsequent launches
/// (triggered by the OS registering the custom URI scheme and the
/// browser opening the scheme after the OIDC redirect) detect the
/// existing instance, write the URL to the pipe, and exit.
///
/// This is the standard RFC 8252 §7.1 pattern for native apps that
/// use a custom URI scheme: the OS launches a fresh process for each
/// callback, and the running app must hand the URL back to itself.
/// </summary>
public static class SingleInstance
{
    public const string PipeName = "PostIt.OidcCallback";

    /// <summary>
    /// Tries to open the well-known pipe. If a server is already
    /// listening (another PostIt instance), the URL is written and
    /// the call returns <c>true</c> to signal the caller to exit. If
    /// nothing is listening, returns <c>false</c> and the caller
    /// should start its own server via <see cref="StartServerAsync"/>.
    /// </summary>
    public static async Task<bool> TryHandOffAsync(string callbackUrl, TimeSpan? timeout = null)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await client.ConnectAsync((int)(timeout ?? TimeSpan.FromSeconds(2)).TotalMilliseconds).ConfigureAwait(false);
            var bytes = Encoding.UTF8.GetBytes(callbackUrl);
            await client.WriteAsync(bytes).ConfigureAwait(false);
            await client.FlushAsync().ConfigureAwait(false);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    /// Starts the single-instance server loop. Yields each received
    /// callback URL on the returned channel. The server runs until
    /// the cancellation token is tripped (typically when the
    /// owning <see cref="CustomSchemeBrowser"/> returns from
    /// <c>InvokeAsync</c> or the process shuts down).
    /// </summary>
    public static async Task StartServerAsync(
        Action<string> onCallback,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream? server = null;
            try
            {
                server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                using var ms = new MemoryStream();
                await server.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                var url = Encoding.UTF8.GetString(ms.ToArray());
                onCallback(url);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (IOException)
            {
                // Client disconnected mid-write; loop and wait for the next.
            }
            finally
            {
                server?.Dispose();
            }
        }
    }
}
