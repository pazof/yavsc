using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Services;

namespace Yavsc.Server.Tests.Fixtures;

/// <summary>
/// Service-level test host for <see cref="BlogSpotService"/> and the
/// other Yavsc.Server services that take an
/// <see cref="ApplicationDbContext"/> + <see cref="IAuthorizationService"/>.
///
/// <para>Unlike <c>BlogsWebServerFixture</c> (in Yavsc.Blogs.Tests),
/// this fixture does <b>not</b> spin up Kestrel, JWT bearer, or MVC
/// controllers. <see cref="BlogSpotService.Details"/> takes the
/// <see cref="System.Security.Claims.ClaimsPrincipal"/> as a parameter,
/// so the caller controls identity directly — there is no HTTP/auth
/// middleware in the path under test. Building only the DI graph the
/// service actually resolves keeps these tests fast and isolates the
/// service layer from the blog API's transport concerns, which
/// Yavsc.Blogs.Tests already covers end-to-end.</para>
///
/// <para><b>Database provider.</b> Mirrors
/// <c>ApiWebServerFixture</c>: SQLite <c>:memory:</c> by default, or
/// real PostgreSQL when <c>YAVSC_SERVER_TEST_DB_PROVIDER=npgsql</c> is
/// set. Production runs on Npgsql, so the Npgsql path — which applies
/// the real <c>Yavsc.Org</c> migrations against a dedicated test
/// database — is the one that catches provider-specific bugs (FK
/// enforcement, type mappings, migration drift). SQLite stays the
/// default so the suite runs out-of-the-box with no external server.</para>
///
/// <list type="bullet">
///   <item><description>SQLite: a named <c>:memory:</c> database
///   (Cache=Shared) backed by a single connection held open for the
///   fixture's lifetime, schema via <c>EnsureCreated</c>. Reset is
///   <c>EnsureDeleted</c> + <c>EnsureCreated</c>.</description></item>
///   <item><description>Npgsql: a dedicated database
///   <c>yavscServerTestDb</c> (created from the admin connection if it
///   does not exist), schema via <c>Migrate</c> using the
///   <c>Yavsc.Org</c> migrations assembly. Reset purges the blog-graph
///   tables in FK order — children before parents — so no DELETE
///   violates a constraint, instead of dropping the database (which
///   would force a full re-migration per test).</description></item>
///   <item><description>The real <see cref="BlogSpotService"/> and the
///   real <see cref="PermissionHandler"/>: <c>Details</c> calls
///   <c>IAuthorizationService.AuthorizeAsync(user, blog, new
///   ReadPermission())</c>, and <see cref="PermissionHandler"/> answers
///   that requirement with the public/owner/sponsor/Administrator
///   disjunction that the tests below pin. No mocking.</description></item>
///   <item><description>A trivial <see cref="IFileSystemAuthManager"/>
///   stub: <c>Details</c> never touches the file system, but the DI
///   container needs an instance to construct the service.</description></item>
/// </list>
///
/// <para>Marked <see cref="CollectionDefinitionAttribute"/> so a single
/// fixture instance (one store, one DI container) is shared across
/// every <c>[Collection("Yavsc Server")]</c> test class. Each test
/// resets the store via <see cref="ResetDatabase"/>.</para>
/// </summary>
[CollectionDefinition("Yavsc Server")]
public sealed class ServerServicesFixture : IDisposable
{
    private const string DbProviderEnvVar = "YAVSC_SERVER_TEST_DB_PROVIDER";
    private const string NpgsqlAdminConnectionEnvVar = "YAVSC_SERVER_TEST_NPGSQL_ADMIN_CONNECTION";
    private const string DedicatedNpgsqlDatabaseName = "yavscServerTestDb";

    private static readonly object _npgsqlLock = new();
    private static string? _sharedNpgsqlConnectionString;

    private readonly SqliteConnection? _sqliteConnection;
    private readonly ServiceProvider _services;

    public IServiceProvider Services => _services;

