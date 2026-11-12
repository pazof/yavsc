using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Chat.Calls;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Server.Tests;

[Trait("Category", "non-reg")]
public sealed class CallLanSignalingTests
{
    private static LanCallEndpoint Endpoint(string address = "192.168.1.2", int port = 5004) =>
        new(address, port, new string('a', 32), new string('b', 28));

    private static CallSignalingService Service(RecordingCallHub hub, bool enabled = true) =>
        new(hub, Options.Create(new AudioVideoCallOptions { Enabled = true, EnableLanCalls = enabled }),
            TimeProvider.System, NullLogger<CallSignalingService>.Instance);

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Disabled_lan_or_ip_masking_prevents_invitation(bool enabled, bool relay)
    {
        var hub = new RecordingCallHub();
        using var service = Service(hub, enabled);
        await Assert.ThrowsAsync<HubException>(() =>
            service.InviteAsync("General", "alice", "bob", "a", ["b"], relay, CallTransports.LanSrtp));
        Assert.Empty(hub.Messages);
    }

    [Fact]
    public async Task Ip_masking_activated_before_acceptance_blocks_the_lan_call()
    {
        var hub = new RecordingCallHub();
        using var service = Service(hub);
        var call = await service.InviteAsync("General", "alice", "bob", "a", ["b"], false, CallTransports.LanSrtp);
        await Assert.ThrowsAsync<HubException>(() => service.AcceptAsync(call.Id, "bob", "b", true));
        Assert.DoesNotContain(hub.Messages, m => m.Method == CallHubMethods.Accepted);
    }

    [Theory]
    [InlineData("192.168.1.2")]
    [InlineData("8.8.8.8")]
    [InlineData("2001:db8::1")]
    [InlineData("fd00::1")]
    public async Task Endpoints_are_exchanged_only_once_between_consented_devices_without_ice_credentials(string address)
    {
        var hub = new RecordingCallHub();
        using var service = Service(hub);
        var call = await service.InviteAsync("General", "alice", "bob", "a", ["b", "other"], false, CallTransports.LanSrtp);
        Assert.Equal(CallTransports.LanSrtp, call.Transport);
        var first = new CallSignal("rtp-endpoint", Endpoint: Endpoint(address));
        var second = new CallSignal("rtp-endpoint", Endpoint: Endpoint("10.0.0.2"));
        await Assert.ThrowsAsync<HubException>(() => service.SignalAsync(call.Id, "alice", "a", first));
        await service.AcceptAsync(call.Id, "bob", "b", false);
        Assert.Empty(Assert.IsType<CallAccepted>(hub.Messages.Last().Arguments[0]).IceServers);
        await Assert.ThrowsAsync<HubException>(() => service.SignalAsync(call.Id, "bob", "other", second));
        await service.SignalAsync(call.Id, "alice", "a", first);
        await service.SignalAsync(call.Id, "bob", "b", second);
        var sent = hub.Messages.Where(m => m.Method == CallHubMethods.SignalReceived).ToArray();
        Assert.Equal(2, sent.Length);
        Assert.Equal(["b"], sent[0].Targets);
        Assert.Equal(["a"], sent[1].Targets);
        await Assert.ThrowsAsync<HubException>(() => service.SignalAsync(call.Id, "alice", "a", first));
        await service.EndAsync(call.Id, "alice", "a");
        await Assert.ThrowsAsync<HubException>(() => service.SignalAsync(call.Id, "bob", "b", second));
    }

    [Fact]
    public async Task Lan_transport_rejects_sdp_candidates_and_mixed_endpoint_payloads()
    {
        var hub = new RecordingCallHub();
        using var service = Service(hub);
        var call = await service.InviteAsync("General", "alice", "bob", "a", ["b"], false, CallTransports.LanSrtp);
        await service.AcceptAsync(call.Id, "bob", "b", false);
        CallSignal[] invalid =
        [
            new("offer", "v=0\r\nm=audio 9 UDP/TLS/RTP/SAVPF 111\r\n"),
            new("candidate", Candidate: "candidate:1 1 udp 100 192.168.1.1 5004 typ host"),
            new("rtp-endpoint"), new("rtp-endpoint", Sdp: "injected", Endpoint: Endpoint()),
            new("rtp-endpoint", Endpoint: Endpoint() with { Key = new string('x', 32) }),
            new("rtp-endpoint", Endpoint: Endpoint() with { Salt = "short" }),
            new("rtp-endpoint", Endpoint: Endpoint("255.255.255.255")),
            new("rtp-endpoint", Endpoint: Endpoint(port: 5005))
        ];
        foreach (var signal in invalid)
            await Assert.ThrowsAsync<HubException>(() => service.SignalAsync(call.Id, "alice", "a", signal));
        Assert.DoesNotContain(hub.Messages, m => m.Method == CallHubMethods.SignalReceived);
        Assert.Throws<HubException>(() => CallSignalValidator.Validate(
            new("offer", "v=0\r\nm=audio 9 UDP/TLS/RTP/SAVPF 111\r\n", Endpoint: Endpoint()), false));
    }

    [Theory]
    [InlineData("10.1.2.3", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("172.31.255.254", true)]
    [InlineData("192.168.1.2", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("172.32.0.1", true)]
    [InlineData("169.254.169.254", false)]
    [InlineData("8.8.8.8", true)]
    [InlineData("100.64.0.1", true)]
    [InlineData("223.255.255.254", true)]
    [InlineData("224.0.0.1", false)]
    [InlineData("0.0.0.0", false)]
    [InlineData("0.1.2.3", false)]
    [InlineData("240.0.0.1", false)]
    [InlineData("255.255.255.255", false)]
    [InlineData("169.254.1.1", false)]
    [InlineData("::ffff:8.8.8.8", false)]
    [InlineData("::1", true)]
    [InlineData("2001:db8::1", true)]
    [InlineData("2606:4700:4700::1111", true)]
    [InlineData("fd00::123", true)]
    [InlineData("fc00::1", true)]
    [InlineData("::", false)]
    [InlineData("ff02::1", false)]
    [InlineData("fe80::1", false)]
    [InlineData("fe80::1%2", false)]
    [InlineData("2001:db8::1%2", false)]
    [InlineData("fec0::1", false)]
    [InlineData("2001:DB8::1", false)]
    [InlineData("[2001:db8::1]", false)]
    [InlineData("2001:db8::1,port=80", false)]
    [InlineData("localhost", false)]
    [InlineData("127.1", false)]
    [InlineData("192.168.1.2,port=80", false)]
    public void Only_canonical_supported_unicast_endpoints_are_allowed(string address, bool expected) =>
        Assert.Equal(expected, Endpoint(address).IsValid());

    [Fact]
    public void Credentials_are_not_exposed_by_record_diagnostics()
    {
        var endpoint = Endpoint();
        var diagnostic = new CallSignal("rtp-endpoint", Endpoint: endpoint).ToString();
        Assert.DoesNotContain(endpoint.Key, diagnostic);
        Assert.DoesNotContain(endpoint.Salt, diagnostic);
        Assert.DoesNotContain(endpoint.Address, diagnostic);
    }
}
