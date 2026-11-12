using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(PostIt.Tests.TestAppBuilder))]
// Ordinary tests also use App.Current and Platform while headless tests reset the shared runtime.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace PostIt.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        var nativeMedia = System.Environment.GetEnvironmentVariable("POSTIT_TEST_NATIVE_WEBRTC") == "1";
        var builder = AppBuilder.Configure<PostIt.App>();
        if (nativeMedia) builder.UseSkia();
        return builder.UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = !nativeMedia
        });
    }
}