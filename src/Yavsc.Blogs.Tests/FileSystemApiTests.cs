using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Blogs.Tests.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Integration tests for the personal-storage file endpoints
/// (<c>api/v1/fs</c>) served by <c>FileSystemApiController</c> on the
/// Blogs host.
///
/// <para>Two concerns:</para>
/// <list type="number">
///   <item><description><b>Upload records metadata</b>: a multipart
///   <c>POST api/v1/fs</c> persists an <see cref="UploadedFile"/> row
///   owned by the caller and returns its <c>fileId</c>, so the client
///   can later attach that file to an estimate/query by
///   reference.</description></item>
///   <item><description><b>Download authorization</b>:
///   <c>GET api/v1/fs/file/{fileId}</c> serves the bytes from the
///   owner's personal space. The owner may always read; a party
///   (client or provider) of an estimate/query the file is attached
///   to may read; a third party is refused. The bytes live on the
///   Blogs host's disk — this is the cross-container reason the
///   download endpoint lives here and not on the API host.</description></item>
/// </list>
/// </summary>
[Collection("Yavsc Blogs")]
public sealed class FileSystemApiTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public FileSystemApiTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient NewClient(string subject = "alice", string? name = null)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.Addresses.First(a => a.StartsWith("https://")))
        };
        // Pass a distinct name to mirror production (sub = GUID, name =
        // login); defaults to subject for the existing tests where the
        // two are equal.
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, name: name));
        return http;
    }

    private static string PhysicalFile(BlogsWebServerFixture fixture, string owner, string fileName) =>
        Path.Combine(fixture.BlogFilesRoot, owner, fileName);

    /// <summary>Write a physical file into the owner's personal space,
    /// so <c>GetFile</c> can stream it back. Creates the directory if
    /// needed.</summary>
    private static void WritePhysicalFile(BlogsWebServerFixture fixture, string owner, string fileName, byte[] content)
    {
        var path = PhysicalFile(fixture, owner, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
    }

    private long SeedUploadedFile(string ownerId, string path, string contentType, long length)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var file = new UploadedFile
        {
            OwnerId = ownerId,
            Path = path,
            ContentType = contentType,
            Length = length,
        };
        db.UploadedFiles.Add(file);
        db.SaveChanges();
        return file.Id;
    }

    [Fact]
    public async Task Post_fs_creates_an_UploadedFile_row_owned_by_the_caller()
    {
        _fixture.ResetDatabase();
        // DiskQuota must be non-zero: ReceiveUserFile flags QuotaOffense
        // once usage >= DiskQuota, and the controller skips the metadata
        // row when QuotaOffense is set. A fresh user has quota 0.
        _fixture.SeedUser("alice", u => u.DiskQuota = 10_000_000);

        var payload = System.Text.Encoding.UTF8.GetBytes("hello personal storage");

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(payload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "note.txt");

        using var http = NewClient("alice");
        var response = await http.PostAsync("/api/v1/fs", form, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var item = Assert.Single(doc.RootElement.EnumerateArray());
        var fileId = item.GetProperty("fileId").GetInt64();
        Assert.True(fileId > 0, $"Expected a positive fileId, got {fileId}. Body: {body}");
        Assert.Equal("note.txt", item.GetProperty("fileName").GetString());
        Assert.False(item.GetProperty("quotaOffense").GetBoolean());

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var row = Assert.Single(db.UploadedFiles.Where(u => u.OwnerId == "alice"));
        Assert.Equal("note.txt", row.Path);
        Assert.Equal("text/plain", row.ContentType);
        Assert.Equal(payload.Length, row.Length);

        // The file physically landed under {BlogFilesRoot}/alice/note.txt.
        Assert.True(File.Exists(PhysicalFile(_fixture, "alice", "note.txt")),
            "Expected the uploaded file on disk under the owner's personal root");
    }

    /// <summary>
    /// Regression for the "Mes fichiers" page listing nothing: the
    /// personal-storage tree is keyed by the user's <b>login</b>
    /// (<c>UserName</c>) — <c>EnsureDestinationDirectory</c> writes
    /// under <c>{Blog}/{name-claim}</c>, the download endpoint and
    /// avatar paths use <c>user.UserName</c> — but <c>GET api/v1/fs</c>
    /// passed <c>User.GetUserId()</c> (the <c>sub</c> claim, a GUID in
    /// production) to <c>GetUserFiles</c>, so the listing looked under
    /// <c>{Blog}/{sub-GUID}</c>, an empty tree. This test decouples
    /// <c>sub</c> from the login (mirroring production) and asserts the
    /// listing returns the file stored under the login.
    /// </summary>
    [Fact]
    public async Task Get_fs_lists_files_stored_under_the_user_login()
    {
        _fixture.ResetDatabase();
        // sub = "alice-sub" (GUID-like), login = "alice" — the shape
        // ProfileService mints in production. DiskQuota non-zero so the
        // upload persists the UploadedFile row (see Post_fs test).
        _fixture.SeedUser("alice", u =>
        {
            u.Id = "alice-sub";
            u.DiskQuota = 10_000_000;
        });

        var payload = System.Text.Encoding.UTF8.GetBytes("list me");

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(payload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "note.txt");

        // Upload with a token whose sub is the GUID-like id and whose
        // name is the login — EnsureDestinationDirectory keys off "name".
        using (var http = NewClient("alice-sub", name: "alice"))
        {
            var post = await http.PostAsync("/api/v1/fs", form, TestContext.Current.CancellationToken);
            var postBody = await post.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(post.StatusCode == HttpStatusCode.OK,
                $"Upload failed: {(int)post.StatusCode} ({post.StatusCode}): {postBody}");
        }

        // The file landed under the login, not the subject.
        Assert.True(File.Exists(PhysicalFile(_fixture, "alice", "note.txt")),
            "Expected the uploaded file under {BlogFilesRoot}/alice/note.txt (the login)");
        Assert.False(Directory.Exists(Path.Combine(_fixture.BlogFilesRoot, "alice-sub")),
            "The subject-named directory must NOT exist — storage is keyed by the login");

        // GET /api/v1/fs must list under the login and find the file.
        using var get = NewClient("alice-sub", name: "alice");
        var response = await get.GetAsync("/api/v1/fs", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"List failed: {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("files", out var filesEl),
            $"Response has no 'files' property. Body: {body}");
        var names = filesEl.EnumerateArray().Select(f => f.GetProperty("name").GetString()).ToArray();
        Assert.Contains("note.txt", names);

        // The UploadedFile row (OwnerId = sub) is enriched onto the
        // entry, so the client can attach the file by reference.
        var noteEntry = filesEl.EnumerateArray()
            .First(f => f.GetProperty("name").GetString() == "note.txt");
        Assert.True(noteEntry.TryGetProperty("id", out var idEl) && idEl.GetInt64() > 0,
            "Expected the enriched UploadedFile id on the listed entry");
    }

    [Fact]
    public async Task GetFile_owner_can_download_their_own_file()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");

        var bytes = System.Text.Encoding.UTF8.GetBytes("owner payload");
        WritePhysicalFile(_fixture, "alice", "owner-note.txt", bytes);
        var fileId = SeedUploadedFile("alice", "owner-note.txt", "text/plain", bytes.Length);

        using var http = NewClient("alice");
        var response = await http.GetAsync(
            $"/api/v1/fs/file/{fileId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var downloaded = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(bytes, downloaded);
    }

    [Fact]
    public async Task GetFile_refuses_a_third_party()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("carol");

        var bytes = System.Text.Encoding.UTF8.GetBytes("private to alice");
        WritePhysicalFile(_fixture, "alice", "secret.txt", bytes);
        var fileId = SeedUploadedFile("alice", "secret.txt", "text/plain", bytes.Length);

        // carol is neither the owner nor a party to any estimate/query
        // this file is attached to.
        using var http = NewClient("carol");
        var response = await http.GetAsync(
            $"/api/v1/fs/file/{fileId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Seed the graph needed to prove a party (the client of an
    /// estimate the file is attached to) can download a file they do
    /// not own: alice owns the file and is the provider; bob is the
    /// client; the file is attached to the estimate by reference.
    /// </summary>
    private long SeedPartyDownloadGraph(out long estimateId, out string fileName)
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");
        _fixture.SeedUser("bob");

        fileName = "party-spec.pdf";
        var bytes = System.Text.Encoding.UTF8.GetBytes("party payload");

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Location + PerformerProfile(alice): Estimate.OwnerId is a FK
        // to PerformerProfile.PerformerId, so the provider profile must
        // exist before the estimate can be inserted.
        var location = new Location { Address = "1 rue du Test", Latitude = 48.8, Longitude = 2.3 };
        db.Locations.Add(location);
        db.SaveChanges();

        db.Performers.Add(new PerformerProfile
        {
            PerformerId = "alice",
            SIREN = "12345678901234",
            OrganizationAddressId = location.Id,
            ExerciseCountryCode = "fr",
            AcceptNotifications = true,
            AcceptPublicContact = true,
            Active = true,
            Rate = 5,
            WebSite = "https://alice.dev",
        });
        db.SaveChanges();

        var estimate = new Estimate
        {
            OwnerId = "alice",
            ClientId = "bob",
            CommandType = "Rdv",
            Title = "Devis party test",
            Description = "Devis",
            AttachedFiles = new List<string>(),
            AttachedGraphics = new List<string>(),
            Bill = new List<CommandLine>
            {
                new() { Name = "Prestation", Description = "Prestation", Count = 1, UnitaryCost = 10m, Currency = "EUR" },
            },
        };
        db.Estimates.Add(estimate);
        db.SaveChanges();
        estimateId = estimate.Id;

        var file = new UploadedFile
        {
            OwnerId = "alice",
            Path = fileName,
            ContentType = "application/pdf",
            Length = bytes.Length,
        };
        db.UploadedFiles.Add(file);
        db.SaveChanges();

        db.EstimateAttachedFiles.Add(new EstimateAttachedFile
        {
            FileId = file.Id,
            EstimateId = estimate.Id,
        });
        db.SaveChanges();

        // The bytes must exist on disk for GetFile to stream them.
        WritePhysicalFile(_fixture, "alice", fileName, bytes);
        return file.Id;
    }

    [Fact]
    public async Task GetFile_party_client_of_an_attached_estimate_can_download()
    {
        var fileId = SeedPartyDownloadGraph(out _, out _);

        // bob is the client of the estimate the file is attached to:
        // a party, authorized to read even though alice owns the file.
        using var http = NewClient("bob");
        var response = await http.GetAsync(
            $"/api/v1/fs/file/{fileId}", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");
        var downloaded = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes("party payload"), downloaded);
    }

    [Fact]
    public async Task GetFile_party_provider_of_an_attached_estimate_can_download()
    {
        var fileId = SeedPartyDownloadGraph(out _, out _);

        // alice is the owner too, so this also exercises the owner path;
        // the provider of an attached estimate is authorized regardless.
        using var http = NewClient("alice");
        var response = await http.GetAsync(
            $"/api/v1/fs/file/{fileId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFile_unknown_id_returns_404()
    {
        _fixture.ResetDatabase();
        _fixture.SeedUser("alice");

        using var http = NewClient("alice");
        var response = await http.GetAsync(
            "/api/v1/fs/file/999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}