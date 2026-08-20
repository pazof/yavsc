using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;
using Yavsc.Blogspot;

namespace PostIt.Tests;

/// <summary>
/// Headless UI tests for the "Save" flow in <see cref="MainPage"/>.
/// Uses the shared <see cref="PostItHeadlessFixture"/> (a real
/// <see cref="MainWindow"/> with the production DI graph attached
/// to <see cref="App"/>) plus a local
/// <see cref="ServiceCollection"/> that swaps
/// <see cref="YavscApiClient"/> for the recording fake.
///
/// <para>The bug we are pinning: the title <c>TextBox</c> is
/// currently <c>{Binding SelectedPost.Title, Mode=TwoWay}</c>.
/// When <c>SelectedPost is null</c> (i.e. the user has not yet
/// clicked an item in the posts list — which is the only state
/// in which a brand-new post can be created), the binding has
/// no target and the user's keystrokes are silently dropped.
/// Clicking "Save" then routes to the VM branch
/// <c>if (SelectedPost is null) { new BlogPostDto { Title = string.Empty, ... } }</c>
/// which the controller rejects with 400 "The Title field is
/// required." This test fails on that branch today and will
/// pass once the VM owns a dedicated <c>Title</c>/<c>Article</c>
/// buffer that the XAML binds to and the Save command consumes.</para>
/// </summary>
[Collection("PostIt Headless")]
public sealed class MainPageSaveTests
{
    private readonly PostItHeadlessCollection _host;

    public MainPageSaveTests(PostItHeadlessCollection host)
    {
        _host = host;
    }

    [AvaloniaFact]
    public void Typing_a_title_then_clicking_Save_sends_that_title_in_the_post_body()
    {
        // Arrange: VM with a recording API client, mounted on
        // the shared MainWindow's nav stack.
        var recorder = new CallRecorder();

        var blog = _host.Services.GetRequiredService<BlogApiClient>();
        var viewModel = new MainPageViewModel(blog);
        var page = new MainPage { DataContext = viewModel };
        _host.PushAsync(page);

        // Act: type a title into the editor's TextBox without
        // first selecting a post in the list — the only state
        // in which a new post can be created. Then click Save.
        var titleBox = _host.Window.GetVisualDescendants()
            .OfType<TextBox>()
            .First(t => t.PlaceholderText == "Title");
        const string typed = "Mon premier billet";
        titleBox.Text = typed;

        var saveButton = _host.Window.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Save");
        saveButton.Command!.Execute(null);

        // The Save command is async (RelayCommand over Task) but
        // ExecuteAsync would await; the sync Execute enqueues the
        // task on the dispatcher. Give the dispatcher a chance to
        // run so the awaited CallAsync has actually fired before
        // we inspect the recorder.
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (recorder.Calls.Count == 0 && DateTime.UtcNow < deadline)
        {
            Task.Delay(20).GetAwaiter().GetResult();
        }

        // Assert: the first POST to "blog" carried a BlogPostDto
        // whose Title is exactly what the user typed. The bug
        // fails this assertion with Title == string.Empty.
        Assert.NotEmpty(recorder.Calls);
        var (method, path, body) = recorder.FirstCall;
        Assert.Equal(HttpMethod.Post, method);
        Assert.Equal("blog", path);
        var sent = Assert.IsType<BlogPostDto>(body);
        Assert.Equal(typed, sent.Title);
    }
}
