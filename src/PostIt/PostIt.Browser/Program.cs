using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PostIt;
using PostIt.Services;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        Platform.IsPodcastSupported = false;

        return BuildAvaloniaApp()
            .WithInterFont()
#if DEBUG
            .WithDeveloperTools()
#endif
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
