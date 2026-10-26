using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using PostIt.Services;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Tests;

public sealed class CallSignalingClientTests
{
    [Fact]
    public async Task All_operations_fail_explicitly_without_a_connected_chat_transport()
    {
        await using var connection = new HubConnectionBuilder().WithUrl("http://127.0.0.1:1/chathub").Build();
        using var client = new CallSignalingClient(connection);
        var id = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.InviteAsync("General", "bob", ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.InviteLanAsync("General", "bob", ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.AcceptAsync(id, ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.EndAsync(id, ct));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(id, new("offer", "sdp"), ct));
    }

    [Fact]
    public async Task Disposing_call_subscriptions_does_not_dispose_the_chat_connection()
    {
        await using var connection = new HubConnectionBuilder().WithUrl("http://127.0.0.1:1/chathub").Build();
        var client = new CallSignalingClient(connection);
        client.Dispose();
        client.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.AcceptAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.Equal(HubConnectionState.Disconnected, connection.State);
        using var replacement = new CallSignalingClient(connection);
        await Assert.ThrowsAsync<InvalidOperationException>(() => replacement.InviteAsync("General", "bob", TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Shared_contracts_round_trip_with_signalr_web_json_naming()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var call = new CallSession(Guid.NewGuid(), "General", "alice", "bob", true,
            DateTimeOffset.Parse("2026-10-26T01:00:00Z"));
        var ready = new CallAccepted(call, [new(["turns:turn.example.test:5349"], "expires:nonce", "credential")]);
        var json = JsonSerializer.Serialize(ready, options);
        var read = JsonSerializer.Deserialize<CallAccepted>(json, options);
        Assert.NotNull(read);
        Assert.Equal(call, read.Session);
        Assert.Equal(ready.IceServers[0].Urls, read.IceServers[0].Urls);
        Assert.Equal(ready.IceServers[0].Username, read.IceServers[0].Username);
        Assert.Equal(ready.IceServers[0].Credential, read.IceServers[0].Credential);
        var signal = new CallSignal("candidate", Candidate: "candidate:1 1 udp 100 192.0.2.1 5000 typ relay",
            SdpMid: "video", SdpMLineIndex: 1);
        Assert.Equal(signal, JsonSerializer.Deserialize<CallSignal>(JsonSerializer.Serialize(signal, options), options));
        var lan = call with { RelayOnly = false, Transport = CallTransports.LanSrtp };
        Assert.Equal(lan, JsonSerializer.Deserialize<CallSession>(JsonSerializer.Serialize(lan, options), options));
        var endpoint = new CallSignal("rtp-endpoint",
            Endpoint: new("192.168.1.2", 5004, new string('a', 32), new string('b', 28)));
        Assert.Equal(endpoint, JsonSerializer.Deserialize<CallSignal>(JsonSerializer.Serialize(endpoint, options), options));
    }
}
