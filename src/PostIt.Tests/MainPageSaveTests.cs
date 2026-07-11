using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
namespace PostIt.Tests;

/// <summary>
/// Headless UI tests for the "Save" flow in <see cref="MainPage"/>.
/// The pattern is the one <c>SessionStatusBannerTests</c>
/// established: <c>[AvaloniaFact]</c>, a <see cref="Window"/>
/// hosting the page (via a <see cref="Frame"/> because
/// <c>MainPage</c> is a <c>ContentPage</c>), then drive the
/// controls through their public surface and assert on what
/// <see cref="RecordingYavscApiClient"/> saw go on the wire.
///
/// <para>The bug we are pinning: the title <c>TextBox</c> is
/// currently <c>{Binding SelectedPost.Title, Mode=TwoWay}</c>.
/// When <c>SelectedPost is null</c> (i.e. the user has not yet
/// clicked an item in the posts list — which is the only state
/// in which a brand-new post can be created), the binding has
/// no target and the user's keystrokes are silently dropped.
/// Clicking "Save" then routes to the VM branch
/// <c>if (SelectedPost is null) { new BlogPost { Title = string.Empty, ... } }</c>
/// which the controller rejects with 400 "The Title field is
/// required." This test fails on that branch today and will
/// pass once the VM owns a dedicated <c>Title</c>/<c>Article</c>
/// buffer that the XAML binds to and the Save command consumes.</para>
/// </summary>
public class MainPageSaveTests
{
    [AvaloniaFact]
    public async Task Typing_a_title_then_clicking_Save_sends_that_title_in_the_post_body()
    {
        // Arrange: VM with a recording API client, mounted in a
        // headless window via a Frame (MainPage is a ContentPage,
        // not a Control, so it needs a navigation host).
        var recorder = new CallRecorder();
        var api = new RecordingYavscApiClient(recorder);
        var blog = new BlogApiClient(api);
        var viewModel = new MainPageViewModel(blog);

        var page = new MainPage { DataContext = viewModel };
        // MainPage is a ContentPage (a Page, not a Control), so it
        // must be hosted in a navigation surface. The production
        // MainWindow.axaml uses NavigationPage, and the API is the
        // same one App.axaml.cs drives at boot (PushAsync, fire-
        // and-forget in prod because the page is the top of the
        // stack immediately).
        var nav = new NavigationPage();
        _ = nav.PushAsync(page);
        var window = new Window { Content = nav };
        window.Show();

        // Act: type a title into the editor's TextBox without
        // first selecting a post in the list — the only state in
        // which a new post can be created. Then click Save.
        var titleBox = window.GetVisualDescendants()
            .OfType<TextBox>()
            .First(t => t.PlaceholderText == "Title");
        const string typed = "Mon premier billet";
        titleBox.Text = typed;

        var saveButton = window.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Save");
        saveButton.Command!.Execute(null);

        // The Save command is async (RelayCommand over Task) but
        // ExecuteAsync would await; the sync Execute enqueues the
        // task on the dispatcher. Give the dispatcher a chance to
        // run so the awaited CallAsync has actually fired before
        // we inspect the recorder.
        await Task.Delay(200);

        // Assert: the first POST to "blog" carried a BlogPost
        // whose Title is exactly what the user typed. The bug
        // fails this assertion with Title == string.Empty.
        Assert.NotEmpty(recorder.Calls);
        var (method, path, body) = recorder.FirstCall;
        Assert.Equal(HttpMethod.Post, method);
        Assert.Equal("blog", path);
        var sent = Assert.IsType<BlogPost>(body);
        Assert.Equal(typed, sent.Title);
    }
}
