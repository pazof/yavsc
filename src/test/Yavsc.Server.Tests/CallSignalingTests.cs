using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Server.Hubs;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Server.Tests;

public sealed class CallSignalingTests
{
    private const string Sdp = "v=0\r\no=- 1 1 IN IP4 127.0.0.1\r\ns=-\r\nt=0 0\r\nm=audio 9 UDP/TLS/RTP/SAVPF 111\r\nc=IN IP4 0.0.0.0\r\n";
    private static CallSignal Candidate(string type = "relay") =>
        new("candidate", Candidate: $"candidate:1 1 udp 100 192.0.2.1 5000 typ {type} raddr 0.0.0.0 rport 0",
            SdpMid: "0", SdpMLineIndex: 0);

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-26T00:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Fixture : IDisposable
    {
        public RecordingCallHub Hub { get; } = new();
        public Clock Clock { get; } = new();
        public AudioVideoCallOptions Options { get; } = new()
        {
            Enabled = true,
            StunUrls = ["stun:stun.example.test:3478"],
            TurnUrls = ["turn:turn.example.test:3478?transport=udp", "turns:turn.example.test:5349?transport=tcp"],
            TurnSharedSecret = "test-only-secret"
        };
        public CallSignalingService Service { get; }
        public Fixture()
        {
            Service = new(Hub, Microsoft.Extensions.Options.Options.Create(Options), Clock,
                NullLogger<CallSignalingService>.Instance);
        }
        public Task<CallSession> Invite(bool relay = false) =>
            Service.InviteAsync("General", "alice", "bob", "alice-device", ["bob-1", "bob-2"], relay);
        public void Dispose() => Service.Dispose();
    }

    [Fact]
    public async Task Calls_are_disabled_by_default()
    {
        using var f = new Fixture();
        f.Options.Enabled = new AudioVideoCallOptions().Enabled;
        await Assert.ThrowsAsync<HubException>(() => f.Invite());
        Assert.Empty(f.Hub.Messages);
    }

