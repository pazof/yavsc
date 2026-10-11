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
    [Theory]
    [InlineData("note.txt", "note.txt")]
    [InlineData("INSC_E03686381_SCHNEIDER_000113378AA__Troisi\u00e8me__.PDF",
        "INSC_E03686381_SCHNEIDER_000113378AA__Troisi_232me__.PDF")]
    [InlineData("rapport final.pdf", "rapport%20final.pdf")]
    public async Task Save_with_attachment_appends_absolute_link_derived_from_the_oidc_authority(
        string originalName, string urlName)
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
            originalName,
            System.Text.Encoding.UTF8.GetBytes("payload test"),
            "text/plain"));

        var expectedUrl = $"{authority}/files/tester/blogs/42/{urlName}";
        Assert.Contains($"- [{originalName}]({expectedUrl})", vm.DraftArticle);
        Assert.Empty(recorder.Calls);
        var draftBeforeSave = vm.DraftArticle;

        await vm.SaveAsync();

        var linkPut = Assert.Single(recorder.Calls, c => c.method == HttpMethod.Put);
        Assert.NotEqual(default, linkPut);
        var sent = Assert.IsType<System.Func<HttpContent>>(linkPut.body);
        var multipartContent = Assert.IsType<MultipartFormDataContent>(sent());

        var sentStringContent = Assert.IsType<StringContent>(multipartContent.First());

        var sentString = await sentStringContent.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = System.Text.Json.JsonDocument.Parse(sentString);
        var article = document.RootElement.GetProperty("article").GetString();
        Assert.Contains($"- [{originalName}]({expectedUrl})", article);
        // The regression shape: a relative, authority-less link.
        Assert.DoesNotContain("](/files/", article);
        Assert.Equal(draftBeforeSave, vm.DraftArticle);
        Assert.NotNull(vm.SelectedPost);
        Assert.False(vm.IsDraftModified);
    }

    [Fact]
    public async Task New_post_resolves_only_the_provisional_link_at_first_save()
    {
        var recorder = new CallRecorder();
        var api = new RecordingYavscApiClient(recorder);
        var vm = new BlogsViewModel(new BlogApiClient(api, "https://blogs.example.test/api/v1/"),
            new Settings { Authentication = new AuthenticationSettings { Authority = "https://idp.example.test" } });
        vm.DraftTitle = "Nouveau billet";
        vm.DraftArticle = "Texte conservé.";
        vm.DraftAttachments.Add(new BlogUploadFile("Troisi\u00e8me.PDF", new byte[] { 1 }));

        Assert.Contains("postit-attachment:", vm.DraftArticle);
        Assert.Empty(recorder.Calls);
        await vm.SaveAsync();

        Assert.Equal("Nouveau billet", vm.DraftTitle);
        Assert.Contains("Texte conservé.", vm.DraftArticle);
        Assert.Contains("- [Troisi\u00e8me.PDF](https://idp.example.test/files/tester/blogs/42/Troisi_232me.PDF)",
            vm.DraftArticle);
        Assert.DoesNotContain("postit-attachment:", vm.DraftArticle);
        Assert.Equal(42, vm.SelectedPost?.Id);
        Assert.Empty(vm.DraftAttachments);
        Assert.False(vm.IsDraftModified);
        Assert.Single(recorder.Calls, c => c.method == HttpMethod.Post);
        Assert.Single(recorder.Calls, c => c.method == HttpMethod.Put);
    }
}
