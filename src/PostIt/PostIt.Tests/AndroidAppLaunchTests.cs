using System.Diagnostics;
using Xamarin.UITest;

namespace PostIt.Tests;

/// <summary>
/// Smoke test: launches the installed PostIt.Android app on the running
/// emulator and waits for the first Avalonia frame to render. Reveals the
/// "démarrage KO" bug — the test fails if Avalonia never draws a frame
/// within the timeout.
///
/// Skip conditions: the package is not installed on the connected device,
/// or no device is connected via adb.
/// </summary>
[Trait("Platform", "Android")]
[Trait("Category", "non-reg")]
public class AndroidAppLaunchTests
{
    private const string PackageName = "fr.pschneider.postit";

    private readonly ITestOutputHelper _output;

    public AndroidAppLaunchTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // TODO https://twosixtech.com/blog/integrating-docker-and-adb/
    [Fact]
    public void PostIt_starts_and_draws_a_first_frame_on_the_emulator()
    {
        if (IsRunningInCi())
        {
            _output.WriteLine("[skip] Android instrumentation smoke test disabled in CI because the emulator/test APK signature must match the target package exactly and is not reliable on hosted runners");
            return;
        }

        if (!HasAdb())
        {
            _output.WriteLine("[skip] adb not available on this machine");
            return;
        }

        if (!HasConnectedDevice())
        {
            _output.WriteLine("[skip] no Android device/emulator connected via adb");
            return;
        }

        if (!IsPackageInstalledOnAnyDevice())
        {
            _output.WriteLine($"[skip] {PackageName} not installed on any device");
            return;
        }

        _output.WriteLine($"[step] configuring app via InstalledApp({PackageName})");
        var app = ConfigureApp.Android
            .InstalledApp(PackageName)
            .StartApp(Xamarin.UITest.Configuration.AppDataMode.DoNotClear);
        _output.WriteLine("[step] app.StartApp returned, waiting for first frame");

        app.WaitForElement(
            e => e.Class("android.view.View"),
            timeout: TimeSpan.FromSeconds(30));
        _output.WriteLine("[step] first frame observed");
    }

    private static bool IsRunningInCi()
    {
        return Environment.GetEnvironmentVariable("CI") is { Length: > 0 }
            || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is { Length: > 0 }
            || Environment.GetEnvironmentVariable("FORGEJO_CI") is { Length: > 0 }
            || Environment.GetEnvironmentVariable("CI_NAME") is { Length: > 0 };
    }

    private static bool HasAdb()
    {
        try
        {
            var startInfo = new ProcessStartInfo("adb", "version")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(startInfo);
            if (proc is null) return false;
            var _ = proc.StandardOutput.ReadToEnd();
            var __ = proc.StandardError.ReadToEnd();
            if (!proc.WaitForExit(2000))
            {
                proc.Kill(entireProcessTree: true);
                return false;
            }

            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasConnectedDevice()
    {
        try
        {
            var startInfo = new ProcessStartInfo("adb", "devices")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(startInfo);
            if (proc is null) return false;
            var stdout = proc.StandardOutput.ReadToEnd();
            _ = proc.StandardError.ReadToEnd();
            if (!proc.WaitForExit(2000))
            {
                proc.Kill(entireProcessTree: true);
                return false;
            }

            return stdout
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Any(line => line.Contains("device", StringComparison.OrdinalIgnoreCase)
                    && !line.Contains("offline", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPackageInstalledOnAnyDevice()
    {
        try
        {
            var startInfo = new ProcessStartInfo("adb", "shell pm list packages")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(startInfo);
            if (proc is null) return false;
            var stdout = proc.StandardOutput.ReadToEnd();
            _ = proc.StandardError.ReadToEnd();
            if (!proc.WaitForExit(5000))
            {
                proc.Kill(entireProcessTree: true);
                return false;
            }

            return stdout
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Any(line => line.Trim().Equals($"package:{PackageName}", StringComparison.Ordinal));
        }
        catch
        {
            return false;
        }
    }
}