    [Fact]
    public async Task Invitation_does_not_grant_media_signaling_or_disclose_turn_credentials()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        Assert.Equal(f.Clock.Now.AddSeconds(45), call.ExpiresAt);
        var message = Assert.Single(f.Hub.Messages);
        Assert.Equal(CallHubMethods.Invitation, message.Method);
        Assert.Equal(["bob-1", "bob-2"], message.Targets);
        Assert.IsType<CallSession>(Assert.Single(message.Arguments));
        await Assert.ThrowsAsync<HubException>(() =>
            f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp)));
    }

    [Fact]
    public async Task First_acceptance_wins_and_other_devices_cannot_signal_or_end_the_call()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        await Assert.ThrowsAsync<HubException>(() => f.Service.AcceptAsync(call.Id, "bob", "bob-2", false));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "bob", "bob-2", Candidate()));
        await Assert.ThrowsAsync<HubException>(() => f.Service.EndAsync(call.Id, "bob", "bob-2"));
        var dismissed = Assert.Single(f.Hub.Messages, m => m.Method == CallHubMethods.Ended);
        Assert.Equal(["bob-2"], dismissed.Targets);
        Assert.Equal("answered-elsewhere", Assert.IsType<CallEnded>(dismissed.Arguments[0]).Reason);
        var ready = Assert.Single(f.Hub.Messages, m => m.Method == CallHubMethods.Accepted);
        Assert.Equal(["alice-device", "bob-1"], ready.Targets);
    }

    [Fact]
    public async Task Concurrent_acceptance_selects_exactly_one_device()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        var attempts = new[] { "bob-1", "bob-2" }.Select(async connection =>
            await Record.ExceptionAsync(() => f.Service.AcceptAsync(call.Id, "bob", connection, false)));
        var results = await Task.WhenAll(attempts);
        Assert.Single(results, r => r == null);
        Assert.Single(results, r => r is HubException);
        Assert.Single(f.Hub.Messages, m => m.Method == CallHubMethods.Accepted);
    }

    [Fact]
    public async Task Privacy_can_only_be_strengthened_on_acceptance_and_turn_credentials_expire()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", true);
        var ready = Assert.IsType<CallAccepted>(
            f.Hub.Messages.Single(m => m.Method == CallHubMethods.Accepted).Arguments[0]);
        Assert.True(ready.Session.RelayOnly);
        var ice = Assert.Single(ready.IceServers);
        Assert.Equal(f.Options.TurnUrls, ice.Urls);
        Assert.NotNull(ice.Username);
        Assert.StartsWith($"{ready.Session.ExpiresAt.AddMinutes(5).ToUnixTimeSeconds()}:", ice.Username);
        Assert.Equal(Convert.ToBase64String(HMACSHA1.HashData(
            Encoding.UTF8.GetBytes(f.Options.TurnSharedSecret!), Encoding.UTF8.GetBytes(ice.Username))), ice.Credential);
        await f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp));
        await Assert.ThrowsAsync<HubException>(() =>
            f.Service.SignalAsync(call.Id, "alice", "alice-device", Candidate("host")));
    }

    [Fact]
    public async Task Relay_invitation_cannot_be_downgraded_and_missing_turn_fails_closed()
    {
        using var f = new Fixture();
        f.Options.TurnUrls = [];
        await Assert.ThrowsAsync<HubException>(() => f.Invite(true));
        Assert.Empty(f.Hub.Messages);
        f.Options.TurnUrls = ["turn:turn.example.test:3478"];
        var call = await f.Invite(true);
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        Assert.True(Assert.IsType<CallAccepted>(f.Hub.Messages.Last().Arguments[0]).Session.RelayOnly);
    }

    [Theory]
    [InlineData("https://turn.example.test")]
    [InlineData("turn:")]
    [InlineData("turn://turn.example.test")]
    [InlineData("turn:user:password@turn.example.test")]
    [InlineData("turn:turn.example.test:0")]
    [InlineData("turn:turn.example.test?transport=invalid")]
    [InlineData("turn:turn.example.test#fragment")]
    public async Task Invalid_turn_configuration_is_rejected_before_inviting(string url)
    {
        using var f = new Fixture();
        f.Options.TurnUrls = [url];
        await Assert.ThrowsAsync<HubException>(() => f.Invite(true));
        Assert.Empty(f.Hub.Messages);
    }

    [Fact]
    public async Task Failed_delivery_does_not_leave_participants_busy()
    {
        using var f = new Fixture();
        f.Hub.FailMethod = CallHubMethods.Invitation;
        await Assert.ThrowsAsync<IOException>(() => f.Invite());
        f.Hub.FailMethod = null;
        var call = await f.Invite();
        f.Hub.FailMethod = CallHubMethods.Accepted;
        await Assert.ThrowsAsync<IOException>(() => f.Service.AcceptAsync(call.Id, "bob", "bob-1", false));
        f.Hub.FailMethod = null;
        Assert.NotEqual(call.Id, (await f.Invite()).Id);
    }

    [Fact]
    public async Task Signaling_is_bound_to_identity_device_consent_and_initial_offer_order()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await Assert.ThrowsAsync<HubException>(() => f.Service.AcceptAsync(call.Id, "mallory", "bob-1", false));
        await Assert.ThrowsAsync<HubException>(() => f.Service.AcceptAsync(call.Id, "bob", "uninvited", false));
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "bob", "bob-1", new("offer", Sdp)));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "bob", "bob-1", new("answer", Sdp)));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "alice", "other-device", new("offer", Sdp)));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "mallory", "alice-device", new("offer", Sdp)));
        await f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp));
        await f.Service.SignalAsync(call.Id, "alice", "alice-device", Candidate("host"));
        await f.Service.SignalAsync(call.Id, "bob", "bob-1", new("answer", Sdp));
        await f.Service.SignalAsync(call.Id, "bob", "bob-1", Candidate("host"));
        var signals = f.Hub.Messages.Where(m => m.Method == CallHubMethods.SignalReceived).ToArray();
        Assert.Equal(4, signals.Length);
        Assert.Equal(["bob-1"], signals[0].Targets);
        Assert.Equal(["alice-device"], signals[2].Targets);
        Assert.All(signals, m => Assert.Equal(call.Id, m.Arguments[0]));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp)));
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "bob", "bob-1", new("answer", Sdp)));
    }

    [Fact]
    public async Task Busy_users_cannot_be_invited_again_and_expiry_releases_them()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await Assert.ThrowsAsync<HubException>(() => f.Invite());
        await Assert.ThrowsAsync<HubException>(() =>
            f.Service.InviteAsync("Other", "carol", "bob", "carol-1", ["bob-1"], false));
        f.Clock.Now = call.ExpiresAt;
        await f.Service.ExpireAsync();
        Assert.Equal("expired", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
        await Assert.ThrowsAsync<HubException>(() => f.Service.AcceptAsync(call.Id, "bob", "bob-1", false));
        Assert.NotEqual(call.Id, (await f.Invite()).Id);
    }

    [Fact]
    public async Task Unselected_device_disconnect_does_not_end_an_accepted_call()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        await f.Service.DisconnectAsync("bob-2");
        await f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp));
        await f.Service.DisconnectAsync("bob-1");
        Assert.Equal("disconnected", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
        await Assert.ThrowsAsync<HubException>(() =>
            f.Service.SignalAsync(call.Id, "alice", "alice-device", Candidate()));
    }

    [Fact]
    public async Task Decline_hangup_room_departure_and_profile_change_release_the_call()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await Assert.ThrowsAsync<HubException>(() => f.Service.EndAsync(call.Id, "mallory", "mallory-1"));
        await f.Service.EndAsync(call.Id, "bob", "bob-2");
        Assert.Equal("cancelled-or-declined", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
        call = await f.Invite();
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        await f.Service.EndAsync(call.Id, "alice", "alice-device");
        Assert.Equal("hangup", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
        call = await f.Invite();
        await f.Service.EndForUserAsync("bob", "left-room", "Other");
        Assert.Equal(call.Id, (await f.Service.GetSessionAsync(call.Id, "alice", "alice-device")).Id);
        await f.Service.EndForUserAsync("bob", "left-room", "General");
        Assert.Equal("left-room", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
        await f.Invite();
        await f.Service.EndForUserAsync("alice", "profile-changed");
        Assert.Equal("profile-changed", Assert.IsType<CallEnded>(f.Hub.Messages.Last().Arguments[0]).Reason);
    }

    [Fact]
    public async Task Candidate_count_is_bounded()
    {
        using var f = new Fixture();
        var call = await f.Invite();
        await f.Service.AcceptAsync(call.Id, "bob", "bob-1", false);
        await f.Service.SignalAsync(call.Id, "alice", "alice-device", new("offer", Sdp));
        for (var i = 0; i < 256; i++)
            await f.Service.SignalAsync(call.Id, "alice", "alice-device", Candidate());
        await Assert.ThrowsAsync<HubException>(() => f.Service.SignalAsync(call.Id, "alice", "alice-device", Candidate()));
    }

    [Theory]
    [InlineData("candidate:1 1 udp 100 192.0.2.1 5000 typ host")]
    [InlineData("candidate:1 1 udp 100 192.0.2.1 5000 typ host typ relay")]
    [InlineData("candidate:1 1 udp 100 192.0.2.1 5000 typ relay raddr 10.0.0.1 rport 5000")]
    public void Relay_policy_rejects_direct_forged_and_related_addresses(string candidate)
    {
        Assert.Throws<HubException>(() =>
            CallSignalValidator.Validate(new("candidate", Candidate: candidate, SdpMid: "0", SdpMLineIndex: 0), true));
        Assert.Throws<HubException>(() =>
            CallSignalValidator.Validate(new("offer", Sdp + "a=" + candidate + "\r\n"), true));
    }

    [Theory]
    [InlineData("o=- 1 1 IN IP4 10.0.0.1\r\n")]
    [InlineData("c=IN IP4 10.0.0.1\r\n")]
    [InlineData("a=rtcp:5000 IN IP4 10.0.0.1\r\n")]
    public void Relay_sdp_must_not_expose_a_direct_address(string line) =>
        Assert.Throws<HubException>(() => CallSignalValidator.Validate(new("offer", Sdp + line), true));

    [Fact]
    public void Signal_validation_enforces_exact_shapes_and_size_limits()
    {
        CallSignalValidator.Validate(new("offer", Sdp), true);
        CallSignalValidator.Validate(Candidate(), true);
        CallSignal[] invalid =
        [
            new("bogus"), new("offer", "v=0"), new("offer", Sdp + new string('x', 24000)),
            new("offer", Sdp + new string('\u00e9', 12000)), new("offer", Sdp + "a=ignored\nc=IN IP4 10.0.0.1"),
            new("answer", Sdp, "unexpected"), Candidate() with { SdpMid = null },
            Candidate() with { SdpMLineIndex = -1 }, Candidate() with { SdpMLineIndex = 17 },
            Candidate() with { Candidate = new string('x', 2049) }, Candidate() with { Sdp = Sdp },
            Candidate() with { Candidate = Candidate().Candidate + "\r\ninjected" }
        ];
        foreach (var signal in invalid) Assert.Throws<HubException>(() => CallSignalValidator.Validate(signal, false));
    }
}

internal sealed class RecordingCallHub : IHubContext<ChatHub>, IHubClients
{
    internal sealed record Message(string[] Targets, string Method, object?[] Arguments);
    public List<Message> Messages { get; } = [];
    public string? FailMethod { get; set; }
    IHubClients IHubContext<ChatHub>.Clients => this;
    IGroupManager IHubContext<ChatHub>.Groups => throw new NotSupportedException();
    private sealed class Proxy(RecordingCallHub hub, string[] targets) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            if (hub.FailMethod == method) throw new IOException("Test delivery failed.");
            hub.Messages.Add(new(targets, method, args));
            return Task.CompletedTask;
        }
    }
    public IClientProxy Client(string id) => new Proxy(this, [id]);
    public IClientProxy Clients(IReadOnlyList<string> ids) => new Proxy(this, ids.ToArray());
    public IClientProxy All => throw new NotSupportedException();
    public IClientProxy AllExcept(IReadOnlyList<string> ids) => throw new NotSupportedException();
    public IClientProxy Group(string name) => throw new NotSupportedException();
    public IClientProxy GroupExcept(string name, IReadOnlyList<string> ids) => throw new NotSupportedException();
    public IClientProxy Groups(IReadOnlyList<string> names) => throw new NotSupportedException();
    public IClientProxy User(string id) => throw new NotSupportedException();
    public IClientProxy Users(IReadOnlyList<string> ids) => throw new NotSupportedException();
}
