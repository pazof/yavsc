using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
[Collection("Yavsc Api")]
public sealed class PerformerDonationSettingsTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private const string Endpoint = "/api/v1/account/performer-profile";
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient Client(string? subject, string scope = "api")
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }) { BaseAddress = new Uri(fixture.BaseAddress) };
        if (subject != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                TestTokenIssuer.Issue(subject, scope, extraClaims: [new Claim(Yavsc.Constants.RoleClaimType, "Performer")]));
        return client;
    }

    private async Task<string> CreatePerformerAsync(bool withProfile = true)
    {
        var id = "donation-settings-" + Guid.NewGuid().ToString("N");
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser { Id = id, UserName = id, Email = id + "@example.test" });
        if (withProfile) db.Performers.Add(new PerformerProfile
        {
            PerformerId = id, SIREN = "123456789", WebSite = "", Active = true,
            DonationPayPalEmail = "initial@payments.example",
            OrganizationAddress = new Location { Address = "Test" }
        });
        await db.SaveChangesAsync(Ct);
        return id;
    }

    [Fact]
    public async Task New_performer_can_set_a_recipient_when_creating_their_profile()
    {
        var id = await CreatePerformerAsync(withProfile: false);
        using var client = Client(id);
        var profile = await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
        Assert.NotNull(profile);
        Assert.Equal("", profile.DonationPayPalEmail);
        Assert.False(profile.CanIssueEstimates);
        profile.DonationPayPalEmail = "new@payments.example";
        using var response = await client.PutAsJsonAsync(Endpoint, profile, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
        Assert.Equal("new@payments.example", saved?.DonationPayPalEmail);
        Assert.False(saved?.CanIssueEstimates);
    }

    [Theory]
    [InlineData("  new@payments.example  ", "new@payments.example")]
    [InlineData("   ", "")]
    public async Task Recipient_is_trimmed_and_whitespace_disables_donations(string input, string expected)
    {
        var id = await CreatePerformerAsync();
        using var client = Client(id);
        using var response = await client.PutAsJsonAsync(Endpoint, new { donationPayPalEmail = input }, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected,
            (await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct))?.DonationPayPalEmail);
    }

    [Fact]
    public async Task Performer_can_read_update_and_disable_their_paypal_recipient_across_devices()
    {
        var id = await CreatePerformerAsync();
        using var client = Client(id);
        var profile = await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
        Assert.NotNull(profile);
        Assert.Equal("initial@payments.example", profile.DonationPayPalEmail);
        using var read = await client.GetAsync(Endpoint, Ct);
        Assert.True(read.Headers.CacheControl?.NoStore);
        foreach (var email in new[] { "new@payments.example", "" })
        {
            profile.DonationPayPalEmail = email;
            using var response = await client.PutAsJsonAsync(Endpoint, profile, Ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var otherDevice = Client(id);
            var reloaded = await otherDevice.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
            Assert.NotNull(reloaded);
            Assert.Equal(email, reloaded.DonationPayPalEmail);
            using var scope = fixture.Services.CreateScope();
            var stored = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                .Performers.AsNoTracking().SingleAsync(p => p.PerformerId == id, Ct);
            Assert.Equal(email.Length == 0 ? null : email, stored.DonationPayPalEmail);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Older_clients_omitting_or_sending_null_preserve_the_paypal_recipient(bool explicitNull)
    {
        var id = await CreatePerformerAsync();
        using var client = Client(id);
        using var response = explicitNull
            ? await client.PutAsJsonAsync(Endpoint, new { performerId = id, donationPayPalEmail = (string?)null }, Ct)
            : await client.PutAsJsonAsync(Endpoint, new { performerId = id }, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reloaded = await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
        Assert.Equal("initial@payments.example", reloaded?.DonationPayPalEmail);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("https://payments.example/donate")]
    [InlineData("a@b@c.example")]
    public async Task Invalid_recipient_returns_400_without_changing_the_profile(string email)
    {
        var id = await CreatePerformerAsync();
        using var client = Client(id);
        using var response = await client.PutAsJsonAsync(Endpoint,
            new { performerId = id, donationPayPalEmail = email, webSite = "must-not-be-saved" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var profile = await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct);
        Assert.Equal("initial@payments.example", profile?.DonationPayPalEmail);
        Assert.Equal("", profile?.WebSite);
    }

    [Fact]
    public async Task Too_long_recipient_returns_400_without_saving()
    {
        var id = await CreatePerformerAsync();
        using var client = Client(id);
        using var response = await client.PutAsJsonAsync(Endpoint,
            new { donationPayPalEmail = new string('a', 128) + "@example.test" }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("initial@payments.example",
            (await client.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct))?.DonationPayPalEmail);
    }

    [Fact]
    public async Task Performer_cannot_write_another_accounts_recipient()
    {
        var owner = await CreatePerformerAsync();
        var other = await CreatePerformerAsync();
        using var client = Client(owner);
        using var response = await client.PutAsJsonAsync(Endpoint,
            new { performerId = other, donationPayPalEmail = "attacker@payments.example" }, Ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var otherClient = Client(other);
        Assert.Equal("initial@payments.example",
            (await otherClient.GetFromJsonAsync<PerformerProfileSettings>(Endpoint, Ct))?.DonationPayPalEmail);
    }

    [Theory]
    [InlineData(null, "api", HttpStatusCode.Unauthorized)]
    [InlineData("anyone", "profile", HttpStatusCode.Forbidden)]
    public async Task Read_and_write_require_authentication_and_api_scope(string? subject, string scope, HttpStatusCode expected)
    {
        using var client = Client(subject, scope);
        using var read = await client.GetAsync(Endpoint, Ct);
        using var write = await client.PutAsJsonAsync(Endpoint, new { donationPayPalEmail = "test@payments.example" }, Ct);
        Assert.Equal(expected, read.StatusCode);
        Assert.Equal(expected, write.StatusCode);
    }
}
