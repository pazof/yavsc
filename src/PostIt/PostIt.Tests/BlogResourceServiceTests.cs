using System.Net;
using System.Net.Http.Headers;
using PostIt.Services;

namespace PostIt.Tests;

public class BlogResourceServiceTests
{
    private const string Authority = "https://yavsc.pschneider.fr";

    [Theory]
    [InlineData("IMG-20260923-WA0001.jpg", Authority + "/files/paul/IMG-20260923-WA0001.jpg")]
    [InlineData("blogs/42/vidéo.mp4", Authority + "/files/paul/blogs/42/vid%C3%A9o.mp4")]
    [InlineData("/files/alice/video.webm", Authority + "/files/alice/video.webm")]
    [InlineData(Authority + "/files/paul/photo.jpg", Authority + "/files/paul/photo.jpg")]
    [InlineData("https://elsewhere.test/page", "https://elsewhere.test/page")]
    [InlineData("//elsewhere.test/page", "https://elsewhere.test/page")]
    public void Resolves_links_against_org_user_login_not_blogs_host(string link, string expected)
        => Assert.Equal(expected, BlogResourceService.Resolve(link, Authority + "/connect/", "paul").AbsoluteUri);

    [Fact]
    public void Encodes_login_and_keeps_trailing_slash()
        => Assert.Equal(Authority + "/files/paul%20test/",
            BlogResourceService.GetUserBaseUri(Authority, "paul test").AbsoluteUri);

    [Theory]
    [InlineData(Authority + "/files/paul/movie.mp4", true)]
    [InlineData(Authority + "/files/alice/blogs/42/movie.mp4?download=1", true)]
    [InlineData(Authority + "/files/paul/", false)]
    [InlineData(Authority + "/files/paul", false)]
    [InlineData(Authority + "/files/", false)]
    [InlineData(Authority + "/files-other/paul/movie.mp4", false)]
    [InlineData(Authority + "/page.mp4", false)]
    [InlineData("https://blogs.pschneider.fr/files/paul/movie.mp4", false)]
    [InlineData("https://yavsc.pschneider.fr.evil.test/files/paul/movie.mp4", false)]
    [InlineData("https://yavsc.pschneider.fr:8443/files/paul/movie.mp4", false)]
    [InlineData("http://yavsc.pschneider.fr/files/paul/movie.mp4", false)]
    [InlineData("https://user@yavsc.pschneider.fr/files/paul/movie.mp4", false)]
    public void Recognizes_only_files_on_the_configured_org_origin(string link, bool expected)
        => Assert.Equal(expected, BlogResourceService.IsUserResource(new Uri(link), Authority));

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///tmp/video.mp4")]
    [InlineData("data:video/mp4;base64,AAAA")]
    public void Rejects_non_web_links(string link)
        => Assert.Throws<InvalidOperationException>(() => BlogResourceService.Resolve(link, Authority, "paul"));

    [Theory]
    [InlineData("video/webm", "movie.jpg", "video/webm")]
    [InlineData("image/jpeg", "movie.mp4", "image/jpeg")]
    [InlineData("application/octet-stream", "movie.MP4", "video/mp4")]
    [InlineData(null, "movie.webm", "video/webm")]
    [InlineData(null, "IMG-20260923-WA0001.jpg", "image/jpeg")]
    [InlineData(null, "file.unknown", "application/octet-stream")]
    public async Task Prefers_server_mime_with_extension_fallback(string? mime, string file, string expected)
    {
        using var client = new HttpClient(new StubHandler(request =>
        {
            Assert.Equal(HttpMethod.Head, request.Method);
            return Response(HttpStatusCode.OK, mime);
        }));
        var service = new BlogResourceService(client);
        Assert.Equal(expected, await service.GetMimeTypeAsync(new Uri($"{Authority}/files/paul/{file}?v=1"),
            TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.NotImplemented)]
    public async Task Falls_back_to_a_ranged_get_when_head_is_not_supported(HttpStatusCode status)
    {
        var calls = 0;
        using var client = new HttpClient(new StubHandler(request =>
        {
            calls++;
            if (request.Method == HttpMethod.Head)
                return Response(status, null);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("bytes=0-0", request.Headers.Range?.ToString());
            return Response(HttpStatusCode.PartialContent, "video/mp4");
        }));
        Assert.Equal("video/mp4", await new BlogResourceService(client)
            .GetMimeTypeAsync(new Uri($"{Authority}/files/paul/movie"), TestContext.Current.CancellationToken));
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Does_not_disguise_http_errors_as_a_mime_guess(HttpStatusCode status)
    {
        using var client = new HttpClient(new StubHandler(_ => Response(status, null)));
        await Assert.ThrowsAsync<HttpRequestException>(() => new BlogResourceService(client)
            .GetMimeTypeAsync(new Uri($"{Authority}/files/paul/movie.mp4"), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("photo.jpg", Authority + "/files/paul/photo.jpg")]
    [InlineData("/files/alice/photo.jpg", Authority + "/files/alice/photo.jpg")]
    [InlineData(Authority + "/files/alice/photo.jpg", Authority + "/files/alice/photo.jpg")]
    public async Task Inline_images_use_the_same_user_base_and_support_root_relative_paths(string link, string expected)
    {
        using var client = new HttpClient(new StubHandler(request =>
        {
            Assert.Equal(expected, request.RequestUri!.AbsoluteUri);
            return Response(HttpStatusCode.OK, "image/jpeg");
        }));
        var resolver = new BlogImageResolver(client) { AssetPathRoot = Authority + "/files/paul/" };
        await using var stream = await resolver.ResolveImageResource(link);
        Assert.NotNull(stream);
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("data:image/png;base64,AAAA")]
    public async Task Inline_images_cannot_read_local_files_or_non_web_schemes(string link)
    {
        var resolver = new BlogImageResolver { AssetPathRoot = Authority + "/files/paul/" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveImageResource(link));
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string? mime)
    {
        var response = new HttpResponseMessage(status) { Content = new ByteArrayContent([]) };
        if (mime is not null)
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mime);
        return response;
    }

    internal sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }
}
