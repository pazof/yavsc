using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yavsc.Blogs.Controllers;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Services;
using Yavsc.Tests.Shared;
using static Yavsc.Constants;
namespace Yavsc.Blogs.Tests;

/// <summary>
/// Shared integration-test host for the Yavsc.Blogs API surface.
/// Specialisation of <see cref="WebHostFixture"/> that wires up
/// only the bits the blog API actually depends on:
///
/// <list type="bullet">
///   <item><description>A SQLite <c>:memory:</c> database
///   (<see cref="Microsoft.EntityFrameworkCore.Sqlite"/>) backed
///   by a single shared <see cref="SqliteConnection"/> held open
///   for the lifetime of the host. SQLite enforces real foreign
///   keys and real transactional semantics, so the tests see the
///   same INSERT-time FK validation a production Postgres host
///   would — unlike the EF Core InMemory provider, which silently
///   ignores FKs and masks bugs that surface only against a real
///   relational engine.</description></item>
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
/// tests. Marked <see cref="CollectionDefinitionAttribute"/> so the
/// host is shared across every <c>[Collection("Yavsc Blogs")]</c>
/// test class: one host, one SQLite DB, one Kestrel port.
/// </summary>
[CollectionDefinition("Yavsc Blogs")]
public sealed class BlogsWebServerFixture : WebHostFixture
{
    protected override int HttpsPort => 5103;

    public long CircleId { get; private set; }
    public long PostId { get; private set; }
    public string DefaultUserLogin { get => "alice"; }

    // A single SqliteConnection held open at the static level,
    // mirroring how Yavsc.Org.Tests.WebServerFixture hoists its
    // shared configuration into static slots. Closing the
    // connection destroys the in-memory database — so we close
    // it only when the last fixture instance is disposed (see
    // Dispose below), exactly when WebHostFixture tears down the
    // host.
    private static SqliteConnection? _sharedSqliteConnection;
    private static readonly object _sqliteLock = new();

    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        // Open the shared in-memory connection lazily on the first
        // fixture construction. Subsequent constructions (xUnit
        // creates one fixture instance per IClassFixture) reuse
        // the same connection so all DbContexts across all tests
        // see the same database.
        SqliteConnection sharedConnection;
        lock (_sqliteLock)
        {
            if (_sharedSqliteConnection is null)
            {
                // Mode=Memory + Cache=Shared gives us a named
                // in-memory database that every connection string
                // referencing "File:YavscBlogsTests?mode=memory&cache=shared"
                // will resolve to the same backing store, as long
                // as at least one SqliteConnection stays open
                // against it.
                _sharedSqliteConnection = new SqliteConnection(
                    "Data Source=YavscBlogsTests;Mode=Memory;Cache=Shared");
                _sharedSqliteConnection.Open();
            }
            sharedConnection = _sharedSqliteConnection;
        }

        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            // UseSqlite(DbConnection) keeps the connection we just
            // opened alive for the DbContext's lifetime, instead of
            // letting EF open and close its own. Without this,
            // each DbContext would get a fresh connection pointing
            // at an empty :memory: store and nothing would persist
            // across requests.
            opt.UseSqlite(sharedConnection));

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
                options.TokenValidationParameters
                = new TokenValidationParameters
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
                    RoleClaimType = Yavsc.Constants.RoleClaimType,
                };
            });

        return builder.Build();
    }

    protected override async Task<WebApplication> ConfigurePipelineAsync(WebApplication app)
    {
        // UseDeveloperExceptionPage gives full stack traces on
        // 500s during tests — much easier to debug than the
        // default empty InternalServerError body. Production
        // (Yavsc.Org) wires its own exception handler; this
        // fixture is test-only.
        app.UseDeveloperExceptionPage();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // EnsureCreated + seed alice, run once at host startup.
        // EnsureCreated is idempotent (creates only the tables that
        // don't exist yet) and runs against the shared
        // SqliteConnection (Cache=Shared), so every DbContext that
        // resolves through this fixture's host sees the same schema.
        // We do NOT call EnsureDeleted: the SqliteConnection is held
        // open at the static level and closing it destroys the
        // :memory: store for every other DbContext — the org
        // fixture can afford EnsureDeleted because its store is
        // built fresh per fixture, but the blogs fixture's static
        // connection outlives a single fixture instance.
        using (var seedScope = app.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            if (!db.Users.Any(u => u.Id == "alice"))
            {
                db.Users.Add(new ApplicationUser
                {
                    Id = "alice",
                    UserName = "alice",
                    Email = "alice@example.com",
                    EmailConfirmed = true,
                    FullName = "Alice Dupont",
                    Avatar = "/avatars/alice.png",
                });
                db.SaveChanges();

                // Inline the seed of the circle + post. We don't
                // call SeedCircle/SeedBlogPost (the instance helpers)
                // because those resolve through this.Services, which
                // is null until WebHostFixture.InitializeAsync has
                // finished wiring the shared slot — i.e. after this
                // method returns. Use app.Services directly.
                var circle = new Circle
                {
                    OwnerId = "alice",
                    Name = "test",
                    Public = true,
                };
                db.Circle.Add(circle);
                db.SaveChanges();
                CircleId = circle.Id;

                var post = new BlogPost
                {
                    AuthorId = "alice",
                    Title = "Billet ACL test",
                    Article = "Test article body.",
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                };
                db.BlogSpot.Add(post);
                db.SaveChanges();
                PostId = post.Id;
            }
        }

        await Task.CompletedTask;
        return app;
    }
