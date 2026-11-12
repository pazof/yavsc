using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
[Collection("Yavsc Api")]
public sealed class ApiWebServerFixtureTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    [Fact]
    public void Haircut_reset_seeds_catalog_and_both_command_types()
    {
        for (var reset = 0; reset < 2; reset++)
        {
            fixture.ResetAndSeedHaircutGraph();
            using var scope = fixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(3, db.HairPrestation.Count());
            var haircut = Assert.Single(db.HairCutQueries.Include(query => query.Prestation));
            Assert.Equal("brush", haircut.ActivityCode);
            Assert.Equal("alice", haircut.PerformerId);
            Assert.NotNull(haircut.Prestation);
            var grouped = Assert.Single(db.HairMultiCutQueries.Include(query => query.Prestations));
            Assert.Equal("mbrush", grouped.ActivityCode);
            Assert.Equal("alice", grouped.PerformerId);
            Assert.Equal(2, grouped.Prestations.Count);
        }
    }

    [Fact]
    public void Repeated_reset_removes_previous_data_and_reseeds_unique_users()
    {
        fixture.ResetAndSeedActivityGraph();
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.Add(new ApplicationUser
            {
                Id = "previous-test-user",
                UserName = "previous-test-user",
                Email = "previous-test-user@example.test",
            });
            db.SaveChanges();
        }

        for (var reset = 0; reset < 2; reset++)
        {
            fixture.ResetAndSeedActivityGraph();
            using var scope = fixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(new[] { "alice", "bob" }, db.Users.OrderBy(user => user.Id).Select(user => user.Id).ToArray());
            Assert.Equal(2, db.Performers.Count());
            Assert.Single(db.Locations);

            if (db.Database.IsSqlite())
            {
                db.Database.OpenConnection();
                try
                {
                    using var command = db.Database.GetDbConnection().CreateCommand();
                    command.CommandText = "PRAGMA foreign_keys;";
                    Assert.Equal(1L, command.ExecuteScalar());
                }
                finally
                {
                    db.Database.CloseConnection();
                }
            }
        }
    }
}
