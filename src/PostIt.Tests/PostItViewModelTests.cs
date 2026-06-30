using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Tests;

public class PostItViewModelTests
{

    [Fact]
    public void SearchCommand_filters_posts_by_title_article_or_author()
    {
        // MainPageViewModel no longer owns a BlogApiClient instance by
        // default; tests construct one with a fake YavscApiClient that
        // throws on any call (we never call the API in this test).
        var fakeApi = new ThrowingYavscApiClient();
        var blog = new BlogApiClient(fakeApi);
        var viewModel = new MainPageViewModel(blog);

        viewModel.Posts.Add(new BlogPost { Id = 1, Title = "First post", Article = "Hello world", AuthorId = "alice" });
        viewModel.Posts.Add(new BlogPost { Id = 2, Title = "Second post", Article = "Nothing here", AuthorId = "bob" });
        viewModel.Posts.Add(new BlogPost { Id = 3, Title = "Third post", Article = "Search me", AuthorId = "carol" });

        viewModel.SearchText = "search";
        viewModel.SearchCommand.Execute(null);

        Assert.Single(viewModel.FilteredPosts);
        Assert.Equal(3, viewModel.FilteredPosts[0].Id);

        viewModel.SearchText = "bob";
        viewModel.SearchCommand.Execute(null);

        Assert.Single(viewModel.FilteredPosts);
        Assert.Equal(2, viewModel.FilteredPosts[0].Id);
    }

    [Fact]
    public async Task BlogApiClient_GetPostsAsync_returns_posts_from_api()
    {
        // The new BlogApiClient delegates transport to YavscApiClient.
        // We feed it a fake YavscApiClient that returns the expected
        // list straight from CallAsync.
        var expected = new List<BlogPost>
        {
            new() { Id = 1, Title = "Hello" },
            new() { Id = 2, Title = "World" }
        };
        var api = new StubYavscApiClient(expected);
        var blog = new BlogApiClient(api);

        var posts = await blog.GetPostsAsync();

        Assert.Equal(2, posts.Count);
        Assert.Equal("Hello", posts[0].Title);
    }

    /// <summary>Test fake that always throws if the API is invoked.</summary>
    private sealed class ThrowingYavscApiClient : YavscApiClient
    {
        public ThrowingYavscApiClient() : base(
            new Settings
            {
                Scopes = new[] { "openid" },
                Authentication = new AuthenticationSettings
                {
                    Authority = "https://stub.invalid",
                    ClientId = "stub",
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
        private readonly List<BlogPost> _posts;
        public StubYavscApiClient(List<BlogPost> posts)
            : base(
                new Settings
                {
                    Scopes = new[] { "openid" },
                    Authentication = new AuthenticationSettings
                    {
                        Authority = "https://stub.invalid",
                        ClientId = "stub",
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
            if (typeof(T) == typeof(List<BlogPost>))
                return Task.FromResult((T)(object)_posts);
            return Task.FromResult(default(T)!);
        }
    }
}
