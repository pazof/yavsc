using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using Yavsc.Models;
using Yavsc.Server.Helpers;

namespace Yavsc.Blogs.Tests;

[Trait("Category", "non-reg")]
public sealed class FilterFileNameTests
{
    [Theory]
    [InlineData("note.txt", "note.txt")]
    [InlineData("ABC-123=_~.PDF", "ABC-123=_~.PDF")]
    [InlineData("Troisi\u00e8me.PDF", "Troisi_232me.PDF")]
    [InlineData("\u00e9\u00e0\u00e7.pdf", "_233_224_231.pdf")]
    [InlineData("folder/file\\name.pdf", "folder_047file_092name.pdf")]
    public void Filter_encodes_unsupported_characters_without_URL_fragments(
        string originalName, string expectedName)
    {
        var filtered = AbstractFileSystemHelpers.FilterFileName(originalName);

        Assert.Equal(expectedName, filtered);
        Assert.DoesNotContain("#", filtered);
        Assert.True(filtered.IsValidYavscPath());
        Assert.Equal(filtered, AbstractFileSystemHelpers.FilterFileName(filtered));
    }

    [Theory]
    [InlineData(true, 5)]
    [InlineData(false, 2)]
    public void Paid_upload_accepts_exact_effective_quota_and_refuses_overflow_without_touching_files(bool premium, long multiplier)
    {
        var root = Path.Combine(Path.GetTempPath(), $"yavsc-disk-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var limit = Yavsc.Constants.DefaultFSQ * multiplier;
            var user = new ApplicationUser
            {
                DiskQuota = 10L, DiskUsage = limit - 3,
                PremiumDiskQuotaUntil = premium ? DateTimeOffset.UtcNow.AddDays(1) : null,
                DiscoveryDiskQuotaUntil = premium ? null : DateTimeOffset.UtcNow.AddDays(1)
            };
            using var input = new MemoryStream(new byte[] { 1, 2, 3 });
            var received = user.ReceiveUserFile(root, new FormFile(input, 0, 3, "file", "disk.bin")
            {
                Headers = new HeaderDictionary(), ContentType = "application/octet-stream"
            });
            Assert.False(received.QuotaOffense);
            Assert.Equal(limit, user.DiskUsage);
            using var extra = new MemoryStream(new byte[] { 4 });
            Assert.True(user.ReceiveUserFile(root, new FormFile(extra, 0, 1, "file", "extra.bin")).QuotaOffense);
            Assert.False(File.Exists(Path.Combine(root, "extra.bin")));
            user.PremiumDiskQuotaUntil = DateTimeOffset.UtcNow.AddSeconds(-1);
            user.DiscoveryDiskQuotaUntil = DateTimeOffset.UtcNow.AddSeconds(-1);
            using var replacement = new MemoryStream(new byte[] { 5 });
            Assert.True(user.ReceiveUserFile(root, new FormFile(replacement, 0, 1, "file", "disk.bin")).QuotaOffense);
            Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(Path.Combine(root, "disk.bin")));
            Assert.Equal(limit, user.DiskUsage);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Uploaded_accented_filename_is_accessible_through_static_files()
    {
        var root = Path.Combine(Path.GetTempPath(), $"yavsc-filename-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("accented filename regression");
            using var input = new MemoryStream(payload);
            var file = new FormFile(input, 0, payload.Length, "file",
                "INSC_E03686381_SCHNEIDER_000113378AA__Troisi\u00e8me__.PDF")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
            var user = new ApplicationUser { DiskQuota = 10_000_000 };
            var received = user.ReceiveUserFile(root, file);
            Assert.False(received.QuotaOffense);
            Assert.Equal("INSC_E03686381_SCHNEIDER_000113378AA__Troisi_232me__.PDF",
                received.FileName);

            using var provider = new PhysicalFileProvider(root);
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            await using var app = builder.Build();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = provider,
                RequestPath = "/files"
            });
            await app.StartAsync(TestContext.Current.CancellationToken);
            using var http = app.GetTestClient();
            var uri = new Uri(http.BaseAddress!, $"/files/{received.FileName}");
            Assert.Equal(string.Empty, uri.Fragment);
            using var response = await http.GetAsync(uri, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(payload,
                await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
