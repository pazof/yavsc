using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt.Tests;

public class ActivityNavigationTests
{
    [AvaloniaFact]
    public async Task Activity_recreation_restores_fresh_pages_and_back_navigation()
    {
        var app = Assert.IsType<App>(Application.Current);
        var previousView = app.View;
        var original = new MainView();
        var home = new HomePage();
        var draft = new Settings { SelectedLanguage = "fr" };
        var editor = new ContentPage { DataContext = draft };
        await original.NavRoot.PushAsync(home, null);
        await original.NavRoot.PushAsync(editor, null);
        app.AttachMainWindow(original);
        var window = new Window();
        try
        {
            var restored = app.CreateActivityView(new Settings());
            window.Content = restored;
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, restored.NavRoot.NavigationStack.Count);
            Assert.IsType<HomePage>(restored.NavRoot.NavigationStack[0]);
            Assert.NotSame(home, restored.NavRoot.NavigationStack[0]);
            Assert.NotSame(editor, restored.NavRoot.NavigationStack[1]);
            Assert.Same(draft, restored.NavRoot.NavigationStack[1].DataContext);
            Assert.Equal("fr", draft.SelectedLanguage);

            window.Content = null;
            window.Content = restored;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, restored.NavRoot.NavigationStack.Count);

            // Another activity recreation must retain the already-restored stack.
            window.Content = null;
            var recreatedAgain = app.CreateActivityView(new Settings());
            window.Content = recreatedAgain;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, recreatedAgain.NavRoot.NavigationStack.Count);
            Assert.Same(draft, recreatedAgain.NavRoot.NavigationStack[1].DataContext);
            await recreatedAgain.NavRoot.PopAsync(null);
            Assert.IsType<HomePage>(Assert.Single(recreatedAgain.NavRoot.NavigationStack));
        }
        finally
        {
            window.Close();
            app.AttachMainWindow(previousView ?? new MainView());
        }
    }
}
