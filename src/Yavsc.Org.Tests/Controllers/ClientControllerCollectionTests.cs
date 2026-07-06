using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityServer8.EntityFramework.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Yavsc.Models;
using Yavsc.Tests.Shared;

namespace Yavsc.Org.Tests.Controllers;

/// <summary>
/// Integration tests for the per-collection edit pages of the OAuth2
/// client editor. Hits the real ASP.NET pipeline against
/// <see cref="WebServerFixture"/>: the WebApplication is built once per
/// collection and shared, so these tests must be defensive about row
/// IDs (they pick their own dedicated client to mutate).
///
/// Admin auth is satisfied by sending an <c>X-Test-Role: Administrator</c>
/// header. The <see cref="TestAuthPolicyProvider"/> short-circuits the
/// production policy to honour that header, sidestepping the login flow
/// (which is itself not exercised by these tests).
/// </summary>
[Collection("Yavsc Server")]
public class ClientControllerCollectionTests : IClassFixture<TestWebApplicationFactory>
{
    private const string TargetClientId = "collection-tests-target";

    private readonly TestWebApplicationFactory _factory;

    public ClientControllerCollectionTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        EnsureTargetClient();
    }

    private void EnsureTargetClient()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Clients.Any(c => c.ClientId == TargetClientId)) return;

        db.Clients.Add(new Client
        {
            ClientId = TargetClientId,
            ClientName = "Collection-edit tests target",
            Enabled = true,
            RequireClientSecret = false,
            RequirePkce = true,
            ProtocolType = "oidc",
            AllowedGrantTypes = new List<ClientGrantType>
            {
                new() { GrantType = "authorization_code" },
            },
            AllowedScopes = new List<ClientScope>
            {
                new() { Scope = "openid" },
            },
            RedirectUris = new List<ClientRedirectUri>
            {
                new() { RedirectUri = "https://app.example.com/cb" },
            },
        });
        db.SaveChanges();
    }

    private int TargetClientDbId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.Clients.Single(c => c.ClientId == TargetClientId).Id;
    }

    private HttpClient CreateAdminClient()
    {
        // WebApplicationFactory.CreateClient() returns an HttpClient wired
        // directly to the in-memory test server — no Kestrel socket, no
        // self-signed cert, no IServerAddressesFeature lookup. Keep
        // cookies enabled so the antiforgery cookie set on the GET that
        // fetches the form is replayed on the POST that submits it.
        var http = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });
        http.DefaultRequestHeaders.Add(TestAuthPolicyProvider.HeaderName, TestAuthPolicyProvider.AdminRole);
        return http;
    }

    [Fact]
    public async Task Edit_GET_returns_200_for_admin()
    {
        var http = CreateAdminClient();
        var id = TargetClientDbId();
        var resp = await http.GetAsync($"/Client/Edit/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(TargetClientId, body);
    }

    [Fact]
    public async Task EditRedirectUris_GET_returns_200_and_lists_seeded_uri()
    {
        var http = CreateAdminClient();
        var id = TargetClientDbId();
        var resp = await http.GetAsync($"/Client/EditRedirectUris/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("https://app.example.com/cb", body);
    }

    [Fact]
    public async Task AddRedirectUri_POST_appends_to_database()
    {
        var http = CreateAdminClient();
        var id = TargetClientDbId();
        const string newUri = "https://app.example.com/cb2";

        // Read the GET page first to grab the antiforgery token attached
        // to the Add form on EditRedirectUris.
        var pageResp = await http.GetAsync($"/Client/EditRedirectUris/{id}", TestContext.Current.CancellationToken);
        pageResp.EnsureSuccessStatusCode();
        var pageHtml = await pageResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var token = ExtractAntiforgeryToken(pageHtml);
        Assert.False(string.IsNullOrEmpty(token));

        var form = new MultipartFormDataContent
        {
            { new StringContent(newUri), "redirectUri" },
            { new StringContent(token!), "__RequestVerificationToken" },
        };
        var postResp = await http.PostAsync($"/Client/AddRedirectUri/{id}", form, TestContext.Current.CancellationToken);
        var postBody = await postResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(postResp.StatusCode == HttpStatusCode.OK,
            $"Expected 200, got {(int)postResp.StatusCode}. Body[0..500]: {postBody.Substring(0, Math.Min(500, postBody.Length))}");
        Assert.Equal(HttpStatusCode.OK, postResp.StatusCode);

        // Verify in DB.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = db.Clients
            .Include(c => c.RedirectUris)
            .Single(c => c.ClientId == TargetClientId);
        Assert.Contains(client.RedirectUris, r => r.RedirectUri == newUri);

        // Cleanup so the test is idempotent across runs.
        var toRemove = client.RedirectUris.First(r => r.RedirectUri == newUri);
        db.ClientRedirectUris.Remove(toRemove);
        db.SaveChanges();
    }

    [Fact]
    public async Task RemoveRedirectUri_POST_with_foreign_rowId_returns_NotFound()
    {
        // SECURITY: a Remove call for a row that belongs to a different
        // client must be rejected. The test creates a second client,
        // gets a real rowId from it, then calls Remove on the target
        // client with that foreign rowId. Expected: 404, target
        // client unchanged.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (!db.Clients.Any(c => c.ClientId == "collection-tests-other"))
            {
                db.Clients.Add(new Client
                {
                    ClientId = "collection-tests-other",
                    ClientName = "Other target",
                    Enabled = true,
                    ProtocolType = "oidc",
                    RequireClientSecret = false,
                    RequirePkce = true,
                });
                db.SaveChanges();
            }
        }

        int otherRowId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var other = db.Clients.Include(c => c.RedirectUris)
                .Single(c => c.ClientId == "collection-tests-other");
            if (!other.RedirectUris.Any())
            {
                other.RedirectUris.Add(new ClientRedirectUri { RedirectUri = "https://other.example/cb" });
                db.SaveChanges();
            }
            otherRowId = other.RedirectUris.First().Id;
        }

        var http = CreateAdminClient();
        var targetId = TargetClientDbId();
        var pageResp = await http.GetAsync($"/Client/EditRedirectUris/{targetId}", TestContext.Current.CancellationToken);
        var token = ExtractAntiforgeryToken(await pageResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var form = new MultipartFormDataContent
        {
            { new StringContent(targetId.ToString()), "id" },
            { new StringContent(otherRowId.ToString()), "rowId" },
            { new StringContent(token!), "__RequestVerificationToken" },
        };
        var resp = await http.PostAsync($"/Client/RemoveRedirectUri", form,
        TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    private static string? ExtractAntiforgeryToken(string html)
    {
        const string marker = "name=\"__RequestVerificationToken\"";
        var idx = html.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return null;
        var valueStart = html.IndexOf("value=\"", idx, StringComparison.Ordinal);
        if (valueStart < 0) return null;
        valueStart += "value=\"".Length;
        var valueEnd = html.IndexOf('"', valueStart);
        return valueEnd < 0 ? null : html[valueStart..valueEnd];
    }
}
