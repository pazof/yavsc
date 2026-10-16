using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Markdown.Avalonia;
using ColorTextBlock.Avalonia;
using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using PostIt.Views.Blogs;
using Yavsc.Api.Client;
using Yavsc.Blogspot;

namespace PostIt.Tests;

public class BlogReaderTests
{
    private static BlogPostDto Post(string article = "Texte du billet.") => new()
    {
        Id = 42,
        Title = "Un billet à lire",
        Article = article,
        AuthorId = "user-id-not-login",
        Author = new BlogPostAuthorDto { UserName = "paul" },
        DateModified = DateTime.UtcNow
    };

    private static BlogsViewModel CreateViewModel(BlogResourceService? resources = null)
        => new(new BlogApiClient(new RecordingYavscApiClient(new CallRecorder()), "https://blogs.example.test/api/v1/"),
            new Settings { Authentication = new AuthenticationSettings { Authority = "https://org.example.test" } },
            resources: resources);

    [Fact]
    public void Selecting_a_post_defaults_to_reading_and_uses_the_author_login()
    {
        var vm = CreateViewModel();
        vm.SelectedPost = Post();
        Assert.False(vm.IsEditing);
        Assert.Equal("paul", vm.ReaderAuthor);
        Assert.Equal("https://org.example.test/files/paul/", vm.ReaderBaseUrl);
        Assert.Equal("Texte du billet.", vm.ReaderArticle);
    }

    [Fact]
    public void Reading_does_not_publish_or_discard_unsaved_editor_changes()
    {
        var vm = CreateViewModel();
        vm.SelectedPost = Post();
        vm.EditPostCommand.Execute(null);
        vm.DraftArticle = "Modifications non enregistrées";
        vm.ReadPostCommand.Execute(null);
        Assert.False(vm.IsEditing);
        Assert.Equal("Texte du billet.", vm.ReaderArticle);
        Assert.Equal("Modifications non enregistrées", vm.DraftArticle);
        Assert.True(vm.IsDraftModified);
        vm.EditPostCommand.Execute(null);
        Assert.True(vm.IsEditing);
        Assert.Equal("Modifications non enregistrées", vm.DraftArticle);
    }

    [Fact]
    public void New_post_is_an_explicit_action_and_protects_unsaved_changes()
    {
        var vm = CreateViewModel();
        vm.SelectedPost = Post();
        vm.NewPostCommand.Execute(null);
        Assert.True(vm.IsEditing);
        Assert.Null(vm.SelectedPost);
        Assert.Empty(vm.DraftTitle);
        Assert.Empty(vm.DraftArticle!);
        vm.DraftTitle = "Nouveau brouillon";
        vm.NewPostCommand.Execute(null);
        Assert.Equal("Nouveau brouillon", vm.DraftTitle);
        Assert.Equal(StatusSeverity.Warning, vm.ActionStatus.Severity);
    }

