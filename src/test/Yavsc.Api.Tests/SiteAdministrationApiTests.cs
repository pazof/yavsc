using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Administration;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class SiteAdministrationApiTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private const string Root = "/api/v1/administration";

    private HttpClient Client(string? role = "Administrator")
    {
        var http = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
        { BaseAddress = new Uri(fixture.BaseAddress) };
        if (role is not null)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                TestTokenIssuer.Issue("site-admin", "api", extraClaims: [new Claim(Yavsc.Constants.RoleClaimType, role)]));
        return http;
    }

    [Fact]
    public async Task Administration_openapi_is_available_only_to_administrators()
    {
        using var http = Client();
        var document = await http.GetFromJsonAsync<JsonElement>("/openapi/administration.json");
        Assert.True(document.GetProperty("paths").TryGetProperty(Root + "/clients", out _));
        Assert.True(document.GetProperty("paths").TryGetProperty(Root + "/transfer", out _));
        using var moderator = Client("Moderator");
        Assert.Equal(HttpStatusCode.Forbidden, (await moderator.GetAsync("/openapi/administration.json")).StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Moderator", HttpStatusCode.Forbidden)]
    [InlineData("Performer", HttpStatusCode.Forbidden)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Every_administration_route_is_restricted(string? role, HttpStatusCode expected)
    {
        using var http = Client(role);
        var routes = new (HttpMethod Method, string Path)[]
        {
            (HttpMethod.Get, "summary"), (HttpMethod.Get, "roles"), (HttpMethod.Post, "roles/ensure"),
            (HttpMethod.Get, "roles/role/members"), (HttpMethod.Put, "roles/role/members/user"),
            (HttpMethod.Delete, "roles/role/members/user"), (HttpMethod.Get, "clients"),
            (HttpMethod.Get, "clients/1"), (HttpMethod.Post, "clients"), (HttpMethod.Put, "clients/1"),
            (HttpMethod.Delete, "clients/1"), (HttpMethod.Post, "clients/1/collections/Scopes"),
            (HttpMethod.Delete, "clients/1/collections/Scopes/1"), (HttpMethod.Post, "clients/1/secrets"),
            (HttpMethod.Post, "clients/1/secrets/regenerate"), (HttpMethod.Delete, "clients/1/secrets/1"),
            (HttpMethod.Get, "scopes"), (HttpMethod.Get, "scopes/1"), (HttpMethod.Post, "scopes"),
            (HttpMethod.Put, "scopes/1"), (HttpMethod.Delete, "scopes/1"),
            (HttpMethod.Get, "mailing-criteria"), (HttpMethod.Get, "mailing-templates"),
            (HttpMethod.Get, "mailing-templates/template"), (HttpMethod.Post, "mailing-templates"),
            (HttpMethod.Put, "mailing-templates/template"), (HttpMethod.Delete, "mailing-templates/template"),
            (HttpMethod.Get, "transfer"), (HttpMethod.Post, "transfer")
        };
        foreach (var route in routes)
        {
            using var request = new HttpRequestMessage(route.Method, $"{Root}/{route.Path}");
            using var response = await http.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(expected, response.StatusCode);
        }
    }

    [Fact]
    public async Task Client_scalars_collections_and_secrets_round_trip_without_exposing_stored_secrets()
    {
        using var http = Client();
        var settings = new ClientSettings { ClientId = "test-" + Guid.NewGuid().ToString("N"), ClientName = "Test client",
            RequirePkce = true, AllowOfflineAccess = true, AccessTokenLifetime = 1200, UserSsoLifetime = 600 };
        var created = await http.PostAsJsonAsync($"{Root}/clients", settings);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var client = (await created.Content.ReadFromJsonAsync<AdministrationClient>())!;
        Assert.Equal(settings, client.Settings);
        var id = client.Id;
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync($"{Root}/clients", settings)).StatusCode);
        var edited = settings with { Description = "Edited", RequireConsent = true, ClientClaimsPrefix = "app_" };
        Assert.Equal(HttpStatusCode.OK, (await http.PutAsJsonAsync($"{Root}/clients/{id}", edited)).StatusCode);
        Assert.Equal(edited, (await http.GetFromJsonAsync<AdministrationClient>($"{Root}/clients/{id}"))!.Settings);

        foreach (var collection in Enum.GetValues<ClientCollection>())
        {
            var value = collection switch
            {
                ClientCollection.RedirectUris or ClientCollection.PostLogoutRedirectUris => "https://example.invalid/callback",
                ClientCollection.CorsOrigins => "https://example.invalid",
                _ => "test-value"
            };
            var entry = new ClientEntry { Key = collection is ClientCollection.Claims or ClientCollection.Properties ? "key" : null, Value = value };
            var response = await http.PostAsJsonAsync($"{Root}/clients/{id}/collections/{collection}", entry);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict,
                (await http.PostAsJsonAsync($"{Root}/clients/{id}/collections/{collection}", entry)).StatusCode);
            client = (await http.GetFromJsonAsync<AdministrationClient>($"{Root}/clients/{id}"))!;
            var row = Assert.Single(client.Entries, row => row.Collection == collection && row.Value == value);
            Assert.Equal(HttpStatusCode.NotFound, (await http.DeleteAsync($"{Root}/clients/{id + 99999}/collections/{collection}/{row.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/clients/{id}/collections/{collection}/{row.Id}")).StatusCode);
        }

        var secretResponse = await http.PostAsJsonAsync($"{Root}/clients/{id}/secrets",
            new ClientSecretRequest { Description = "Added by integration test", Expiration = DateTimeOffset.UtcNow.AddDays(1) });
        Assert.Equal(HttpStatusCode.Created, secretResponse.StatusCode);
        Assert.True(secretResponse.Headers.CacheControl?.NoStore);
        var secret = (await secretResponse.Content.ReadFromJsonAsync<NewClientSecret>())!;
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stored = await db.ClientSecrets.SingleAsync(row => row.Id == secret.Id);
            Assert.Equal(Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret.Value))), stored.Value);
            var payload = await http.GetStringAsync($"{Root}/clients/{id}");
            Assert.DoesNotContain(secret.Value, payload);
            Assert.DoesNotContain(stored.Value, payload);
        }
        var replacementResponse = await http.PostAsJsonAsync($"{Root}/clients/{id}/secrets/regenerate", new ClientSecretRequest());
        Assert.Equal(HttpStatusCode.OK, replacementResponse.StatusCode);
        var replacement = (await replacementResponse.Content.ReadFromJsonAsync<NewClientSecret>())!;
        client = (await http.GetFromJsonAsync<AdministrationClient>($"{Root}/clients/{id}"))!;
        Assert.Equal(replacement.Id, Assert.Single(client.Secrets).Id);
        Assert.NotEqual(secret.Value, replacement.Value);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/clients/{id}/secrets/{replacement.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/clients/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync($"{Root}/clients/{id}")).StatusCode);
    }

    [Fact]
    public async Task Scopes_and_mailing_templates_support_create_update_delete_and_validation()
    {
        using var http = Client();
        var scopeSettings = new ApiScopeSettings { Name = "scope-" + Guid.NewGuid().ToString("N"), DisplayName = "Test" };
        var response = await http.PostAsJsonAsync($"{Root}/scopes", scopeSettings);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var scope = (await response.Content.ReadFromJsonAsync<AdministrationScope>())!;
        var edited = scopeSettings with { Description = "Updated", Enabled = false, Required = true, Emphasize = true };
        Assert.Equal(HttpStatusCode.OK, (await http.PutAsJsonAsync($"{Root}/scopes/{scope.Id}", edited)).StatusCode);
        Assert.Equal(edited, (await http.GetFromJsonAsync<AdministrationScope>($"{Root}/scopes/{scope.Id}"))!.Settings);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync($"{Root}/scopes", edited)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/scopes/{scope.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync($"{Root}/scopes/{scope.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync($"{Root}/scopes", new ApiScopeSettings())).StatusCode);

        var criteria = (await http.GetFromJsonAsync<List<string>>($"{Root}/mailing-criteria"))!;
        Assert.NotEmpty(criteria);
        var template = new MailingTemplateSettings { Id = criteria[0], Topic = "Test topic", Body = "Test body",
            ReplyToAddress = "test@example.invalid", ToSend = MailPeriodicity.Weekly };
        response = await http.PostAsJsonAsync($"{Root}/mailing-templates", template);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var escapedId = Uri.EscapeDataString(template.Id);
        var updated = template with { Topic = "Changed topic", ToSend = MailPeriodicity.Monthly };
        Assert.Equal(HttpStatusCode.OK, (await http.PutAsJsonAsync($"{Root}/mailing-templates/{escapedId}", updated)).StatusCode);
        Assert.Equal(updated, await http.GetFromJsonAsync<MailingTemplateSettings>($"{Root}/mailing-templates/{escapedId}"));
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync($"{Root}/mailing-templates", template)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/mailing-templates/{escapedId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync($"{Root}/mailing-templates/{escapedId}")).StatusCode);
    }

    [Fact]
    public async Task Roles_support_membership_and_protect_the_last_administrator()
    {
        using var http = Client();
        Assert.Equal(HttpStatusCode.NoContent, (await http.PostAsync($"{Root}/roles/ensure", null)).StatusCode);
        var roles = (await http.GetFromJsonAsync<List<AdministrationRole>>($"{Root}/roles"))!;
        Assert.Contains(roles, role => role.Name == "Administrator");
        Assert.Contains(roles, role => role.Name == "Moderator");
        var role = roles.Single(role => role.Name == "Moderator");
        var adminRole = roles.Single(role => role.Name == "Administrator");
        using var scope = fixture.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "role-" + Guid.NewGuid().ToString("N"),
            Email = Guid.NewGuid().ToString("N") + "@example.invalid" };
        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.Equal(HttpStatusCode.NoContent, (await http.PutAsync($"{Root}/roles/{role.Id}/members/{user.Id}", null)).StatusCode);
        var members = (await http.GetFromJsonAsync<List<RoleMember>>($"{Root}/roles/{role.Id}/members"))!;
        Assert.Contains(members, member => member.Id == user.Id);
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"{Root}/roles/{role.Id}/members/{user.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await http.PutAsync($"{Root}/roles/{adminRole.Id}/members/{user.Id}", null)).StatusCode);
        try
        {
            Assert.Equal(HttpStatusCode.Conflict, (await http.DeleteAsync($"{Root}/roles/{adminRole.Id}/members/{user.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await http.PutAsync($"{Root}/roles/missing/members/{user.Id}", null)).StatusCode);
        }
        finally { await manager.RemoveFromRoleAsync(user, "Administrator"); }
        Assert.NotNull(await http.GetFromJsonAsync<AdministrationSummary>($"{Root}/summary"));
    }

    [Fact]
    public async Task Business_transfer_is_persistent_atomic_and_excludes_authentication_material()
    {
        using var http = Client();
        var id = Guid.NewGuid().ToString("N");
        var user = new UserBusinessData { Id = id, UserName = "transfer-" + id, FullName = "Transfer user",
            Email = id + "@example.invalid" };
        var parentCode = "parent-" + id;
        var childCode = "child-" + id;
        var tagName = "transfer-" + id;
        long expectedTagId;
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var firstTag = new Tag { Name = tagName };
            var secondTag = new Tag { Name = tagName };
            db.Tags.AddRange(firstTag, secondTag);
            await db.SaveChangesAsync();
            expectedTagId = Math.Min(firstTag.Id, secondTag.Id);
        }
        var transfer = new AdministrationTransfer
        {
            Users = [user], Activities = [
                new ActivityBusinessData { Code = childCode, Name = "Child", ParentCode = parentCode },
                new ActivityBusinessData { Code = parentCode, Name = "Parent" }],
            Blogs = [new BlogBusinessData { Id = 7654321, AuthorId = id, Title = "Transferred post",
                DateCreated = DateTimeOffset.UtcNow, Published = true, Tags = [tagName] }]
        };
        var response = await http.PostAsJsonAsync($"{Root}/transfer", transfer);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new ImportResult(1, 1, 2), await response.Content.ReadFromJsonAsync<ImportResult>());
        response = await http.PostAsJsonAsync($"{Root}/transfer", transfer);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var saved = await db.Users.SingleAsync(row => row.Id == id);
            Assert.Null(saved.PasswordHash);
            Assert.False(saved.EmailConfirmed);
            Assert.Empty(await db.UserRoles.Where(row => row.UserId == id).ToListAsync());
            Assert.Equal(parentCode, (await db.Activities.SingleAsync(row => row.Code == childCode)).ParentCode);
            Assert.True(await db.blogSpotPublications.AnyAsync(row => row.PostId == 7654321));
            var post = await db.BlogSpot.Include(row => row.Tags).ThenInclude(row => row.Tag)
                .SingleAsync(row => row.Id == 7654321);
            Assert.Equal(expectedTagId, Assert.Single(post.Tags).Tag.Id);
            Assert.Equal(2, await db.Tags.CountAsync(row => row.Name == tagName));
        }
        var exported = await http.GetStringAsync($"{Root}/transfer");
        using (var json = JsonDocument.Parse(exported))
        {
            var account = json.RootElement.GetProperty("users").EnumerateArray().Single(row => row.GetProperty("id").GetString() == id);
            Assert.False(account.TryGetProperty("passwordHash", out _));
            Assert.False(account.TryGetProperty("securityStamp", out _));
            Assert.False(account.TryGetProperty("roles", out _));
        }
        var invalid = transfer with
        {
            Users = [user with { FullName = "Must roll back" }],
            Blogs = [transfer.Blogs[0] with { AuthorId = "missing-author" }]
        };
        response = await http.PostAsJsonAsync($"{Root}/transfer", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using (var scope = fixture.Services.CreateScope())
            Assert.Equal("Transfer user", (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                .Users.SingleAsync(row => row.Id == id)).FullName);

        var cycle = new AdministrationTransfer
        {
            Activities = [new ActivityBusinessData { Code = "cycle-" + id, Name = "Cycle", ParentCode = "cycle-" + id }]
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync($"{Root}/transfer", cycle)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await http.PostAsJsonAsync($"{Root}/transfer", transfer with { Users = [user, user] })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await http.PostAsJsonAsync($"{Root}/transfer", transfer with { Blogs = [transfer.Blogs[0] with { CircleIds = [long.MaxValue] }] })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await http.PostAsJsonAsync($"{Root}/transfer", new { unrelated = "not a transfer" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,
            (await http.PostAsJsonAsync($"{Root}/transfer", transfer with
                { Users = [user with { Email = "changed-" + id + "@example.invalid" }] })).StatusCode);
    }

    [Fact]
    public async Task Account_import_never_changes_credentials_confirmations_or_roles()
    {
        using var http = Client();
        var id = Guid.NewGuid().ToString("N");
        using var scope = fixture.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { Id = id, UserName = "safe-" + id, Email = id + "@example.invalid" };
        Assert.True((await manager.CreateAsync(user, "A1!" + Guid.NewGuid().ToString("N"))).Succeeded);
        var hash = user.PasswordHash;
        var stamp = user.SecurityStamp;
        var response = await http.PostAsJsonAsync($"{Root}/transfer", new
        {
            version = 1,
            users = new[] { new { id, userName = user.UserName, email = user.Email, fullName = "Business update",
                passwordHash = "must-not-be-imported", securityStamp = "must-not-be-imported",
                emailConfirmed = true, roles = new[] { "Administrator" } } },
            blogs = Array.Empty<object>(), activities = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var verification = fixture.Services.CreateScope();
        var db = verification.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var saved = await db.Users.SingleAsync(row => row.Id == id);
        Assert.Equal("Business update", saved.FullName);
        Assert.Equal(hash, saved.PasswordHash);
        Assert.Equal(stamp, saved.SecurityStamp);
        Assert.False(saved.EmailConfirmed);
        Assert.False(await db.UserRoles.AnyAsync(row => row.UserId == id));
    }
}
