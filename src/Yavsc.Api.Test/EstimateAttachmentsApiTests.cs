using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

/// <summary>
/// Integration tests for the estimate attachment endpoints
/// (<c>GET/POST/DELETE api/v1/estimate/{id}/attachments</c>), which
/// attach a file from the provider's personal storage to an estimate
/// **by reference** (<see cref="EstimateAttachedFile"/>).
///
/// <para>Coverage mirrors the authorization contract documented on the
/// controller: only the provider (<c>estimate.OwnerId</c>) may attach or
/// detach, the file must belong to the caller, and listing is open to
/// both parties (provider/client) plus admin. A third party is refused.
/// The download of the bytes themselves is served by the Blogs host and
/// exercised in <c>Yavsc.Blogs.Tests</c>; these tests cover only the
/// metadata/relationship endpoints that live on the API host.</para>
/// </summary>
[Collection("Yavsc Api")]
public sealed class EstimateAttachmentsApiTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public EstimateAttachmentsApiTests(ApiWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient NewClient(string subject = "alice", string scope = "api")
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
    /// Reset the graph (seeding alice the provider and bob the client),
    /// create an estimate owned by alice for bob, and seed two personal
    /// <see cref="UploadedFile"/> rows: one owned by alice (the provider's
    /// own file) and one owned by bob (a file that does NOT belong to the
    /// provider, used to assert the ownership guard on attach).
    /// </summary>
    private (long estimateId, long aliceFileId, long bobFileId) Seed()
    {
        _fixture.ResetAndSeedActivityGraph();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var estimate = new Estimate
        {
            ClientId = "bob",
            OwnerId = "alice",
            CommandType = BillingCodes.Rdv,
            Title = "Devis prestation",
            Description = "Devis de test",
            AttachedFiles = new List<string>(),
            AttachedGraphics = new List<string>(),
            Bill = new List<CommandLine>
            {
                new() { Name = "Prestation", Description = "Prestation de base", Count = 1, UnitaryCost = 120m, Currency = "EUR" },
            },
        };
        db.Estimates.Add(estimate);
        db.SaveChanges();

        var aliceFile = new UploadedFile
        {
            OwnerId = "alice",
            Path = "provider-doc.pdf",
            ContentType = "application/pdf",
            Length = 2048,
        };
        var bobFile = new UploadedFile
        {
            OwnerId = "bob",
            Path = "client-note.txt",
            ContentType = "text/plain",
            Length = 64,
        };
        db.UploadedFiles.AddRange(aliceFile, bobFile);
        db.SaveChanges();

        return (estimate.Id, aliceFile.Id, bobFile.Id);
    }

    private int CountAttachments(long estimateId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.EstimateAttachedFiles.Count(a => a.EstimateId == estimateId);
    }

    [Fact]
    public async Task Provider_attaching_own_file_creates_the_link()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var http = NewClient("alice");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");

        Assert.Equal(1, CountAttachments(estimateId));

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var link = Assert.Single(db.EstimateAttachedFiles.Where(a => a.EstimateId == estimateId));
        Assert.Equal(aliceFileId, link.FileId);
    }

    [Fact]
    public async Task Attaching_the_same_file_twice_is_idempotent()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var http = NewClient("alice");
        await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);
        var second = await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(1, CountAttachments(estimateId));
    }

    [Fact]
    public async Task Non_provider_cannot_attach_to_an_estimate()
    {
        var (estimateId, aliceFileId, _) = Seed();

        // bob is the client of the estimate, not the provider.
        using var http = NewClient("bob");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, CountAttachments(estimateId));
    }

    [Fact]
    public async Task Provider_cannot_attach_a_file_they_do_not_own()
    {
        var (estimateId, _, bobFileId) = Seed();

        // alice is the provider, but the file belongs to bob.
        using var http = NewClient("alice");
        var response = await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = bobFileId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, CountAttachments(estimateId));
    }

    [Fact]
    public async Task Provider_detaches_a_file()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var http = NewClient("alice");
        await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);
        Assert.Equal(1, CountAttachments(estimateId));

        var response = await http.DeleteAsync(
            $"/api/v1/estimate/{estimateId}/attachments/{aliceFileId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, CountAttachments(estimateId));
    }

    [Fact]
    public async Task Non_provider_cannot_detach()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var http = NewClient("alice");
        await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        using var bob = NewClient("bob");
        var response = await bob.DeleteAsync(
            $"/api/v1/estimate/{estimateId}/attachments/{aliceFileId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, CountAttachments(estimateId));
    }

    [Fact]
    public async Task Provider_lists_attachments()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var http = NewClient("alice");
        await http.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        var response = await http.GetAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var item = Assert.Single(doc.RootElement.EnumerateArray());
        Assert.Equal(aliceFileId, item.GetProperty("fileId").GetInt64());
        Assert.Equal("provider-doc.pdf", item.GetProperty("path").GetString());
        Assert.Equal("alice", item.GetProperty("ownerId").GetString());
    }

    [Fact]
    public async Task Client_can_list_attachments_as_a_party()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var alice = NewClient("alice");
        await alice.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        // bob is the client of the estimate: a party, may list.
        using var bob = NewClient("bob");
        var response = await bob.GetAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Third_party_cannot_list_attachments()
    {
        var (estimateId, aliceFileId, _) = Seed();

        using var alice = NewClient("alice");
        await alice.PostAsJsonAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            new { fileId = aliceFileId },
            TestContext.Current.CancellationToken);

        // carol is neither the provider nor the client.
        using var carol = NewClient("carol");
        var response = await carol.GetAsync(
            $"/api/v1/estimate/{estimateId}/attachments",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Listing_an_unknown_estimate_returns_404()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient("alice");

        var response = await http.GetAsync(
            "/api/v1/estimate/999999/attachments",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}