using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Controllers;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class HomeControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public HomeControllerTests(ApiWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Index_does_not_list_activity_without_declaration()
    {
        _fixture.ResetAndSeedActivityGraph();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();

        var controller = new HomeController(
            NullLogger<HomeController>.Instance,
            localizer: null!,
            context: db,
            settingsOptions: Options.Create(new SiteSettings()),
            env: new TestEnvironment());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("sub", "alice"),
                    new Claim(ClaimTypes.NameIdentifier, "alice")
                }, "Bearer"))
            }
        };

        var result = await controller.Index(id: null);
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Activity>>(view.Model);

        Assert.Contains(model, a => a.Code == "dev");
        Assert.DoesNotContain(model, a => a.Code == "ghost");
    }

    [Fact]
    public async Task Index_does_not_show_performer_notification_to_anonymous_visitors()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        db.Notification.AddRange(
            Note("Rendez-vous professionnel", "user/alice"),
            Note("Annonce publique", "pub/all"));
        await db.SaveChangesAsync();

        var controller = CreateHomeController(db, new ClaimsPrincipal(new ClaimsIdentity()));
        controller.ControllerContext.HttpContext.Session = new TestSession();

        await controller.Index(id: null);

        object notifyValue = controller.ViewBag.Notify;
        var notifications = Assert.IsAssignableFrom<IEnumerable<Notification>>(notifyValue);
        Assert.Equal("Annonce publique", Assert.Single(notifications).title);
    }

    [Fact]
    public async Task Index_shows_performer_notification_only_to_its_target_user()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        db.Notification.AddRange(
            Note("Rendez-vous professionnel", "user/alice"),
            Note("Autre rendez-vous", "user/bob"),
            Note("Annonce publique", "pub/all"));
        await db.SaveChangesAsync();

        var controller = CreateHomeController(db, UserPrincipal("bob"));
        await controller.Index(id: null);

        object notifyValue = controller.ViewBag.Notify;
        var notifications = Assert.IsAssignableFrom<IEnumerable<Notification>>(notifyValue);
        Assert.Equal(new[] { "Autre rendez-vous", "Annonce publique" },
            notifications.Select(notification => notification.title));

        var performerController = CreateHomeController(db, UserPrincipal("alice"));
        await performerController.Index(id: null);

        object performerNotifyValue = performerController.ViewBag.Notify;
        var performerNotifications = Assert.IsAssignableFrom<IEnumerable<Notification>>(performerNotifyValue);
        Assert.Equal(new[] { "Rendez-vous professionnel", "Annonce publique" },
            performerNotifications.Select(notification => notification.title));
    }

    private static HomeController CreateHomeController(
        Yavsc.Models.ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var controller = new HomeController(
            NullLogger<HomeController>.Instance,
            localizer: null!,
            context: db,
            settingsOptions: Options.Create(new SiteSettings()),
            env: new TestEnvironment());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        return controller;
    }

    private static ClaimsPrincipal UserPrincipal(string userId)
        => new(new ClaimsIdentity(new[]
        {
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "Bearer"));

    private static Notification Note(string title, string target)
        => new()
        {
            title = title,
            body = "Corps",
            click_action = "Fermer",
            Target = target
        };

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public bool IsAvailable => true;
        public string Id => "home-controller-test-session";
        public IEnumerable<string> Keys => _values.Keys;

        public void Clear() => _values.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _values.Remove(key);
        public void Set(string key, byte[] value) => _values[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _values.TryGetValue(key, out value!);
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Yavsc.Api.Test";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
