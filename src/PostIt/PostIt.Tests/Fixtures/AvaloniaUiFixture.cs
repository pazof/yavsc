using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;

namespace PostIt.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AvaloniaUiCollection : ICollectionFixture<AvaloniaUiFixture>
{
    public const string Name = "Avalonia UI";
}

public sealed class AvaloniaUiFixture : IAsyncLifetime
{
    private HeadlessUnitTestSession? _session;

    public async ValueTask InitializeAsync()
    {
        _session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder), AvaloniaTestIsolationLevel.PerTest);
        try
        {
            await RunAsync(() => Task.CompletedTask, CancellationToken.None);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task RunAsync(Func<Task> action, CancellationToken ct)
    {
        var session = _session ?? throw new InvalidOperationException("La fixture Avalonia n'est pas initialisee.");
        await session.Dispatch(async () =>
        {
            await action();
            return true;
        }, ct);
    }

    public Task WithWindowAsync(Func<Control> createContent, Func<Window, Task> test, CancellationToken ct,
        double width = 800, double height = 600) => RunAsync(async () =>
    {
        var window = new Window { Width = width, Height = height };
        try
        {
            window.Content = createContent();
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            await test(window);
        }
        finally
        {
            window.Close();
            if (window.Content is Control content) content.DataContext = null;
            window.Content = null;
            window.DataContext = null;
            Dispatcher.UIThread.RunJobs();
        }
    }, ct);

    public async ValueTask DisposeAsync()
    {
        if (_session is not { } session) return;
        _session = null;
        await session.DisposeAsync();
    }
}