/// <summary>Reset the in-memory database to a known empty state.
    /// <c>UseInMemoryDatabase</c> shares its store across the
    /// lifetime of the <see cref="BlogsWebServerFixture"/> instance,
    /// so without a per-test reset the test order would leak
    /// state between tests.</summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
    public void CleanupAcl()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.CircleAuthorizationToBlogPost
            .Where(a => a.CircleId == CircleId
                     && a.BlogPostId == PostId)
            .ExecuteDelete();
    }

    public override void Dispose()
    {
        try
        {
            base.Dispose();
        }
        finally
        {
            // Close the shared SQLite connection only when the
            // last fixture instance goes away, matching the
            // lifetime contract of WebHostFixture.Dispose. We
            // rely on base.Dispose's _instanceCount decrement
            // having run, so we close only if the host is gone
            // (base already nulled _app when count==0).
            lock (_sqliteLock)
            {
                if (_sharedSqliteConnection is not null)
                {
                    // Synchronous close: SQLite's Close() is
                    // documented as safe to call from a sync
                    // context and avoids the GetAwaiter().GetResult()
                    // pattern that's historically caused teardown
                    // hangs in this repo's async pipeline.
                    _sharedSqliteConnection.Close();
                    _sharedSqliteConnection.Dispose();
                    _sharedSqliteConnection = null;
                }
            }
        }
    }

    /// <summary>Seed an <see cref="ApplicationUser"/> in the shared
    /// SQLite store, so tests that POST/PUT/DELETE a
    /// <c>BlogPost</c> (whose <c>AuthorId</c> is a FK to
    /// <c>AspNetUsers.Id</c>) don't trip the FK constraint that
    /// SQLite enforces but the EF Core InMemory provider silently
    /// ignored. Idempotent on <paramref name="userName"/>: a
    /// second call for the same id is a no-op (the user already
    /// exists).</summary>
    /// <param name="userName">Both the PK id and the login name.
    /// The JWT subject in tests is this same string, so seeding
    /// this id is enough to make the FK from a
    /// <c>BlogPost.AuthorId</c> resolve.</param>
    /// <param name="configure">Optional hook to fill in fields
    /// like <c>FullName</c> / <c>Avatar</c> / <c>EmailConfirmed</c>
    /// that downstream tests assert on.</param>
    public ApplicationUser SeedUser(string userName,
    Action<ApplicationUser>? configure = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existing = db.Users.SingleOrDefault(u => u.Id == userName);
        if (existing != null) return existing;

        // Email is an alternate key on ApplicationUser; seeding
        // it explicitly avoids the InMemory provider's null-claim
        // tracking quirk (cf. PublishEndpointTests.ResetDatabase)
        // and keeps the column shape realistic for prod.
        var user = new ApplicationUser
        {
            Id = userName,
            UserName = userName,
            Email = $"{userName}@example.test",
        };
        configure?.Invoke(user);
        db.Users.Add(user);
        db.SaveChanges();
        return user;
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



    /// <summary>Create a circle owned by <paramref name="ownerId"/>
    /// directly in the SQLite store and return its server-assigned
    /// id.</summary>
    public long SeedCircle(string ownerId, string name, bool isPublic = false,
        ICollection<String> members = null
    )
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var circle = new Circle { OwnerId = ownerId, Name = name, Public = isPublic };
        db.Circle.Add(circle);
        db.SaveChanges();
        if (members != null && members.Count > 0)
        {
            foreach (String memberId in members)
            {
                var member = new CircleMember { CircleId = circle.Id, MemberId = memberId };
                db.CircleMembers.Add(member);
            }
            db.SaveChanges();
        }
        return circle.Id;
    }

    /// <summary>Create a blog post owned by <paramref name="authorId"/>
    /// directly in the SQLite store and return its server-assigned
    /// id.</summary>
    public long SeedBlogPost(string authorId, string title)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = new BlogPost
        {
            AuthorId = authorId,
            Title = title,
            Article = "Test article body.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.BlogSpot.Add(post);
        db.SaveChanges();
        return post.Id;
    }

}