    public ServerServicesFixture()
    {
        var services = new ServiceCollection();

        if (UseNpgsqlProvider())
        {
            var npgsqlConnectionString = EnsureNpgsqlDatabaseCreated();
            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseNpgsql(npgsqlConnectionString,
                    x => x.MigrationsAssembly("Yavsc.Org")));
        }
        else
        {
            // Mode=Memory + Cache=Shared: a named in-memory database
            // every connection string referencing "YavscServerTests"
            // resolves to the same backing store, as long as at least
            // one connection stays open against it. _sqliteConnection
            // is held for the fixture's lifetime (disposed in Dispose),
            // so the store survives across scoped DbContexts per test.
            _sqliteConnection = new SqliteConnection(
                "Data Source=YavscServerTests;Mode=Memory;Cache=Shared");
            _sqliteConnection.Open();

            services.AddDbContext<ApplicationDbContext>(opt =>
                // UseSqlite(DbConnection) keeps _sqliteConnection alive
                // for the DbContext's lifetime instead of letting EF
                // open and close its own — without this each DbContext
                // would get a fresh connection pointing at an empty
                // store.
                opt.UseSqlite(_sqliteConnection));
        }

        // BlogSpotService.AttachFiles writes under siteSettings.Blog.
        // Details itself doesn't read it, but the service constructor
        // throws if SiteSettings.Blog is null, so configure a temp
        // value to keep construction alive.
        services.Configure<SiteSettings>(s => s.Blog = "server-tests-files-root");

        // Trivial file-system auth: Details never calls into it, but
        // the DI container needs an instance to construct the service.
        services.AddSingleton<IFileSystemAuthManager>(
            new NoopFileSystemAuthManager());

        // The real BlogSpotService — same shape the production host
        // builds (ApplicationDbContext, IAuthorizationService,
        // IFileSystemAuthManager, IOptions<SiteSettings>,
        // ILoggerFactory).
        services.AddScoped<BlogSpotService>();

        // The real PermissionHandler: Details authorizes with
        // ReadPermission, and PermissionHandler resolves it via the
        // IsPublic / IsOwner / IsSponsor / Administrator disjunction.
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        // Default authorization services register
        // IAuthorizationService (DefaultAuthorizationService), which
        // collects the IAuthorizationHandler above and runs it against
        // the requirement passed to AuthorizeAsync.
        services.AddAuthorization();
        services.AddLogging();

        _services = services.BuildServiceProvider();

