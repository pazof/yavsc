using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PostIt.Tests;

[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class AvaloniaUiFixtureTests(AvaloniaUiFixture ui)
{
    [Fact]
    public async Task Dispatches_share_the_UI_thread_but_isolate_the_application()
    {
        Application? firstApplication = null;
        var firstThread = 0;
        await ui.RunAsync(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            firstApplication = Application.Current;
            firstThread = Environment.CurrentManagedThreadId;
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);
        await ui.RunAsync(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            Assert.Equal(firstThread, Environment.CurrentManagedThreadId);
            Assert.NotNull(firstApplication);
            Assert.NotNull(Application.Current);
            Assert.NotSame(firstApplication, Application.Current);
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Windows_are_closed_and_unbound_even_when_the_test_fails(bool fail)
    {
        var expectedFailure = new InvalidOperationException("Test failure");
        Window? opened = null;
        Control? content = null;
        var run = ui.WithWindowAsync(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            content = new TextBlock { DataContext = new object(), Text = "Fixture" };
            return content;
        }, window =>
        {
            opened = window;
            Assert.True(window.IsVisible);
            Assert.Equal(640, window.Width);
            Assert.Equal(480, window.Height);
            return fail ? Task.FromException(expectedFailure) : Task.CompletedTask;
        }, TestContext.Current.CancellationToken, width: 640, height: 480);

        if (fail)
            Assert.Same(expectedFailure, await Assert.ThrowsAsync<InvalidOperationException>(() => run));
        else
            await run;

        // The controls still belong to the fixture's UI thread after their application is disposed.
        await ui.RunAsync(() =>
        {
            Assert.NotNull(opened);
            Assert.False(opened.IsVisible);
            Assert.Null(opened.Content);
            Assert.Null(content!.DataContext);
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);
    }
}
