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

    // Repro: GET /Client/EditRedirectUris/{id} après qu'une seconde
    // RedirectUri a été ajoutée via POST. LoadClientAsync(id) réhydrate
    // le Client avec 9 Includes (RedirectUris, AllowedScopes, ClientSecrets,
    // etc.) via l'InMemory provider. Sur l'Id=2 seedé, la matérialisation
    // des nav properties sur les entités IdentityServer8 lève
    // IndexOutOfRangeException — l'action renvoie un 500 et la page
    // d'erreur masque le diagnostic.
    //
    // Ce test reproduit le chemin qui plante : GET initial (lit l'AF
    // token) → POST AddRedirectUri → GET final qui exerce LoadClientAsync
    // avec une collection RedirectUris non triviale (2 entrées).
    // Il doit ÉCHOUER tant que le bug n'est pas traité.
    [Fact]
    public async Task EditRedirectUris_GET_after_add_lists_both_uris()
    {
        var http = CreateAdminClient();
        var id = TargetClientDbId();
        const string newUri = "https://app.example.com/cb-repro";

        // Premier GET pour récupérer l'antiforgery token du form Add.
        var pageResp = await http.GetAsync(
            $"/Client/EditRedirectUris/{id}", TestContext.Current.CancellationToken);
        pageResp.EnsureSuccessStatusCode();
        var token = ExtractAntiforgeryToken(
            await pageResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.False(string.IsNullOrEmpty(token));

        // Ajout d'une seconde RedirectUri — c'est l'état non-trivial
        // qui déclenche la matérialisation problématique côté
        // InMemory provider.
        var form = new MultipartFormDataContent
        {
            { new StringContent(newUri), "redirectUri" },
            { new StringContent(token!), "__RequestVerificationToken" },
        };
        var addResp = await http.PostAsync(
            $"/Client/AddRedirectUri/{id}", form, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);

        try
        {
            // GET final : exerce LoadClientAsync(id) sur le client
            // avec 2 RedirectUris, 1 AllowedScope, 1 GrantType, etc.
            // Si la matérialisation échoue (IndexOutOfRange), ce GET
            // renvoie 500 et EnsureSuccessStatusCode fait échouer le test.
            var finalResp = await http.GetAsync(
                $"/Client/EditRedirectUris/{id}", TestContext.Current.CancellationToken);
            finalResp.EnsureSuccessStatusCode();
            var finalBody = await finalResp.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            // La page doit lister les DEUX URIs.
            Assert.Contains("https://app.example.com/cb", finalBody);
            Assert.Contains(newUri, finalBody);
        }
        finally
        {
            // Cleanup idempotent : on retire l'URI ajoutée pour ne
            // pas polluer les autres tests de la collection.
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var row = db.ClientRedirectUris
                .FirstOrDefault(r => r.RedirectUri == newUri);
            if (row is not null)
            {
                db.ClientRedirectUris.Remove(row);
                db.SaveChanges();
            }
        }
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

    // ---------- Bisection de l'Include qui plante ----------
    //
    // LoadClientAsync charge le Client avec 9 Includes :
    //   RedirectUris, PostLogoutRedirectUris, AllowedScopes,
    //   AllowedGrantTypes, AllowedCorsOrigins,
    //   IdentityProviderRestrictions, Claims, Properties, ClientSecrets.
    //
    // Le test EditRedirectUris_GET_after_add_lists_both_uris échoue
    // avec IndexOutOfRangeException au shaper de l'InMemory provider,
    // sans préciser lequel des Includes pose problème. La stack
    // indique IncludeCollection (donc un Include de collection, pas
    // de référence).
    //
    // Ces tests exécutent la même query EF qu'un Include à la fois,
    // sur le client cible déjà seedé par EnsureTargetClient, pour
    // isoler le coupable. Tant que l'InMemory provider est
    // incapable de matérialiser la nav, le test correspondant est
    // rouge. Quand on a l'Include, on peut soit le charger autrement
    // (AsSplitQuery, projection, etc.), soit basculer la fixture
    // vers SQLite in-memory (cf. la discussion laissée ouverte
    // 2026-07-11 sur fix/issue-3-splitquery).
    private async Task<int> BisectClientDbId()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return db.Clients.Single(c => c.ClientId == TargetClientId).Id;
    }

    [Fact]
    public async Task Bisect_RedirectUris_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.RedirectUris)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.RedirectUris);
    }

    [Fact]
    public async Task Bisect_PostLogoutRedirectUris_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.PostLogoutRedirectUris)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.PostLogoutRedirectUris);
    }

    [Fact]
    public async Task Bisect_AllowedScopes_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.AllowedScopes)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.AllowedScopes);
    }

    [Fact]
    public async Task Bisect_AllowedGrantTypes_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.AllowedGrantTypes)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.AllowedGrantTypes);
    }

    [Fact]
    public async Task Bisect_AllowedCorsOrigins_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.AllowedCorsOrigins)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.AllowedCorsOrigins);
    }

    [Fact]
    public async Task Bisect_IdentityProviderRestrictions_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.IdentityProviderRestrictions)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.IdentityProviderRestrictions);
    }

    [Fact]
    public async Task Bisect_Claims_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.Claims)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.Claims);
    }

    [Fact]
    public async Task Bisect_Properties_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.Properties)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.Properties);
    }

    [Fact]
    public async Task Bisect_ClientSecrets_alone()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .Include(c => c.ClientSecrets)
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
        Assert.NotNull(client!.ClientSecrets);
    }

    [Fact]
    public async Task Bisect_baseline_no_include()
    {
        // Sanity check : la query de base (sans Include) doit passer.
        // Si celle-ci échoue, le problème n'est pas un Include.
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = await BisectClientDbId();
        var client = await db.Clients
            .SingleOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);
        Assert.NotNull(client);
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
