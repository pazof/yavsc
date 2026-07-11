using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yavsc.Blogs.Controllers;
using Yavsc.Models;
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
///   <item><description>The real <c>BlogSpotService</c>, which calls
///   <c>IAuthorizationService.AuthorizeAsync(user, blog, new EditPermission())</c>
///   on PUT. The fixture registers the real
///   <see cref="PermissionHandler"/> so the resource-based ownership
///   check runs end-to-end; tests that want a 204 PUT must sign a
///   JWT whose <c>sub</c> matches the post's <c>AuthorId</c>.</description></item>
///   <item><description>A real <c>AddJwtBearer</c> with HS256,
///   sharing its <see cref="TestTokenIssuer.SigningKey"/> with the
///   token issuer. The production OIDC discovery path is bypassed:
///   the test host validates tokens locally, against the static
///   signing key, so no IdP is required to exercise auth.</description></item>
///   <item><description>The production <c>BlogScope</c> policy
///   (RequireAuthenticatedUser + RequireClaim("scope", "blogs"))
///   registered verbatim. Tests that omit the bearer header exercise
///   the unauthenticated path and get 401.</description></item>
/// </list>
///
/// No IdentityServer, no SMTP, no static assets — the Org fixture
/// owns all of that and we don't need any of it for blog integration
/// tests.
/// </summary>
public sealed class BlogsWebServerFixture : WebHostFixture
{
    // SQLite in-memory database is created once and shared across all
    // DbContext instances for the test lifetime. The connection must
    // stay open: closing it destroys the in-memory database. The
    // Microsoft.Data.Sqlite pool will then open additional connections
    // to the same in-memory store, as long as the original connection
    // is alive. This is the SQLite equivalent of the EF Core
    // InMemoryDatabaseRoot we used to use.
    private Microsoft.Data.Sqlite.SqliteConnection? _sharedSqliteConnection;
    // Legacy field kept to make the migration diff readable. The
    // InMemory provider path is no longer used by this fixture, but
    // removing it is out of scope for the SQLite-in-memory migration
    // (forgejo#3 follow-up).
    [System.Obsolete("Replaced by SQLite in-memory (forgejo#3).")]
    private InMemoryDatabaseRoot? _inMemoryRoot;

    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        // Use the real ApplicationDbContext with an in-memory store.
        // BlogSpotService reads _context.BlogSpot directly, so any
        // attempt to mock it would be wasted work; the real service
        // against an empty table returns an empty list, which is
        // exactly what the first test wants to assert.
        //
        // We use SQLite in-memory (not the EF Core InMemory provider)
        // because the InMemory provider cannot materialise navigation
        // properties from IdentityServer8 entity types (see forgejo#3).
        // SQLite in-memory is a transient, file-less store that
        // executes real SQL, so navigation properties work as
        // expected. The shared SqliteConnection keeps the database
        // alive for the test lifetime, mirroring the
        // InMemoryDatabaseRoot pattern we used previously.
        _sharedSqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        _sharedSqliteConnection.Open();
        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlite(_sharedSqliteConnection));

        // Trivial file-system auth: the GET index path never calls
        // into it, but the DI container needs an instance.
        builder.Services.AddSingleton<IFileSystemAuthManager>(
            new NoopFileSystemAuthManager());

        // Real BlogSpotService — same instance the production host
        // builds (ApplicationDbContext, IAuthorizationService,
        // IFileSystemAuthManager). With PermissionHandler registered
        // below, Modify() now answers "is the caller the author of
        // the post?" for real, which is exactly what we want to
        // assert in the PUT tests.
        builder.Services.AddScoped<BlogSpotService>();

        // The real PermissionHandler: BlogSpotService calls
        // IAuthorizationService.AuthorizeAsync(user, blog, new
        // EditPermission()) on Modify, and PermissionHandler
        // resolves it via IsOwner(user, blog) — i.e. blog.AuthorId
        // == user.GetUserId(). To PUT a post, the test JWT must
        // carry sub == post.AuthorId.
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        // The BlogApiController is reached through MVC. AddControllers()
        // by default scans the test assembly only; we explicitly add the
        // Yavsc.Blogs application part so the controller is discovered
        // and routed.
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BlogApiController).Assembly);

        // Production BlogScope policy, verbatim. Two requirements:
        // 1. RequireAuthenticatedUser: a request with no bearer
        //    token (or an invalid one) will be rejected.
        // 2. RequireClaim("scope", "blogs"): the JWT must carry a
        //    "scope" claim whose value is "blogs".
        // TestTokenIssuer.Issue() defaults to scope=blogs; the
        // GetBlog_returns_401_when_no_token test omits the token
        // entirely and asserts the policy fails closed.
        builder.Services.AddAuthorization(opt =>
        {
            opt.AddPolicy("BlogScope", policy =>
            {
                policy.RequireAuthenticatedUser()
                      .RequireClaim("scope", "blogs");
            });
        });

        // Real JWT Bearer authentication, sharing the signing key
        // with TestTokenIssuer. No Authority → no OIDC discovery,
        // no IdP roundtrip; the middleware validates the signature
        // and the standard claims against the static configuration
        // below. Production uses AddYavscJwtBearer with an IdP, but
        // for the unit-test host that path is unwanted coupling.
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.IncludeErrorDetails = true;
                // MapInboundClaims = false here mirrors the
                // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap
                // .Clear() in TestTokenIssuer: the validation
                // pipeline must not rewrite "sub" to
                // ClaimTypes.NameIdentifier, otherwise the
                // PermissionHandler ownership check sees a null
                // user id and rejects every PUT.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestTokenIssuer.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestTokenIssuer.SigningKey,
                    // "sub" stays "sub" (MapInboundClaims only
                    // remaps long Microsoft claim URIs, not sub).
                    // UserHelpers.GetUserId reads sub directly.
                    NameClaimType = "sub",
                    RoleClaimType = YavscConstants.RoleClaimType,
                };
            });

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
        public FileAccessRight GetFilePathAccess(System.Security.Claims.ClaimsPrincipal user, string fileRelativePath)
            => FileAccessRight.None;

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
        }
    }
}
