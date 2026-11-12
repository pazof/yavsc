using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Tests.Shared;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
[Collection("Yavsc Api")]
public sealed class AccountCallPreferenceTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private const string Endpoint = "/api/v1/account/me";

    private HttpClient Client(string? subject, string scope = "api")
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }) { BaseAddress = new Uri(fixture.BaseAddress) };
        if (subject is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, scope));
        }
        return client;
    }

    private async Task<string> CreateUserAsync()
    {
        var id = "call-preference-" + Guid.NewGuid().ToString("N");
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser { Id = id, UserName = id, Email = $"{id}@example.test" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return id;
    }

    [Fact]
    public void Preference_is_not_exposed_when_a_user_entity_is_serialized()
    {
        var user = new ApplicationUser { HideIpAddressInCalls = true };
        Assert.DoesNotContain(nameof(ApplicationUser.HideIpAddressInCalls), JsonSerializer.Serialize(user));
        Assert.DoesNotContain(nameof(ApplicationUser.HideIpAddressInCalls), Newtonsoft.Json.JsonConvert.SerializeObject(user));
    }

    [Fact]
    public async Task Preference_defaults_to_false_and_round_trips_across_clients_and_database_contexts()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await CreateUserAsync();
        using var firstDevice = Client(id);
        var initial = await firstDevice.GetFromJsonAsync<Me>(Endpoint, ct);
        Assert.NotNull(initial);
        Assert.False(initial.HideIpAddressInCalls);

        foreach (var value in new[] { true, false })
        {
            using var response = await firstDevice.PutAsJsonAsync(Endpoint,
                new ProfileUpdateRequest { HideIpAddressInCalls = value }, ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var saved = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            Assert.Equal(value, saved.GetProperty("hideIpAddressInCalls").GetBoolean());

            using var secondDevice = Client(id);
            var profile = await secondDevice.GetFromJsonAsync<Me>(Endpoint, ct);
            Assert.NotNull(profile);
            Assert.Equal(value, profile.HideIpAddressInCalls);
            using var scope = fixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(value, (await db.Users.AsNoTracking().SingleAsync(u => u.Id == id, ct)).HideIpAddressInCalls);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Omitted_or_null_preference_does_not_reset_an_existing_value(bool explicitNull)
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await CreateUserAsync();
        using var client = Client(id);
        using var enabled = await client.PutAsJsonAsync(Endpoint, new { hideIpAddressInCalls = true }, ct);
        Assert.Equal(HttpStatusCode.OK, enabled.StatusCode);

        using var update = explicitNull
            ? await client.PutAsJsonAsync(Endpoint, new { fullName = "Updated profile", hideIpAddressInCalls = (bool?)null }, ct)
            : await client.PutAsJsonAsync(Endpoint, new { fullName = "Updated profile" }, ct);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var profile = await client.GetFromJsonAsync<Me>(Endpoint, ct);
        Assert.NotNull(profile);
        Assert.True(profile.HideIpAddressInCalls);
        Assert.Equal("Updated profile", profile.FullName);
    }

    [Fact]
    public async Task Update_uses_the_authenticated_account_not_a_user_id_in_the_payload()
    {
        var ct = TestContext.Current.CancellationToken;
        var owner = await CreateUserAsync();
        var other = await CreateUserAsync();
        using var client = Client(owner);
        using var response = await client.PutAsJsonAsync(Endpoint,
            new { userId = other, hideIpAddressInCalls = true }, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var otherClient = Client(other);
        var otherProfile = await otherClient.GetFromJsonAsync<Me>(Endpoint, ct);
        Assert.NotNull(otherProfile);
        Assert.False(otherProfile.HideIpAddressInCalls);
        var ownProfile = await client.GetFromJsonAsync<Me>(Endpoint, ct);
        Assert.NotNull(ownProfile);
        Assert.True(ownProfile.HideIpAddressInCalls);
    }

    [Theory]
    [InlineData(null, "api", HttpStatusCode.Unauthorized)]
    [InlineData("unknown-account", "profile", HttpStatusCode.Forbidden)]
    [InlineData("unknown-account", "api", HttpStatusCode.NotFound)]
    public async Task Read_and_write_require_an_existing_authenticated_account_and_api_scope(
        string? subject, string scope, HttpStatusCode expected)
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = Client(subject, scope);
        using var read = await client.GetAsync(Endpoint, ct);
        Assert.Equal(expected, read.StatusCode);
        using var write = await client.PutAsJsonAsync(Endpoint, new { hideIpAddressInCalls = true }, ct);
        Assert.Equal(expected, write.StatusCode);
    }

    [Fact]
    public async Task Invalid_boolean_is_rejected_without_saving_other_profile_changes()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await CreateUserAsync();
        using var client = Client(id);
        using var response = await client.PutAsJsonAsync(Endpoint,
            new { fullName = "Must not be saved", hideIpAddressInCalls = "invalid" }, ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var profile = await client.GetFromJsonAsync<Me>(Endpoint, ct);
        Assert.NotNull(profile);
        Assert.False(profile.HideIpAddressInCalls);
        Assert.NotEqual("Must not be saved", profile.FullName);
    }

    [Fact]
    public async Task Activating_privacy_ends_an_existing_call_before_returning_success()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await CreateUserAsync();
        var other = await CreateUserAsync();
        var options = fixture.Services.GetRequiredService<IOptions<AudioVideoCallOptions>>().Value;
        var calls = fixture.Services.GetRequiredService<CallSignalingService>();
        var delivery = fixture.Services.GetRequiredService<RecordingMessageDelivery>();
        options.Enabled = true;
        try
        {
            var call = await calls.InviteAsync("General", id, other, "preference-caller", ["preference-callee"], false);
            await calls.AcceptAsync(call.Id, other, "preference-callee", false);
            using var client = Client(id);
            using var response = await client.PutAsJsonAsync(Endpoint, new { hideIpAddressInCalls = true }, ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(delivery.Pushes, p => p.Method == CallHubMethods.Ended &&
                p.Arguments[0] is CallEnded ended && ended.CallId == call.Id && ended.Reason == "profile-changed");
            await Assert.ThrowsAsync<HubException>(() => calls.GetSessionAsync(call.Id, id, "preference-caller"));
        }
        finally
        {
            await calls.EndForUserAsync(id, "test-cleanup");
            options.Enabled = false;
        }
    }
}
