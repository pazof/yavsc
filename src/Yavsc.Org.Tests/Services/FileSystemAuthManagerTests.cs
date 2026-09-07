using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Relationship;
using Yavsc.Services;

namespace Yavsc.Org.Tests.Services;

public class FileSystemAuthManagerTests
{
    [Fact]
    public void SetAccess_creates_acl_row_with_owner_path_and_flags()
    {
        using var scope = CreateScope();
        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.Read | FileAccessRight.Write);

        var row = scope.Db.CircleAuthorizationToFile.Single();

        Assert.Equal(scope.Circle.Id, row.CircleId);
        Assert.Equal("alice/documents/report.txt", row.Path);
        Assert.Equal("alice", row.OwnerId);
        Assert.Equal(FileAccessRight.Read | FileAccessRight.Write, row.Access);
    }

    [Fact]
    public void SetAccess_updates_existing_acl_row_without_duplicates()
    {
        using var scope = CreateScope();

        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.Read);
        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.Write);

        var rows = scope.Db.CircleAuthorizationToFile.ToList();

        Assert.Single(rows);
        Assert.Equal(FileAccessRight.Write, rows[0].Access);
    }

    [Fact]
    public void SetAccess_none_removes_existing_acl_row()
    {
        using var scope = CreateScope();

        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.Read);
        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.None);

        Assert.Empty(scope.Db.CircleAuthorizationToFile);
    }

    [Fact]
    public void SetAccess_ignores_unknown_owner_prefix()
    {
        using var scope = CreateScope();

        scope.Service.SetAccess(scope.Circle.Id, "unknown/documents/report.txt", FileAccessRight.Read);

        Assert.Empty(scope.Db.CircleAuthorizationToFile);
    }

    [Fact]
    public void Deleting_circle_cascades_file_acl_rows()
    {
        using var scope = CreateScope();

        scope.Service.SetAccess(scope.Circle.Id, "alice/documents/report.txt", FileAccessRight.Read);
        scope.Db.Circle.Remove(scope.Circle);
        scope.Db.SaveChanges();

        Assert.Empty(scope.Db.CircleAuthorizationToFile);
    }

    private static TestScope CreateScope()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        db.Users.Add(new ApplicationUser
        {
            Id = "alice",
            UserName = "alice",
            Email = "alice@example.test"
        });
        db.SaveChanges();

        var circle = new Circle
        {
            OwnerId = "alice",
            Name = "shared",
            Public = false
        };

        db.Circle.Add(circle);
        db.SaveChanges();

        var service = new FileSystemAuthManager(db, Options.Create(new SiteSettings()));
        return new TestScope(connection, db, service, circle);
    }

    private sealed class TestScope : IDisposable
    {
        public TestScope(SqliteConnection connection, ApplicationDbContext db, FileSystemAuthManager service, Circle circle)
        {
            Connection = connection;
            Db = db;
            Service = service;
            Circle = circle;
        }

        public SqliteConnection Connection { get; }
        public ApplicationDbContext Db { get; }
        public FileSystemAuthManager Service { get; }
        public Circle Circle { get; }

        public void Dispose()
        {
            Db.Dispose();
            Connection.Dispose();
        }
    }
}
