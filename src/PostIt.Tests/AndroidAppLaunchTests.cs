using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xunit;

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
public class AndroidAppLaunchTests
{
    private const string PackageName = "com.CompanyName.PostIt";

    private readonly ITestOutputHelper _output;

    public AndroidAppLaunchTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void PostIt_starts_and_draws_a_first_frame_on_the_emulator()
    {
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
            proc.WaitForExit(5000);
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
