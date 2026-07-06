using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Yavsc;
using Yavsc.Models;
using Yavsc.Server.Services;
using Yavsc.Services;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Test host for the Yavsc.Blogs API surface. Specialisation of
/// <see cref="WebHostFixture"/> that wires up only the bits the
/// blog API actually depends on:
///
/// <list type="bullet">
///   <item><description>An in-memory <see cref="ApplicationDbContext"/>
///   (the real one — no mock) so <c>BlogSpotService.Index</c> can run
///   against an empty table and return an empty list.</description></item>
///   <item><description>A trivial <see cref="IFileSystemAuthManager"/>
///   stub: the GET index path doesn't read the file system, so any
///   implementation is fine.</description></item>
///   <item><description>The default <see cref="IAuthorizationService"/>
///   from <c>Microsoft.AspNetCore.Authorization</c>.</description></item>
///   <item><description>The test auth bypass from
///   <c>Yavsc.Tests.Shared</c> so the <c>[Authorize("BlogScope")]</c>
///   attribute on <c>BlogApiController</c> is satisfied when the
///   test sends the <c>X-Test-Role</c> header.</description></item>
/// </list>
///
/// No IdentityServer, no SMTP, no static assets — the Org fixture
/// owns all of that and we don't need any of it for blog integration
/// tests.
/// </summary>
public sealed class BlogsWebServerFixture : WebHostFixture
{
    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        // Use the real ApplicationDbContext with an in-memory store.
        // BlogSpotService reads _context.BlogSpot directly, so any
        // attempt to mock it would be wasted work; the real service
        // against an empty table returns an empty list, which is
        // exactly what the first test wants to assert.
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseInMemoryDatabase("Yavsc.Blogs.Tests"));

        // Trivial file-system auth: the GET index path never calls
        // into it, but the DI container needs an instance.
        builder.Services.AddSingleton<IFileSystemAuthManager>(
            new NoopFileSystemAuthManager());

        // Real BlogSpotService — same instance the production host
        // builds (ApplicationDbContext, IAuthorizationService,
        // IFileSystemAuthManager).
        builder.Services.AddScoped<BlogSpotService>();

        // The BlogApiController is reached through MVC, so register
        // MVC + the BlogScope authorization policy.
        builder.Services.AddControllers();
        builder.Services.AddAuthorization(opt =>
        {
            // Mirror the production "BlogScope" policy: any
            // authenticated user. The TestAuthPolicyProvider we
            // register below short-circuits the role check via the
            // X-Test-Role header.
            opt.AddPolicy("BlogScope", p => p.RequireAssertion(_ => true));
        });

        // Test auth bypass — swapped in BEFORE the host builds the
        // service collection, so it overrides any production
        // policy provider registered by AddAuthorization above.
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, TestAuthPolicyProvider>();

        return builder.Build();
    }

    protected override async Task<WebApplication> ConfigurePipelineAsync(WebApplication app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await Task.CompletedTask;
        return app;
    }

    /// <summary>Trivial <see cref="IFileSystemAuthManager"/> stub. The
    /// blog API endpoints exercised by the first tests don't read the
    /// file system, so the implementation can be a no-op.</summary>
    private sealed class NoopFileSystemAuthManager : IFileSystemAuthManager
    {
        public FileAccessRight GetFilePathAccess(ClaimsPrincipal user, string fileRelativePath)
            => FileAccessRight.None;

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
        }
    }
}
