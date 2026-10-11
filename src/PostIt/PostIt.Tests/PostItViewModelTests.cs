using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Yavsc.Blogspot;
using Yavsc.Api.Client;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views.Blogs;

namespace PostIt.Tests;

public class PostItViewModelTests
{
    [Fact]
    public void Draft_modified_tracks_title_body_and_attachments_not_publication()
    {
        var vm = new BlogsViewModel(new BlogApiClient(new ThrowingYavscApiClient(), "http://localhost/"));
        vm.SelectedPost = new BlogPostDto { Id = 42, AuthorId = "tester", Title = "Titre", Article = "Corps" };
        Assert.False(vm.IsDraftModified);
        vm.DraftIsPublished = true;
        Assert.False(vm.IsDraftModified);
        vm.DraftTitle = "Autre";
        Assert.True(vm.IsDraftModified);
        vm.DraftTitle = "Titre";
        Assert.False(vm.IsDraftModified);
        vm.DraftArticleDocument!.Insert(0, "Edit ");
        Assert.True(vm.IsDraftModified);
        vm.DraftArticle = "Corps";
        Assert.False(vm.IsDraftModified);
        vm.DraftAttachments.Add(new BlogUploadFile("note.txt", new byte[] { 1 }));
        Assert.True(vm.IsDraftModified);
    }

    [AvaloniaFact]
    public async Task Save_keeps_selection_and_editor_open_and_resets_highlight()
    {
        var recorder = new CallRecorder();
        var vm = new BlogsViewModel(new BlogApiClient(new RecordingYavscApiClient(recorder),
            "https://blogs.example.test/api/v1/"));
        var selected = new BlogPostDto { Id = 42, AuthorId = "tester", Title = "Titre", Article = "Corps" };
        vm.Posts.Add(selected);
        vm.SearchText = "";
        vm.SelectedPost = selected;
        var page = new BlogsPage { DataContext = vm };
        var navigation = new NavigationPage();
        await navigation.PushAsync(page);
        var window = new Window { Content = navigation };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            selected = Assert.Single(vm.FilteredPosts);
            selected.AuthorId = "tester";
            selected.Title = "Titre";
            selected.Article = "Corps";
            page.PostsListBox.SelectedItem = selected;
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selected, page.PostsListBox.SelectedItem);
            Assert.Same(selected, vm.SelectedPost);
            Assert.DoesNotContain("modified", page.SaveButton.Classes);
            vm.DraftTitle = "Titre modifié";
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("modified", page.SaveButton.Classes);
            Assert.Equal(Avalonia.Media.FontWeight.Bold, page.SaveButton.FontWeight);
            Assert.Same(vm.SaveCommand, page.SaveButton.Command);
            await vm.SaveAsync();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(page, navigation.NavigationStack.Last());
            Assert.Same(selected, vm.SelectedPost);
            Assert.Same(selected, page.PostsListBox.SelectedItem);
            Assert.Equal("Titre modifié", vm.DraftTitle);
            Assert.Equal("Corps", vm.DraftArticle);
            Assert.DoesNotContain("modified", page.SaveButton.Classes);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public async Task Failed_save_preserves_draft_attachments_and_modified_state()
    {
        var vm = new BlogsViewModel(new BlogApiClient(new ThrowingYavscApiClient(), "http://localhost/"));
        var post = new BlogPostDto { Id = 42, AuthorId = "tester", Title = "Titre", Article = "Corps" };
        vm.SelectedPost = post;
        vm.DraftTitle = "Titre modifié";
        vm.DraftAttachments.Add(new BlogUploadFile("note.txt", new byte[] { 1 }));
        var article = vm.DraftArticle;
        await vm.SaveAsync();
        Assert.Same(post, vm.SelectedPost);
        Assert.Equal("Titre modifié", vm.DraftTitle);
        Assert.Equal(article, vm.DraftArticle);
        Assert.Single(vm.DraftAttachments);
        Assert.True(vm.IsDraftModified);
        Assert.False(vm.IsBusy);
    }

