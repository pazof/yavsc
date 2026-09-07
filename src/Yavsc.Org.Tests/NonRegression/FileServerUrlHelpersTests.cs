using Yavsc.Abstract.Files;

namespace Yavsc.Org.Tests.NonRegression;

public class FileServerUrlHelpersTests
{
    [Fact]
    public void GetUserFilesBaseUri_appends_the_user_files_path_to_the_authority_root()
    {
        var baseUri = FileServerUrlHelpers.GetUserFilesBaseUri("https://oidc.example.org");

        Assert.Equal("https://oidc.example.org/files/", baseUri.ToString());
    }

    [Fact]
    public void GetUserFilesBaseUri_preserves_the_authority_and_discards_any_existing_path()
    {
        var baseUri = FileServerUrlHelpers.GetUserFilesBaseUri("https://oidc.example.org/signin");

        Assert.Equal("https://oidc.example.org/files/", baseUri.ToString());
    }

    [Fact]
    public void GetUserFilesUri_builds_an_absolute_file_url_from_a_relative_path()
    {
        var fileUri = FileServerUrlHelpers.GetUserFilesUri(
            "https://oidc.example.org",
            "/alice/inbox/report.pdf");

        Assert.Equal("https://oidc.example.org/files/alice/inbox/report.pdf", fileUri.ToString());
    }

    [Fact]
    public void GetUserFilesUri_rejects_blank_relative_path()
    {
        Assert.Throws<ArgumentException>(
            () => FileServerUrlHelpers.GetUserFilesUri(
                "https://oidc.example.org",
                "   "));
    }
}
