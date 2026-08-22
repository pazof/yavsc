using IdentityServer8.EntityFramework.DbContexts;
using IdentityServer8.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Yavsc.Org.Tests.Controllers;

/// <summary>
/// Regression sentinel: two <see cref="TestWebApplicationFactory"/>
/// instances must not see each other's clients.
///
/// EF Core's <c>UseInMemoryDatabase(name)</c> returns the same
/// backing store to every <c>DbContext</c> that asks for it under
/// the same name, in the same process. Before the per-fixture GUID
/// fix, both <see cref="TestWebApplicationFactory"/> and
/// <see cref="WebServerFixture"/> used the bare <c>"InMemory"</c>
/// connection string, so every fixture shared one store and tests
/// were silently order-dependent.
///
/// We assert against <see cref="ConfigurationDbContext"/> directly
/// rather than via <c>IClientStore</c>: the validating wrapper around
/// <c>IClientStore</c> raises events through <c>IEventService</c>,
/// which is not registered in the test host and crashes with a
/// <c>NullReferenceException</c> before it can return a result. Going
/// straight to the DbContext is the same code path the production
/// code uses, so it is the right surface to assert against.
/// </summary>
public class TestWebApplicationFactoryIsolationTests
{
    [Fact]
    public async Task Second_factory_does_not_see_clients_seeded_into_first()
    {
        var marker = $"marker-A-{Guid.NewGuid():N}";

        // First factory: seed a distinctive client.
        using (var first = new TestWebApplicationFactory())
        {
            await using var scope = first.Services.CreateAsyncScope();
            var configDb = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var firstCs = scope.ServiceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("YavscConnection");
            Assert.StartsWith("InMemory-", firstCs);
            configDb.Clients.Add(new Client { ClientId = marker, ClientName = "marker-A" });
            await configDb.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Sanity: the first factory can see its own seed.
            var seenByFirst = await configDb.Clients
                .AsNoTracking()
                .AnyAsync(c => c.ClientId == marker, TestContext.Current.CancellationToken);
            Assert.True(seenByFirst);
        }

        // Second factory: must start from a clean slate. If the
        // in-memory store leaked from the first factory, this
        // assertion fails.
        using var second = new TestWebApplicationFactory();
        await using var secondScope = second.Services.CreateAsyncScope();
        var secondCs = secondScope.ServiceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("YavscConnection");
        Assert.StartsWith("InMemory-", secondCs);
        var secondDb = secondScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var seenBySecond = await secondDb.Clients
            .AsNoTracking()
            .AnyAsync(c => c.ClientId == marker, TestContext.Current.CancellationToken);
        Assert.False(seenBySecond);
    }
}
