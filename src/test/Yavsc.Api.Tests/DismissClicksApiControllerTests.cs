using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class DismissClicksApiControllerTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private HttpClient NewClient(string? subject = "alice")
    {
        var http = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        })
        {
            BaseAddress = new Uri(fixture.BaseAddress)
        };
        if (subject != null)
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, "api"));
        return http;
    }

    private async Task<long> SeedNotificationAsync()
    {
        fixture.ResetAndSeedActivityGraph();
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var note = new Notification { title = "Info", body = "Corps", click_action = "Fermer" };
        db.Notification.Add(note);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return note.Id;
    }

    [Fact]
    public async Task Click_dismisses_once_and_hides_the_notification_for_the_user()
    {
        var id = await SeedNotificationAsync();
        var ct = TestContext.Current.CancellationToken;
        using var http = NewClient();

        for (var i = 0; i < 2; i++)
        {
            var response = await http.PostAsync($"/api/v1/dismiss/click/{id}", null, ct);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<DismissedNotification>(ct);
            Assert.Equal(id, dto!.NotificationId);
        }

        var dismissed = await http.GetFromJsonAsync<List<DismissedNotification>>("/api/v1/dismiss", ct);
        Assert.Equal(id, Assert.Single(dismissed!).NotificationId);

        var visible = await http.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct);
        Assert.DoesNotContain(visible!, n => n.Id == id);

        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(1, await db.DismissClicked.CountAsync(d => d.UserId == "alice" && d.NotificationId == id, ct));
        }

        var restore = await http.DeleteAsync($"/api/v1/dismiss/click/{id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, restore.StatusCode);
        visible = await http.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct);
        Assert.Contains(visible!, n => n.Id == id);
    }

    [Fact]
    public async Task Click_on_an_unknown_notification_returns_not_found()
    {
        var id = await SeedNotificationAsync();
        using var http = NewClient();

        var response = await http.PostAsync($"/api/v1/dismiss/click/{id + 1000}", null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Click_requires_authentication()
    {
        var id = await SeedNotificationAsync();
        using var http = NewClient(null);

        var response = await http.PostAsync($"/api/v1/dismiss/click/{id}", null,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