    [AvaloniaFact]
    public async Task Reader_hides_editor_and_wires_hyperlinks_to_the_application()
    {
        var vm = CreateViewModel();
        await vm.InitializeAsync();
        vm.SelectedPost = Post("[Vidéo](clip.mp4)");
        var page = new BlogsPage { DataContext = vm };
        var nav = new NavigationPage();
        await nav.PushAsync(page);
        var window = new Window { Width = 800, Height = 700, Content = nav };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.True(page.FindControl<Grid>("PostReader")!.IsVisible);
            Assert.False(page.FindControl<Grid>("PostEditor")!.IsVisible);
            Assert.False(page.SaveButton.IsVisible);
            var reader = page.FindControl<MarkdownScrollViewer>("ArticleReader")!;
            Assert.Equal(vm.ReaderArticle, reader.Markdown);
            Assert.Equal(vm.ReaderBaseUrl, reader.AssetPathRoot);
            Assert.Same(vm.OpenResourceCommand, Assert.IsAssignableFrom<IMarkdownEngine2>(reader.Engine).HyperlinkCommand);
            vm.EditPostCommand.Execute(null);
            Assert.False(page.FindControl<Grid>("PostReader")!.IsVisible);
            Assert.True(page.FindControl<Grid>("PostEditor")!.IsVisible);
            Assert.True(page.SaveButton.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Long_reading_content_scrolls_and_clears_when_selection_is_removed()
    {
        var vm = CreateViewModel();
        await vm.InitializeAsync();
        vm.SelectedPost = Post(string.Join("\n\n", Enumerable.Range(0, 100).Select(i => $"Paragraphe {i}.")));
        var page = new BlogsPage { DataContext = vm };
        var nav = new NavigationPage();
        await nav.PushAsync(page);
        var window = new Window { Width = 800, Height = 700, Content = nav };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var reader = page.FindControl<MarkdownScrollViewer>("ArticleReader")!;
            var scroll = reader.GetVisualDescendants().OfType<ScrollViewer>().Single();
            Assert.InRange(scroll.Viewport.Height, 1, window.ClientSize.Height);
            Assert.True(scroll.Extent.Height > scroll.Viewport.Height);
            scroll.ScrollToEnd();
            Assert.True(scroll.Offset.Y > 0);
            vm.SelectedPost = null;
            Dispatcher.UIThread.RunJobs();
            Assert.Empty(reader.Markdown!);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Clicking_a_user_video_pushes_a_dedicated_video_viewmodel()
    {
        using var client = new HttpClient(new BlogResourceServiceTests.StubHandler(request =>
        {
            Assert.Equal("https://org.example.test/files/paul/clip.mp4", request.RequestUri!.AbsoluteUri);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content.Headers.ContentType = new("video/mp4");
            return response;
        }));
        var vm = CreateViewModel(new BlogResourceService(client));
        vm.SelectedPost = Post();
        var app = (App)Application.Current!;
        var root = new MainView();
        app.AttachMainWindow(root);
        var template = new FuncDataTemplate<VideoPageViewModel>((_, _) => new ContentPage());
        app.DataTemplates.Insert(0, template);
        try
        {
            var page = new BlogsPage { DataContext = vm };
            var reader = page.FindControl<MarkdownScrollViewer>("ArticleReader")!;
            Dispatcher.UIThread.RunJobs();
            var engine = Assert.IsAssignableFrom<IMarkdownEngine2>(reader.Engine);
            var link = Assert.IsType<CHyperlink>(Assert.Single(engine.ParseGamutInline("[Vidéo](clip.mp4)")));
            Assert.NotNull(link.Command);
            link.Command(link.CommandParameter!);
            Assert.NotNull(vm.OpenResourceCommand.ExecutionTask);
            await vm.OpenResourceCommand.ExecutionTask;
            var target = Assert.IsType<VideoPageViewModel>(root.NavRoot.NavigationStack.Last().DataContext);
            Assert.Equal("https://org.example.test/files/paul/clip.mp4", target.Source.AbsoluteUri);
            Assert.Equal("video/mp4", target.MimeType);
        }
        finally
        {
            app.DataTemplates.Remove(template);
        }
    }

    [AvaloniaFact]
    public void Video_viewmodel_resolves_to_a_page_with_a_vlc_control()
    {
        var vm = new VideoPageViewModel(new Uri("https://org.example.test/files/paul/clip.mp4"), "video/mp4");
        var page = Assert.IsType<VideoPage>(new ViewLocator().Build(vm));
        Assert.NotNull(page.FindControl<LibVLCSharp.Avalonia.VideoView>("Video"));
    }

    [Fact]
    public async Task Broken_resource_reports_an_error_and_releases_busy_state()
    {
        using var client = new HttpClient(new BlogResourceServiceTests.StubHandler(_ => new(HttpStatusCode.NotFound)));
        var vm = CreateViewModel(new BlogResourceService(client));
        vm.SelectedPost = Post();
        await vm.OpenResourceCommand.ExecuteAsync("absent.mp4");
        Assert.False(vm.IsBusy);
        Assert.Equal(StatusSeverity.Error, vm.ActionStatus.Severity);
    }
}
