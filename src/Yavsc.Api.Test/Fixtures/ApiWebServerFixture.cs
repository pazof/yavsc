using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Yavsc.Controllers;
using Yavsc.Interfaces.Workflow;
using Yavsc.Models;
using Yavsc.Models.Google.Messaging;
using Yavsc.Models.Haircut;
using Yavsc.Models.Messaging;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;
using Yavsc.Services;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test.Fixtures;

public sealed class ApiWebServerFixture : WebHostFixture
{
    private const string DbProviderEnvVar = "YAVSC_API_TEST_DB_PROVIDER";
    private const string NpgsqlAdminConnectionEnvVar = "YAVSC_API_TEST_NPGSQL_ADMIN_CONNECTION";
    private const string DedicatedNpgsqlDatabaseName = "yavscTestDb";
    private const string DefaultDevelopmentConnectionString = "Server=localhost;Port=5432;Database=yavscdev;Username=yavscdev;Password=8*5idas;Include Error Detail=true";

    protected override int HttpsPort => 5104;

    private static SqliteConnection? _sharedSqliteConnection;
    private static readonly object _sqliteLock = new();
    private static readonly object _npgsqlLock = new();
    private static string? _sharedNpgsqlConnectionString;

    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        if (UseNpgsqlProvider())
        {
            var npgsqlConnectionString = EnsureNpgsqlDatabaseCreated();
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseNpgsql(npgsqlConnectionString));
        }
        else
        {
            SqliteConnection sharedConnection;
            lock (_sqliteLock)
            {
                if (_sharedSqliteConnection is null)
                {
                    _sharedSqliteConnection = new SqliteConnection(
                        "Data Source=YavscApiTests;Mode=Memory;Cache=Shared");
                    _sharedSqliteConnection.Open();
                }
                sharedConnection = _sharedSqliteConnection;
            }

            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlite(sharedConnection));
        }

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ActivityApiController).Assembly);

        builder.Services.AddLocalization();
        builder.Services.Configure<GoogleAuthSettings>(_ => { });
        builder.Services.AddTransient<IBillingService, BillingService>();
        builder.Services.AddTransient<IYavscMessageSender, NoopMessageSender>();
        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.IncludeErrorDetails = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = TestTokenIssuer.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestTokenIssuer.SigningKey,
                    NameClaimType = "sub",
                    RoleClaimType = Yavsc.Constants.RoleClaimType,
                };
            });

        return builder.Build();
    }

    protected override async Task<WebApplication> ConfigurePipelineAsync(WebApplication app)
    {
        app.UseDeveloperExceptionPage();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }

        await Task.CompletedTask;
        return app;
    }

    public string BaseAddress => Addresses.First(a => a.StartsWith("https://", StringComparison.Ordinal));

    public static bool UseNpgsqlProvider()
        => string.Equals(
            Environment.GetEnvironmentVariable(DbProviderEnvVar),
            "npgsql",
            StringComparison.OrdinalIgnoreCase);

    private static string EnsureNpgsqlDatabaseCreated()
    {
        lock (_npgsqlLock)
        {
            if (!string.IsNullOrWhiteSpace(_sharedNpgsqlConnectionString))
            {
                return _sharedNpgsqlConnectionString;
            }

            var adminConnectionString = BuildAdminConnectionString();
            var databaseName = DedicatedNpgsqlDatabaseName;

            using (var adminConnection = new NpgsqlConnection(adminConnectionString))
            {
                adminConnection.Open();
                using var existsCommand = adminConnection.CreateCommand();
                existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
                existsCommand.Parameters.AddWithValue("databaseName", databaseName);

                if (existsCommand.ExecuteScalar() is null)
                {
                    using var createCommand = adminConnection.CreateCommand();
                    createCommand.CommandText = $"CREATE DATABASE \"{databaseName}\"";
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
        var configured = Environment.GetEnvironmentVariable(NpgsqlAdminConnectionEnvVar);
        var source = string.IsNullOrWhiteSpace(configured)
            ? DefaultDevelopmentConnectionString
            : configured;

        var builder = new NpgsqlConnectionStringBuilder(source)
        {
            Pooling = false,
            IncludeErrorDetail = true
        };

        if (string.IsNullOrWhiteSpace(configured))
        {
            builder.Database = "postgres";
        }
        else if (string.IsNullOrWhiteSpace(builder.Database))
        {
            builder.Database = "postgres";
        }

        return builder.ToString();
    }

    private sealed class NoopMessageSender : IYavscMessageSender
    {
        public Task<MessageWithPayloadResponse> NotifyBookQueryAsync(IEnumerable<string> connectionIds, RdvQueryEvent ev)
            => Task.FromResult(new MessageWithPayloadResponse());

        public Task<MessageWithPayloadResponse> NotifyEstimateAsync(IEnumerable<string> connectionIds, EstimationEvent ev)
            => Task.FromResult(new MessageWithPayloadResponse());

        public Task<MessageWithPayloadResponse> NotifyHairCutQueryAsync(IEnumerable<string> connectionIds, HairCutQueryEvent ev)
            => Task.FromResult(new MessageWithPayloadResponse());

        public Task<MessageWithPayloadResponse> NotifyAsync(IEnumerable<string> connectionIds, IEvent yaev)
            => Task.FromResult(new MessageWithPayloadResponse());
    }

    public void ResetAndSeedActivityGraph()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        ResetDatabase(db);
        db.Database.EnsureCreated();

        var user = new ApplicationUser
        {
            Id = "alice",
            UserName = "alice",
            Email = "alice@example.test",
            EmailConfirmed = true,
            FullName = "Alice",
        };
        db.Users.Add(user);

        var location = new Location
        {
            Address = "1 rue du Test",
            Latitude = 48.8566,
            Longitude = 2.3522,
        };
        db.Add(location);
        db.SaveChanges();

        var activity = new Activity
        {
            Code = "dev",
            Name = "Dev",
            Hidden = false,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.Activities.Add(activity);

        db.Activities.Add(new Activity
        {
            Code = "ghost",
            Name = "Ghost",
            Hidden = false,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        });

        var performer = new PerformerProfile
        {
            PerformerId = "alice",
            SIREN = "123456789",
            OrganizationAddressId = location.Id,
            AcceptNotifications = true,
            AcceptPublicContact = true,
            Active = true,
            Rate = 5,
            WebSite = "https://alice.dev",
        };
        db.Performers.Add(performer);

        db.UserActivities.Add(new UserActivity
        {
            UserId = "alice",
            DoesCode = "dev",
            Weight = 100,
        });

        db.Users.Add(new ApplicationUser
        {
            Id = "bob",
            UserName = "bob",
            Email = "bob@example.test",
            EmailConfirmed = true,
            FullName = "Bob",
        });

        db.Activities.Add(new Activity
        {
            Code = "declared-only",
            Name = "Declared Only",
            Hidden = false,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        });

        db.Performers.Add(new PerformerProfile
        {
            PerformerId = "bob",
            SIREN = "987654321",
            OrganizationAddressId = location.Id,
            AcceptNotifications = false,
            AcceptPublicContact = false,
            Active = false,
            Rate = 0,
            WebSite = "",
        });

        db.UserActivities.Add(new UserActivity
        {
            UserId = "bob",
            DoesCode = "declared-only",
            Weight = 10,
        });

        db.SaveChanges();
    }

    private static void ResetDatabase(ApplicationDbContext db)
    {
        if (UseNpgsqlProvider())
        {
                db.Set<UserActivity>().RemoveRange(db.Set<UserActivity>());
                db.Set<Activity>().RemoveRange(db.Set<Activity>());
                db.Set<PerformerProfile>().RemoveRange(db.Set<PerformerProfile>());
                db.Set<ApplicationUser>().RemoveRange(db.Set<ApplicationUser>());
                db.Set<Location>().RemoveRange(db.Set<Location>());
                db.Set<RdvQuery>().RemoveRange(db.Set<RdvQuery>());
                db.Set<HairCutQuery>().RemoveRange(db.Set<HairCutQuery>());
                db.Set<HairMultiCutQuery>().RemoveRange(db.Set<HairMultiCutQuery>());
                db.Set<HairPrestation>().RemoveRange(db.Set<HairPrestation>());
                db.Set<HairPrestationCollectionItem>().RemoveRange(db.Set<HairPrestationCollectionItem>());
           
            db.SaveChanges();
            return;
        }

        db.Database.EnsureDeleted();
    }

   

    private static IReadOnlyList<IEntityType> GetDeletionOrder(IModel model)
    {
        var entityTypes = model
            .GetEntityTypes()
            .Where(et =>
                et.ClrType is not null &&
                !et.IsOwned() &&
                et.FindPrimaryKey() is not null)
            .ToArray();

        var included = new HashSet<IEntityType>(entityTypes);
        var dependencies = new Dictionary<IEntityType, HashSet<IEntityType>>();

        foreach (var entityType in entityTypes)
        {
            var principals = entityType
                .GetForeignKeys()
                .Where(fk => !fk.IsOwnership)
                .Select(fk => fk.PrincipalEntityType)
                .Where(included.Contains)
                .ToHashSet();

            dependencies[entityType] = principals;
        }

        var queue = new Queue<IEntityType>(
            dependencies.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key));

        var order = new List<IEntityType>(entityTypes.Length);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!order.Contains(current))
            {
                order.Add(current);
            }

            foreach (var kvp in dependencies)
            {
                if (!kvp.Value.Remove(current) || kvp.Value.Count != 0)
                {
                    continue;
                }

                if (!order.Contains(kvp.Key) && !queue.Contains(kvp.Key))
                {
                    queue.Enqueue(kvp.Key);
                }
            }
        }

        // If cycles remain (rare), append unresolved types last and rely on DB cascades.
        foreach (var entityType in entityTypes)
        {
            if (!order.Contains(entityType))
            {
                order.Add(entityType);
            }
        }

        return order;
    }

    public void ResetAndSeedRdvQueryGraph()
    {
        ResetAndSeedActivityGraph();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var location = db.Locations.Single(l => l.Address == "1 rue du Test");

        db.RdvQueries.Add(new RdvQuery
        {
            ActivityCode = "dev",
            ClientId = "alice",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "alice",
            UserModified = "alice",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = location,
            Reason = "Initial rendez-vous",
            Status = Yavsc.QueryStatus.Inserted,
        });

        db.SaveChanges();
    }

    public void ResetAndSeedHaircutGraph()
    {
        ResetAndSeedActivityGraph();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var location = db.Locations.Single(l => l.Address == "1 rue du Test");

        if (!db.Activities.Any(a => a.Code == "brush"))
        {
            db.Activities.Add(new Activity
            {
                Code = "brush",
                Name = "Brush",
                Hidden = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
        }

        if (!db.Activities.Any(a => a.Code == "mbrush"))
        {
            db.Activities.Add(new Activity
            {
                Code = "mbrush",
                Name = "MBrush",
                Hidden = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
        }

        if (!db.BrusherProfile.Any(p => p.UserId == "alice"))
        {
            db.BrusherProfile.Add(new BrusherProfile
            {
                UserId = "alice",
                ActionDistance = 25,
                WomenLongCutPrice = 50m,
                WomenHalfCutPrice = 40m,
                WomenShortCutPrice = 30m,
                ManCutPrice = 20m,
                KidCutPrice = 15m,
                ShampooPrice = 5m,
            });
        }

        var prestation1 = new HairPrestation
        {
            Gender = HairCutGenders.Women,
            Length = HairLength.HalfLong,
            Cut = true,
            Shampoo = true,
            Dressing = HairDressings.Brushing,
            Tech = HairTechnos.NoTech,
            Cares = false,
            Taints = new List<HairTaintInstance>(),
        };
        var prestation2 = new HairPrestation
        {
            Gender = HairCutGenders.Man,
            Length = HairLength.Short,
            Cut = true,
            Shampoo = false,
            Dressing = HairDressings.Brushing,
            Tech = HairTechnos.NoTech,
            Cares = false,
            Taints = new List<HairTaintInstance>(),
        };

        db.HairPrestation.AddRange(prestation1, prestation2);
        db.SaveChanges();

        db.HairCutQueries.Add(new HairCutQuery
        {
            ActivityCode = "brush",
            ClientId = "alice",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "alice",
            UserModified = "alice",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            EventDate = DateTime.UtcNow.AddDays(3),
            Location = location,
            PrestationId = prestation1.Id,
            Prestation = prestation1,
            AdditionalInfo = "Coupe test",
            Status = Yavsc.QueryStatus.Inserted,
            Description = "Haircut seed",
        });

        db.HairMultiCutQueries.Add(new HairMultiCutQuery
        {
            ActivityCode = "mbrush",
            ClientId = "alice",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "alice",
            UserModified = "alice",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            EventDate = DateTime.UtcNow.AddDays(4),
            Location = location,
            Prestations = new List<HairPrestationCollectionItem>
            {
                new() { PrestationId = prestation1.Id, Prestation = prestation1 },
                new() { PrestationId = prestation2.Id, Prestation = prestation2 },
            },
            Status = Yavsc.QueryStatus.Inserted,
        });

        db.SaveChanges();
    }

    public override void Dispose()
    {
        // Keep the shared in-memory SQLite connection alive for the
        // whole test process. Closing it from one fixture instance can
        // drop the schema while other tests are still running.
        base.Dispose();
    }
}