    [AvaloniaFact]
    public void Published_badge_tracks_post_publication_state()
    {
        var post = new BlogPostDto { Id = 42, Title = "Test post", IsPublished = true };
        var page = new BlogsPage();
        var row = page.PostsListBox.ItemTemplate!.Build(post)!;
        row.DataContext = post;
        var window = new Window { Content = row };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var badge = Assert.Single(row.GetVisualDescendants().OfType<Border>(),
                border => border.Name == "PublishedBadge");

            Assert.True(badge.IsVisible);

            post.IsPublished = false;
            Dispatcher.UIThread.RunJobs();
            Assert.False(badge.IsVisible);

            post.IsPublished = true;
            Dispatcher.UIThread.RunJobs();
            Assert.True(badge.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void SearchCommand_filters_posts_by_title_article_or_author()
    {
        // BlogsViewModel no longer owns a BlogApiClient instance by
        // default; tests construct one with a fake YavscApiClient that
        // throws on any call (we never call the API in this test).
        var fakeApi = new ThrowingYavscApiClient();
        var blog = new BlogApiClient(fakeApi, "http://localhost/");
        var viewModel = new BlogsViewModel(blog);

        viewModel.Posts.Add(new BlogPostDto { Id = 1, Title = "First post", Article = "Hello world", AuthorId = "alice" });
        viewModel.Posts.Add(new BlogPostDto { Id = 2, Title = "Second post", Article = "Nothing here", AuthorId = "bob" });
        viewModel.Posts.Add(new BlogPostDto { Id = 3, Title = "Third post", Article = "Search me", AuthorId = "carol" });

        viewModel.SearchText = "search";

        Assert.Single(viewModel.FilteredPosts);
        Assert.Equal(3, viewModel.FilteredPosts[0].Id);

        viewModel.SearchText = "bob";

        Assert.Single(viewModel.FilteredPosts);
        Assert.Equal(2, viewModel.FilteredPosts[0].Id);
    }

    [Fact]
    public async Task BlogApiClient_GetPostsAsync_returns_posts_from_api()
    {
        // The new BlogApiClient delegates transport to YavscApiClient.
        // We feed it a fake YavscApiClient that returns the expected
        // list straight from CallAsync.
        var expected = new List<BlogPostDto>
        {
            new() { Id = 1, Title = "Hello" },
            new() { Id = 2, Title = "World" }
        };
        var api = new StubYavscApiClient(expected);
        var blog = new BlogApiClient(api, "http://localhost/");

        var posts = await blog.GetPostsAsync(ct: TestContext.Current.CancellationToken);

        Assert.Equal(2, posts.Count);
        Assert.Equal("Hello", posts[0].Title);
    }

    [Fact]
    public async Task TogglePublishCommand_uses_the_current_checked_state_without_inverting_it()
    {
        var api = new RecordingPublishApi();
        var blog = new BlogApiClient(api, "http://localhost/");
        var viewModel = new BlogsViewModel(blog);

        viewModel.SelectedPost = new BlogPostDto { Id = 42, IsPublished = false };
        var changes = new List<string?>();
        viewModel.SelectedPost.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        await viewModel.SetPublishStateAsync(true);

        Assert.True(api.LastPublishValue);
        Assert.True(viewModel.DraftIsPublished);
        Assert.True(viewModel.SelectedPost.IsPublished);
        Assert.Equal(new[] { nameof(BlogPostDto.IsPublished) }, changes);

        await viewModel.SetPublishStateAsync(false);

        Assert.False(api.LastPublishValue);
        Assert.False(viewModel.DraftIsPublished);
        Assert.False(viewModel.SelectedPost.IsPublished);
        Assert.Equal(new[] { nameof(BlogPostDto.IsPublished), nameof(BlogPostDto.IsPublished) }, changes);
    }

    /// <summary>Test fake that always throws if the API is invoked.</summary>
    private sealed class ThrowingYavscApiClient : YavscApiClient
    {
        public ThrowingYavscApiClient() : base(
            new Settings
            {
                Authentication = new AuthenticationSettings
                {
                    Authority = "https://stub.invalid",
                    ClientId = "stub",
                    Scopes = new[] { "openid" },
                },
            },
            new TokenStore(System.IO.Path.GetTempFileName()))
        { }
        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => throw new System.InvalidOperationException("ThrowingYavscApiClient: API not stubbed.");
    }

    /// <summary>Test fake that hands back a canned list of posts from any CallAsync.</summary>
    private sealed class StubYavscApiClient : YavscApiClient
    {
        private readonly List<BlogPostDto> _posts;
        public StubYavscApiClient(List<BlogPostDto> posts)
            : base(
                new Settings
                {
                    Authentication = new AuthenticationSettings
                    {
                        Authority = "https://stub.invalid",
                        ClientId = "stub",
                        Scopes = new[] { "openid" },
                    },
                },
                new TokenStore(System.IO.Path.GetTempFileName()))
        {
            _posts = posts;
        }

        public override Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            // The canned fake only knows about a list of posts; the
            // BlogApiClient test asserts on that list directly.
            if (typeof(T) == typeof(List<BlogPostDto>))
                return Task.FromResult((T)(object)_posts);
            return Task.FromResult(default(T)!);
        }
    }

    private sealed class RecordingPublishApi : IYavscApiClient
    {
        public bool LastPublishValue { get; private set; }
        public HttpClient Http { get; } = new();

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            if (method == HttpMethod.Put && path.Contains("/publish", StringComparison.OrdinalIgnoreCase))
            {
                var publish = body?.GetType().GetProperty("publish")?.GetValue(body) is bool value && value;
                LastPublishValue = publish;
            }

            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            if (method == HttpMethod.Put && path.Contains("/publish", StringComparison.OrdinalIgnoreCase))
            {
                var publish = body?.GetType().GetProperty("publish")?.GetValue(body) is bool value && value;
                LastPublishValue = publish;
            }

            return Task.CompletedTask;
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<T>(method, path, (object?)null, ct);

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync(method, path, (object?)null, ct);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