        // Create the schema once. Npgsql uses Migrate (the Yavsc.Org
        // migrations) so the test schema matches production; SQLite uses
        // EnsureCreated (idempotent, model-derived). ResetDatabase
        // re-creates / purges per test.
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            if (UseNpgsqlProvider())
                db.Database.Migrate();
            else
                db.Database.EnsureCreated();
        }
    }

    /// <summary>True when <c>YAVSC_SERVER_TEST_DB_PROVIDER=npgsql</c>
    /// is set — the fixture then runs against a real PostgreSQL server
    /// instead of the SQLite in-memory default.</summary>
    public static bool UseNpgsqlProvider()
        => string.Equals(
            Environment.GetEnvironmentVariable(DbProviderEnvVar),
            "npgsql",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>Reset the store to a known empty state for the next
    /// test. SQLite: <c>EnsureDeleted</c> + <c>EnsureCreated</c> (the
    /// named in-memory store survives because _sqliteConnection is held
    /// open). Npgsql: purge the blog-graph tables in FK order —
    /// children before parents — so no DELETE trips a foreign key,
    /// avoiding a full re-migration per test.</summary>
    public void ResetDatabase()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        if (UseNpgsqlProvider())
        {
            // Children before parents. BlogAttachedFile, Comment and
            // BlogSpotPublication reference BlogSpot; CircleAuthorization
            // ToBlogPost references BlogSpot + Circle; BlogTag references
            // BlogSpot + Tag; BlogSpot references ApplicationUser;
            // CircleMember references Circle + ApplicationUser.
            db.BlogAttachedFiles.RemoveRange(db.BlogAttachedFiles);
            db.Comment.RemoveRange(db.Comment);
            db.blogSpotPublications.RemoveRange(db.blogSpotPublications);
            db.CircleAuthorizationToBlogPost
                .RemoveRange(db.CircleAuthorizationToBlogPost);
            db.BlogTag.RemoveRange(db.BlogTag);
            db.UploadedFiles.RemoveRange(db.UploadedFiles);
            db.BlogSpot.RemoveRange(db.BlogSpot);
            db.CircleMembers.RemoveRange(db.CircleMembers);
            db.Circle.RemoveRange(db.Circle);
            db.Users.RemoveRange(db.Users);
            db.SaveChanges();
            return;
        }

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    /// <summary>Resolve a fresh service scope and the
    /// <see cref="ApplicationDbContext"/> from it, run
    /// <paramref name="body"/>, and dispose the scope (and its
    /// DbContext). Use this for seeding writes that should not share
    /// a DbContext with the service call under test.</summary>
    public T WithDb<T>(Func<ApplicationDbContext, T> body)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        return body(db);
    }

    public void SeedUser(string userName,
        Action<ApplicationUser>? configure = null)
    {
        WithDb(db =>
        {
            if (db.Users.Any(u => u.Id == userName)) return false;
            var user = new ApplicationUser
            {
                Id = userName,
                UserName = userName,
                Email = $"{userName}@example.test",
            };
            configure?.Invoke(user);
            db.Users.Add(user);
            db.SaveChanges();
            return true;
        });
    }

    public long SeedBlogPost(string authorId, string title,
        Action<BlogPost>? configure = null)
    {
        return WithDb(db =>
        {
            var post = new BlogPost
            {
                AuthorId = authorId,
                Title = title,
                Article = "Test article body.",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            };
            configure?.Invoke(post);
            db.BlogSpot.Add(post);
            db.SaveChanges();
            return post.Id;
        });
    }

    /// <summary>Insert a <see cref="BlogSpotPublication"/> row for
    /// <paramref name="postId"/> — the existence of that row is what
    /// makes a post "public" (readable by anyone) in
    /// <see cref="PermissionHandler"/>. This is the same write
    /// <c>SetPublishAsync</c> performs in production.</summary>
    public void Publish(long postId)
    {
        WithDb(db =>
        {
            db.blogSpotPublications.Add(
                new BlogSpotPublication { PostId = postId });
            db.SaveChanges();
            return true;
        });
    }

    /// <summary>Seed a circle owned by <paramref name="ownerId"/> and
    /// return its server-assigned id. Sponsorship in
    /// <see cref="PermissionHandler"/> is "the viewer is a member of a
    /// circle the post's author owns", so a circle + a
    /// <see cref="SeedCircleMember"/> entry is what grants a non-owner
    /// read access to a draft.</summary>
    public long SeedCircle(string ownerId, string name)
    {
        return WithDb(db =>
        {
            var circle = new Circle
            {
                OwnerId = ownerId,
                Name = name,
                Public = false,
            };
            db.Circle.Add(circle);
            db.SaveChanges();
            return circle.Id;
        });
    }

    public void SeedCircleMember(long circleId, string memberId)
    {
        WithDb(db =>
        {
            db.CircleMembers.Add(
                new CircleMember { CircleId = circleId, MemberId = memberId });
            db.SaveChanges();
            return true;
        });
    }

    /// <summary>Attach a <see cref="CircleAuthorizationToBlogPost"/> ACL
    /// entry to <paramref name="postId"/> for <paramref name="circleId"/>.
    /// The owner always sees the ACL; non-owners have it scrubbed by
    /// <see cref="BlogSpotService"/>'s ScrubAclForViewer.</summary>
    public void SeedAcl(long postId, long circleId)
    {
        WithDb(db =>
        {
            db.CircleAuthorizationToBlogPost.Add(
                new CircleAuthorizationToBlogPost
                {
                    BlogPostId = postId,
                    CircleId = circleId,
                });
            db.SaveChanges();
            return true;
        });
    }

    /// <summary>Seed a comment on <paramref name="postId"/> authored by
    /// <paramref name="authorId"/>. <see cref="BlogSpotService.Details"/>
    /// loads each comment's <c>Author</c> navigation from
    /// <c>_context.Users</c>; seeding the user first keeps the FK
    /// valid (SQLite and PostgreSQL both enforce it).</summary>
    public void SeedComment(long postId, string authorId, string article)
    {
        WithDb(db =>
        {
            db.Comment.Add(new Comment
            {
                ReceiverId = postId,
                AuthorId = authorId,
                Article = article,
                Visible = true,
                UserModified = authorId,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
            db.SaveChanges();
            return true;
        });
    }

    /// <summary>Resolve the real <see cref="BlogSpotService"/> from the
    /// fixture's DI container, run <paramref name="body"/> against it,
    /// and dispose the request scope (and its scoped DbContext). The
    /// service is scoped, so it must be resolved from a scope — and the
    /// scope is what we dispose to avoid leaking a DbContext against
    /// the shared connection across tests.</summary>
    public async Task<T> WithServiceAsync<T>(
        Func<BlogSpotService, Task<T>> body)
    {
        using var scope = _services.CreateScope();
        var service = scope.ServiceProvider
            .GetRequiredService<BlogSpotService>();
        return await body(service);
    }

    private static string EnsureNpgsqlDatabaseCreated()
    {
        lock (_npgsqlLock)
        {
            if (!string.IsNullOrWhiteSpace(_sharedNpgsqlConnectionString))
                return _sharedNpgsqlConnectionString;

            var adminConnectionString = BuildAdminConnectionString();
            var databaseName = DedicatedNpgsqlDatabaseName;

            using (var adminConnection = new NpgsqlConnection(adminConnectionString))
            {
                adminConnection.Open();
                using var existsCommand = adminConnection.CreateCommand();
                existsCommand.CommandText =
                    "SELECT 1 FROM pg_database WHERE datname = @databaseName";
                existsCommand.Parameters.AddWithValue("databaseName", databaseName);

                if (existsCommand.ExecuteScalar() is null)
                {
                    using var createCommand = adminConnection.CreateCommand();
                    createCommand.CommandText =
                        $"CREATE DATABASE \"{databaseName}\"";
                    createCommand.ExecuteNonQuery();
                }
            }

            var testConnectionBuilder = new NpgsqlConnectionStringBuilder(adminConnectionString)
            {
                Database = databaseName,
                Pooling = false,
                IncludeErrorDetail = true
            };

            _sharedNpgsqlConnectionString = testConnectionBuilder.ToString();
            return _sharedNpgsqlConnectionString;
        }
    }

    private static string BuildAdminConnectionString()
    {
        // The admin connection string (host + credentials) is supplied
        // via environment, never embedded in source — committing a
        // real dev password would leak it into git history. The Npgsql
        // path is opt-in, so requiring the env var is fine: a missing
        // value produces a clear failure rather than a silent fallback
        // onto someone's local dev database.
        var configured = Environment.GetEnvironmentVariable(NpgsqlAdminConnectionEnvVar);
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException(
                $"{NpgsqlAdminConnectionEnvVar} is not set. The Npgsql test "
                + "provider requires an admin PostgreSQL connection string "
                + "in that environment variable (e.g. "
                + "'Server=localhost;Port=5432;Database=postgres;"
                + "Username=yavscdev;Password=...;Include Error Detail=true').");

        var builder = new NpgsqlConnectionStringBuilder(configured)
        {
            Pooling = false,
            IncludeErrorDetail = true
        };

        // The admin connection targets the maintenance database
        // (postgres) so we can CREATE/DROP the dedicated test database;
        // whatever database the env var names is overridden here.
        builder.Database = "postgres";

        return builder.ToString();
    }

    public void Dispose()
    {
        _services.Dispose();
        _sqliteConnection?.Dispose();
    }

    private sealed class NoopFileSystemAuthManager : IFileSystemAuthManager
    {
        public FileAccessRight GetFilePathAccess(
            System.Security.Claims.ClaimsPrincipal user,
            string fileRelativePath)
            => FileAccessRight.None;

        public void SetAccess(long circleId, string normalizedFullPath,
            FileAccessRight access) { }
    }
}