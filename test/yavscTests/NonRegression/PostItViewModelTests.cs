using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels;
using Xunit;

namespace Yavsc.Tests.NonRegression;

public class PostItViewModelTests
{
    [Fact]
    public void SearchCommand_filters_posts_by_title_article_or_author()
    {
        var viewModel = new MainViewModel();

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
        var expected = new List<BlogPost>
        {
            new() { Id = 1, Title = "Hello" },
            new() { Id = 2, Title = "World" }
        };

        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expected));
        using var client = new HttpClient(handler)
        {
            BaseAddress = new System.Uri("http://localhost/")
        };

        using var apiClient = new BlogApiClient(client);
        var posts = await apiClient.GetPostsAsync();

        Assert.Equal(2, posts.Count);
        Assert.Equal("Hello", posts[0].Title);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
