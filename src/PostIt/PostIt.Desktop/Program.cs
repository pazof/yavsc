using System;
using System.Threading;
using Avalonia;
using PostIt.Services;

namespace PostIt.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        PlatformBootstrap.EnsureInitialized();

        // Short-circuit 2nd-instance launches (OS handing us the
        // postit://callback URL) BEFORE Avalonia spins up a window.
        // If we let Avalonia initialise, the new MainWindow flashes
        // open for a frame before OnFrameworkInitializationCompleted
        // detects the scheme and shuts down — visible to the user as
        // a second window with "Déconnecté" while the original
        // instance is still waiting on the named pipe.
        //
        // We only need the scheme prefix and the OS-supplied URL,
        // both of which are plain System.* / PostIt.Services —
        // nothing Avalonia-specific is touched here, so the rule
        // above is not violated.
        if (TryHandOffCustomSchemeUrl(args)) return;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Detect a 2nd-instance launch (OS dispatching the postit:// URL
    /// after the user completed login in the system browser), forward
    /// the URL to the running instance over the named pipe, and exit
    /// before Avalonia can open a window. Returns true when the
    /// process should terminate without booting Avalonia.
    /// </summary>
    private static bool TryHandOffCustomSchemeUrl(string[] args)
    {
        var url = SchemeUrlDetector.FindCallbackUrl(args);
        if (url is null) return false;

        // Best-effort: try to send the URL to the running
        // instance via the named pipe. If the pipe isn't
        // answering (user double-clicked the link after closing
        // PostIt), there's no 1st instance to forward to — we
        // exit cleanly anyway rather than booting a stray
        // PostIt window that would just confuse the user.
        try
        {
            SingleInstance.TryHandOffAsync(url).GetAwaiter().GetResult();
        }
        catch
        {
            // Pipe errors are non-fatal for the 2nd-instance
            // hand-off — we still want to exit cleanly.
        }

        Environment.Exit(0);
        return true;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}