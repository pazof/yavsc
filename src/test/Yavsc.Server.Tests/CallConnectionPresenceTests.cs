using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Services;

namespace Yavsc.Server.Tests;

[Trait("Category", "non-reg")]
public sealed class CallConnectionPresenceTests
{
    [Fact]
    public void Disconnecting_an_unselected_device_preserves_the_accounts_chat_presence()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization()
            .AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .BuildServiceProvider();
        var manager = new HubConnectionManager(services.GetRequiredService<IServiceScopeFactory>());
        var user = "call-" + Guid.NewGuid().ToString("N");
        var room = "call-" + Guid.NewGuid().ToString("N");
        var selected = Guid.NewGuid().ToString("N");
        var other = Guid.NewGuid().ToString("N");
        manager.OnConnected(selected, user, false);
        manager.OnConnected(other, user, false);
        manager.Join(room, selected);
        try
        {
            var snapshot = manager.GetConnexionIds(user);
            manager.OnDisconnected(other);
            Assert.True(manager.IsConnected(user));
            Assert.True(manager.IsPresent(room, user));
            Assert.Equal([selected], manager.GetConnexionIds(user));
            Assert.Equal(2, snapshot.Count());
        }
        finally
        {
            manager.OnDisconnected(selected);
            HubConnectionManager.Channels.TryRemove(room, out _);
        }
    }
}
