using System.Net.WebSockets;

public class Streamer {
    private ClientWebSocket _client;

    public Streamer(string token)
    {
        _client = new ClientWebSocket();
            _client.Options.SetRequestHeader("Authorization", $"Bearer {token}");
    }
}