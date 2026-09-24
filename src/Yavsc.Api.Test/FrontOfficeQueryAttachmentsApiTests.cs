using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Blog;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

/// <summary>
/// Integration tests for the query (demande) attachment endpoints
/// (<c>GET/POST/DELETE api/v1/front/query/{queryId}/attachments</c>),
/// which attach a file from the client's personal storage to their
/// <c>NominativeServiceCommand</c> **by reference**
/// (<see cref="QueryAttachedFile"/>).
///
/// <para>Coverage mirrors the controller contract: only the client
/// (the demande's <c>ClientId</c>) may attach or detach, the file must
/// belong to the caller, and listing is open to both parties
/// (client/provider) plus admin. A third party is refused.</para>
/// </summary>
[Collection("Yavsc Api")]
public sealed class FrontOfficeQueryAttachmentsApiTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public FrontOfficeQueryAttachmentsApiTests(ApiWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient NewClient(string subject = "bob", string scope = "api")
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.BaseAddress)
        };
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, scope));
        return http;
    }

    /// <summary>
    /// Seed an Rdv query from bob (client) to alice (provider) and two
    /// personal <see cref="UploadedFile"/> rows: one owned by bob (the
    /// client's own file) and one owned by alice (a file that does NOT
    /// belong to the client, used to assert the ownership guard).
    /// </summary>
    private (long queryId, long bobFileId, long aliceFileId) Seed()
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedActivityGraph();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var location = db.Locations.Single(l => l.Address == "1 rue du Test");

        var query = new RdvQuery
        {
            ActivityCode = "dev",
            ClientId = "bob",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "bob",
            UserModified = "bob",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = location,
            Reason = "Demande de devis",
            Status = QueryStatus.InProgress,
            Description = "Demande en attente",
        };
        db.RdvQueries.Add(query);
        db.SaveChanges();

        var bobFile = new UploadedFile
        {
            OwnerId = "bob",
            Path = "demande-photo.jpg",
            ContentType = "image/jpeg",
            Length = 4096,
        };
        var aliceFile = new UploadedFile
        {
            OwnerId = "alice",
            Path = "provider-spec.pdf",
            ContentType = "application/pdf",
            Length = 1024,
        };
        db.UploadedFiles.AddRange(bobFile, aliceFile);
        db.SaveChanges();

        return (query.Id, bobFile.Id, aliceFile.Id);
    }

    private int CountAttachments(long queryId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.QueryAttachedFiles.Count(a => a.CommandId == queryId);
    }

    private string AttachUrl(long queryId) =>
        $"/api/v1/front/query/{queryId}/attachments?billingCode=Rdv";

    private string DetachUrl(long queryId, long fileId) =>
        $"/api/v1/front/query/{queryId}/attachments/{fileId}?billingCode=Rdv";

    [Fact]
    public async Task Client_attaching_own_file_creates_the_link()
    {
        var (queryId, bobFileId, _) = Seed();

        using var http = NewClient("bob");
        var response = await http.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");

        Assert.Equal(1, CountAttachments(queryId));

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var link = Assert.Single(db.QueryAttachedFiles.Where(a => a.CommandId == queryId));
        Assert.Equal(bobFileId, link.FileId);
    }

    [Fact]
    public async Task Provider_cannot_attach_to_a_query()
    {
        var (queryId, bobFileId, _) = Seed();

        // alice is the provider of the query; only the client attaches.
        using var http = NewClient("alice");
        var response = await http.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, CountAttachments(queryId));
    }

    [Fact]
    public async Task Client_cannot_attach_a_file_they_do_not_own()
    {
        var (queryId, _, aliceFileId) = Seed();

        // bob is the client, but the file belongs to alice.
        using var http = NewClient("bob");
        var response = await http.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, CountAttachments(queryId));
    }

    [Fact]
    public async Task Client_detaches_a_file()
    {
        var (queryId, bobFileId, _) = Seed();

        using var http = NewClient("bob");
        await http.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);
        Assert.Equal(1, CountAttachments(queryId));

        var response = await http.DeleteAsync(
            DetachUrl(queryId, bobFileId),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, CountAttachments(queryId));
    }

    [Fact]
    public async Task Provider_cannot_detach()
    {
        var (queryId, bobFileId, _) = Seed();

        using var bob = NewClient("bob");
        await bob.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);

        using var alice = NewClient("alice");
        var response = await alice.DeleteAsync(
            DetachUrl(queryId, bobFileId),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, CountAttachments(queryId));
    }

    [Fact]
    public async Task Provider_lists_attachments_as_a_party()
    {
        var (queryId, bobFileId, _) = Seed();

        using var bob = NewClient("bob");
        await bob.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);

        // alice is the provider: a party, may list.
        using var alice = NewClient("alice");
        var response = await alice.GetAsync(AttachUrl(queryId), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var item = Assert.Single(doc.RootElement.EnumerateArray());
        Assert.Equal(bobFileId, item.GetProperty("fileId").GetInt64());
        Assert.Equal("bob", item.GetProperty("ownerId").GetString());
    }

    [Fact]
    public async Task Third_party_cannot_list_attachments()
    {
        var (queryId, bobFileId, _) = Seed();

        using var bob = NewClient("bob");
        await bob.PostAsJsonAsync(
            AttachUrl(queryId),
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);

        using var carol = NewClient("carol");
        var response = await carol.GetAsync(AttachUrl(queryId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}