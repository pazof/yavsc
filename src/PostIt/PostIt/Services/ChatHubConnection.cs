using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using PostIt.ViewModels.Chat;
using PostIt.Models;

namespace PostIt.Services;

/// <summary>One authenticated chat transport per application process, independent of page lifetime.</summary>
public sealed class ChatHubConnection : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1);
    private readonly YavscApiClient _api;
    private string _apiUrl;
    private string _authority;
    private string? _accountKey;
    private readonly object _roomsGate = new();
    private readonly Dictionary<string, ChatRoomInfo> _rooms = new(StringComparer.Ordinal);
    private readonly Queue<ChatDelivery> _messages = new();
    private long _sequence;
    public sealed record ChatDelivery(long Sequence, string? Room, string Sender, string Text);
    public event Action<ChatDelivery>? MessageReceived;
    public ChatDelivery[] RecentMessages
    {
        get { lock (_roomsGate) return _messages.ToArray(); }
    }
    public event Action<ChatRoomInfo>? RoomJoined;
    public event Action<string>? Error;
    public ChatRoomInfo[] JoinedRooms
    {
        get { lock (_roomsGate) return _rooms.Values.ToArray(); }
    }
    public HubConnection Connection { get; private set; }
    public bool IsCurrentConfiguration => _apiUrl == _api.Settings.ApiUrl &&
        _authority == _api.Settings.Authentication.Authority && _accountKey == GetAccountKey();

    public ChatHubConnection(YavscApiClient api)
    {
        _api = api;
        _apiUrl = api.Settings.ApiUrl;
        _authority = api.Settings.Authentication.Authority;
        _accountKey = GetAccountKey();
        LoadRooms();
        Connection = BuildConnection();
    }

    private HubConnection BuildConnection()
    {
        var api = _api;
        var hubBase = api.Settings.ApiUrl.TrimEnd('/');
        if (hubBase.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
            hubBase = hubBase[..^"/api/v1".Length];
        var connection = new HubConnectionBuilder().WithUrl(hubBase + "/" + Yavsc.Constants.ChatHubPath, options =>
        {
            options.AccessTokenProvider = async () =>
            {
                if (api.IsSigningOut || !IsCurrentConfiguration)
                    throw new InvalidOperationException("La configuration de messagerie a change.");
                if (!await api.TrySilentLoginAsync())
                    throw new InvalidOperationException("La session de messagerie a expire. Reconnectez-vous.");
                return api.CurrentAccessToken;
            };
        }).WithAutomaticReconnect(new RetryPolicy()).Build();
        connection.On<ChatRoomInfo>("joint", room =>
        {
            if (ReferenceEquals(Connection, connection) && IsCurrentConfiguration)
                RememberRoom(room);
        });
        connection.On<ChatMessage>("ReceiveMessage", message =>
            RememberMessage(connection, message.Room, message.Name ?? "", message.Message ?? ""));
        connection.On<string, string>("addPV", (sender, message) =>
            RememberMessage(connection, null, sender, message));
        connection.Reconnected += _ => ReferenceEquals(Connection, connection) && IsCurrentConfiguration
            ? RestoreRoomsAsync(connection, CancellationToken.None) : Task.CompletedTask;
        return connection;
    }

    public async Task<HubConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (!IsCurrentConfiguration)
            {
                await Connection.DisposeAsync();
                _apiUrl = _api.Settings.ApiUrl;
                _authority = _api.Settings.Authentication.Authority;
                _accountKey = GetAccountKey();
                lock (_roomsGate) _messages.Clear();
                LoadRooms();
                Connection = BuildConnection();
            }
            return Connection;
        }
        finally { _gate.Release(); }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await GetConnectionAsync(ct);
        await _gate.WaitAsync(ct);
        try
        {
            if (_api.IsSigningOut)
                throw new InvalidOperationException("La deconnexion du compte est en cours.");
            if (Connection.State == HubConnectionState.Disconnected)
            {
                await Connection.StartAsync(ct);
                await RestoreRoomsAsync(Connection, ct);
            }
        }
        finally { _gate.Release(); }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try { await Connection.StopAsync(ct); }
        finally
        {
            lock (_roomsGate) _messages.Clear();
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        _gate.Dispose();
    }

    public async Task<ChatRoomInfo> JoinAsync(string room, CancellationToken ct = default)
    {
        var connection = Connection;
        var info = await connection.InvokeAsync<ChatRoomInfo>("Join", room, ct)
            ?? throw new InvalidOperationException("Le serveur n'a pas confirme le salon.");
        if (!ReferenceEquals(Connection, connection) || !IsCurrentConfiguration)
            throw new InvalidOperationException("La session de messagerie a change.");
        if (string.IsNullOrWhiteSpace(info.Name))
            throw new InvalidOperationException("Le serveur a retourne un salon sans nom.");
        RememberRoom(info);
        lock (_roomsGate) PersistRooms();
        return info;
    }

    public async Task LeaveAsync(string room, CancellationToken ct = default)
    {
        var connection = Connection;
        await connection.InvokeAsync("Part", room, "Leaving", ct);
        if (!ReferenceEquals(Connection, connection) || !IsCurrentConfiguration)
            throw new InvalidOperationException("La session de messagerie a change.");
        lock (_roomsGate)
        {
            _rooms.Remove(room);
            PersistRooms();
        }
    }

    private async Task RestoreRoomsAsync(HubConnection connection, CancellationToken ct)
    {
        foreach (var room in JoinedRooms)
        {
            try
            {
                var info = await connection.InvokeAsync<ChatRoomInfo>("Join", room.Name!, ct);
                if (info == null || string.IsNullOrWhiteSpace(info.Name))
                    throw new InvalidOperationException("Le serveur n'a pas confirme le salon.");
                if (ReferenceEquals(Connection, connection) && IsCurrentConfiguration)
                    RememberRoom(info);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ReportError($"Impossible de rejoindre le salon {room.Name} : {ex.Message}");
            }
        }
    }

    private void RememberRoom(ChatRoomInfo room)
    {
        if (string.IsNullOrWhiteSpace(room.Name))
        {
            ReportError("Le serveur a retourne un salon sans nom.");
            return;
        }
        lock (_roomsGate)
        {
            _rooms[room.Name] = room;
        }
        RoomJoined?.Invoke(room);
    }

    private void RememberMessage(HubConnection connection, string? room, string sender, string text)
    {
        if (!ReferenceEquals(Connection, connection) || !IsCurrentConfiguration) return;
        ChatDelivery delivery;
        lock (_roomsGate)
        {
            delivery = new ChatDelivery(++_sequence, room, sender, text);
            _messages.Enqueue(delivery);
            while (_messages.Count > 500) _messages.Dequeue();
        }
        MessageReceived?.Invoke(delivery);
    }

    private void LoadRooms()
    {
        lock (_roomsGate)
        {
            _rooms.Clear();
            if (_accountKey != null && _api.Settings.ChatRoomsByAccount.TryGetValue(_accountKey, out var rooms))
                foreach (var room in rooms.Where(room => !string.IsNullOrWhiteSpace(room)))
                    _rooms[room] = new ChatRoomInfo { Name = room };
        }
    }

    private void PersistRooms()
    {
        if (_accountKey == null) return;
        var rooms = new Dictionary<string, string[]>(_api.Settings.ChatRoomsByAccount)
        {
            [_accountKey] = _rooms.Keys.ToArray()
        };
        _api.Settings.ChatRoomsByAccount = rooms;
        try { _api.Settings.Save(); }
        catch (Exception ex) { ReportError($"Sauvegarde des salons impossible : {ex.Message}"); }
    }

    private void ReportError(string message)
    {
        if (App.Current is App { ServiceProvider: not null } && !Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            Avalonia.Threading.Dispatcher.UIThread.Post(() => _api.Settings.SetActionStatus(message, StatusSeverity.Error));
        else
            _api.Settings.SetActionStatus(message, StatusSeverity.Error);
        Error?.Invoke(message);
    }

    private string? GetAccountKey()
    {
        if (!_api.HasValidSession) return null;
        var token = _api.CurrentIdToken ?? _api.CurrentAccessToken;
        if (string.IsNullOrEmpty(token)) return null;
        var parts = token.Split('.');
        if (parts.Length != 3) return null;
        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var json = JsonDocument.Parse(Convert.FromBase64String(payload));
            if (!json.RootElement.TryGetProperty("sub", out var subject) ||
                subject.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(subject.GetString()))
                return null;
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                _api.Settings.Authentication.Authority + "\n" + _api.Settings.ApiUrl + "\n" + subject.GetString())));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            ReportError("Identite du compte illisible : les salons ne seront pas memorises.");
            return null;
        }
    }

    private sealed class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
            => TimeSpan.FromSeconds(Math.Min(60, 2 + retryContext.PreviousRetryCount * 5));
    }
}
