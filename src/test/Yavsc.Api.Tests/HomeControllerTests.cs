using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Controllers;
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
