using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(PostIt.Tests.TestAppBuilder))]

namespace PostIt.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<PostIt.App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            });
}