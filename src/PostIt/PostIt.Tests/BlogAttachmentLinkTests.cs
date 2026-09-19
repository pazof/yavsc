using Yavsc.Blogspot;
using Yavsc.Api.Client;
using PostIt.ViewModels;

namespace PostIt.Tests;

/// <summary>
/// Pins the markdown link that <see cref="BlogsViewModel.SaveAsync"/>
/// appends to the article after a successful attachment upload.
///
/// <para>The link must be an absolute URL derived from the OIDC
/// authority via <c>FileServerUrlHelpers.GetUserFilesUri</c>
/// (Yavsc.Abstract): the <c>/files</c> file server is hosted by
/// Yavsc.Org — the IdP — not by the blogs API host, so a link
/// built from <c>BlogsApiUrl</c> (or left relative) 404s when the
/// reader opens the post. The regression this guards: a generated
/// link of the shape <c>/files/{user}/blogs/{id}/{name}</c>
/// (relative, authority-less) instead of
/// <c>https://{authority}/files/{user}/blogs/{id}/{name}</c>.</para>
/// </summary>
public class BlogAttachmentLinkTests
{
    [Fact]
    public async Task Save_with_attachment_appends_absolute_link_derived_from_the_oidc_authority()
    {
        const string authority = "https://idp.example.test";

        var recorder = new CallRecorder();
        var api = new RecordingYavscApiClient(recorder);
        var blog = new BlogApiClient(api, "https://blogs.example.test/api/v1/");
        var settings = new Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = authority,
                ClientId = "stub",
                Scopes = new[] { "openid" },
            },
        };
        var vm = new BlogsViewModel(blog, settings);

        // Update branch: an existing post is selected, the user
        // edits the buffers and attaches a file.
        vm.SelectedPost = new BlogPostDto
        {
            Id = 42,
            AuthorId = "tester",
            Title = "Avant",
            Article = "Contenu initial.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        vm.DraftTitle = "Après";
        vm.DraftArticle = "Contenu modifié.";
        vm.DraftAttachments.Add(new BlogUploadFile(
            "note.txt",
            System.Text.Encoding.UTF8.GetBytes("payload test"),
            "text/plain"));

        await vm.SaveAsync();

        // The update branch issues two PUTs: the multipart one
        // carrying the file, then — because TryAppendAttachmentLinks
        // appended a link — a JSON one carrying the final article.
        var linkPut = recorder.Calls.LastOrDefault(
            c => c.method == HttpMethod.Put);
        Assert.NotEqual(default, linkPut);
        var sent = Assert.IsType<System.Func<HttpContent>>(linkPut.body);
        var multipartContent = Assert.IsType<MultipartFormDataContent>(sent());

        var expectedUrl = $"{authority}/files/tester/blogs/42/note.txt";
        var sentStringContent =  multipartContent.First() as StringContent ;

        var sentString = await sentStringContent.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains($"- [note.txt]({expectedUrl})", sentString);
        // The regression shape: a relative, authority-less link.
        Assert.DoesNotContain("](/files/", sentString);
    }
}
