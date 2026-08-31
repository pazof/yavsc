using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Yavsc.Controllers;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test.Fixtures;

public sealed class ApiWebServerFixture : WebHostFixture
{
    protected override int HttpsPort => 5104;

    private static SqliteConnection? _sharedSqliteConnection;
    private static readonly object _sqliteLock = new();

    protected override WebApplication BuildApp(WebApplicationBuilder builder)
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

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ActivityApiController).Assembly);

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

    public void ResetAndSeedActivityGraph()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Database.EnsureDeleted();
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
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = location,
            Reason = "Initial rendez-vous",
            Status = Yavsc.QueryStatus.Inserted,
        });

        db.SaveChanges();
    }

    public override void Dispose()
    {
        try
        {
            base.Dispose();
        }
        finally
        {
            lock (_sqliteLock)
            {
                if (_sharedSqliteConnection is not null)
                {
                    _sharedSqliteConnection.Close();
                    _sharedSqliteConnection.Dispose();
                    _sharedSqliteConnection = null;
                }
            }
        }
    }
}
