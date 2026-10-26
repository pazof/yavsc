using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Models;
using Yavsc.Server.Hubs;
using Yavsc.Services;
using Yavsc.Settings;
using Yavsc.ViewModels.Chat;

namespace Yavsc.Server.Tests;

public sealed class CallHubTests
{
    private sealed class Fixture : IDisposable
    {
        public ApplicationDbContext Db { get; } = new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        public RecordingCallHub Delivery { get; } = new();
        public Connections Connections { get; } = new();
        public CallSignalingService Calls { get; }
        private readonly ServiceProvider _services;

        public Fixture()
        {
            _services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
            Calls = new(Delivery, Options.Create(new AudioVideoCallOptions
            {
                Enabled = true, TurnUrls = ["turn:turn.example.test:3478"], TurnSharedSecret = "test-only"
            }), TimeProvider.System, NullLogger<CallSignalingService>.Instance);
            Db.Users.AddRange(
                new ApplicationUser { Id = "alice-id", UserName = "alice", Email = "alice@example.test" },
                new ApplicationUser { Id = "bob-id", UserName = "bob", Email = "bob@example.test" });
            Db.SaveChanges();
            Connections.OnConnected("alice-1", "alice", false);
            Connections.OnConnected("bob-1", "bob", false);
        }

        public ChatHub Hub(string user, string connection) => new(Db,
            _services.GetRequiredService<ILoggerFactory>(),
            _services.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizerFactory>(),
            Connections, Calls) { Context = new Caller(user, connection) };

        public void Dispose()
        {
            Calls.Dispose();
            Db.Dispose();
            _services.Dispose();
        }
    }

    [Fact]
    public async Task Invitation_requires_existing_accounts_presence_and_the_authenticated_connection()
    {
        using var f = new Fixture();
        using var hub = f.Hub("alice", "alice-1");
        f.Connections.Present.Remove("bob");
        await Assert.ThrowsAsync<HubException>(() => hub.InviteCall("General", "bob"));
        f.Connections.Present.Add("bob");
        using var spoof = f.Hub("bob", "alice-1");
        await Assert.ThrowsAsync<HubException>(() => spoof.InviteCall("General", "alice"));
        await Assert.ThrowsAsync<HubException>(() => hub.InviteCall("General", "alice"));
        Assert.Empty(f.Delivery.Messages);
        f.Db.Users.Remove(await f.Db.Users.SingleAsync(u => u.Id == "bob-id", TestContext.Current.CancellationToken));
        await f.Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<HubException>(() => hub.InviteCall("General", "bob"));
    }

    [Fact]
    public async Task Persistent_preference_is_read_at_invitation_and_rechecked_at_acceptance()
    {
        using var f = new Fixture();
        using var caller = f.Hub("alice", "alice-1");
        using var callee = f.Hub("bob", "bob-1");
        var call = await caller.InviteCall("General", "bob");
        Assert.False(call.RelayOnly);
        var bob = await f.Db.Users.SingleAsync(u => u.Id == "bob-id", TestContext.Current.CancellationToken);
        bob.HideIpAddressInCalls = true;
        await f.Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await callee.AcceptCall(call.Id);
        Assert.True(Assert.IsType<CallAccepted>(f.Delivery.Messages.Last().Arguments[0]).Session.RelayOnly);
        await caller.EndCall(call.Id);
        Assert.True((await caller.InviteCall("General", "bob")).RelayOnly);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Blacklisting_either_participant_prevents_invitations(bool reverse)
    {
        using var f = new Fixture();
        f.Db.BlackListed.Add(new()
        {
            OwnerId = reverse ? "alice-id" : "bob-id",
            UserId = reverse ? "bob-id" : "alice-id"
        });
        await f.Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var hub = f.Hub("alice", "alice-1");
        await Assert.ThrowsAsync<HubException>(() => hub.InviteCall("General", "bob"));
        Assert.Empty(f.Delivery.Messages);
    }

    [Fact]
    public async Task Leaving_the_room_prevents_acceptance_and_privacy_change_prevents_direct_signaling()
    {
        using var f = new Fixture();
        using var caller = f.Hub("alice", "alice-1");
        using var callee = f.Hub("bob", "bob-1");
        var call = await caller.InviteCall("General", "bob");
        f.Connections.Present.Remove("bob");
        await Assert.ThrowsAsync<HubException>(() => callee.AcceptCall(call.Id));
        f.Connections.Present.Add("bob");
        await callee.AcceptCall(call.Id);
        var bob = await f.Db.Users.SingleAsync(u => u.Id == "bob-id", TestContext.Current.CancellationToken);
        bob.HideIpAddressInCalls = true;
        await f.Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<HubException>(() => caller.SendCallSignal(call.Id,
            new("offer", "v=0\r\nm=audio 9 UDP/TLS/RTP/SAVPF 111\r\n")));
        Assert.DoesNotContain(f.Delivery.Messages, m => m.Method == CallHubMethods.SignalReceived);
        Assert.Equal("privacy-changed", Assert.IsType<CallEnded>(f.Delivery.Messages.Last().Arguments[0]).Reason);
    }

    private sealed class Caller(string name, string connection) : HubCallerContext
    {
        public override string ConnectionId => connection;
        public override string? UserIdentifier => name;
        public override ClaimsPrincipal User { get; } = new(new ClaimsIdentity(
            [new Claim("name", name)], "test"));
        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override CancellationToken ConnectionAborted => CancellationToken.None;
        public override void Abort() { }
    }

    private sealed class Connections : IConnexionManager
    {
        public HashSet<string> Present { get; } = ["alice", "bob"];
        private readonly Dictionary<string, string> _names = [];
        public string GetUserName(string cxId) => _names.GetValueOrDefault(cxId)!;
        public bool IsConnected(string user) => _names.ContainsValue(user);
        public bool IsPresent(string room, string user) => room == "General" && Present.Contains(user);
        public IEnumerable<string> GetConnexionIds(string user) => _names.Where(n => n.Value == user).Select(n => n.Key);
        public void OnConnected(string id, string user, bool isCop) => _names[id] = user;
        public void OnDisconnected(string id) => _names.Remove(id);
        public void SetErrorHandler(Action<string, string> errorHandler) { }
        public ChatRoomInfo Join(string room, string id) => throw new NotSupportedException();
        public bool Part(string id, string room, string reason) => throw new NotSupportedException();
        public bool Kick(string id, string user, string room, string reason) => throw new NotSupportedException();
        public bool Op(string room, string user) => throw new NotSupportedException();
        public bool DeOp(string room, string user) => throw new NotSupportedException();
        public bool Hop(string room, string user) => throw new NotSupportedException();
        public bool DeHop(string room, string user) => throw new NotSupportedException();
        public bool TryGetChanInfo(string room, out ChatRoomInfo info) => throw new NotSupportedException();
        public IEnumerable<ChannelShortInfo> ListChannels(string pattern) => throw new NotSupportedException();
    }
}
