using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public abstract class WebRtcNativeMedia : IWebRtcMedia
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    protected WebRtcMediaConfiguration? Configuration { get; private set; }
    public abstract Control LocalVideo { get; }
    public abstract Control RemoteVideo { get; }
    public event Action<CallSignal>? Signal;
    public event Action<WebRtcMediaStatus>? StatusChanged;
    public Task Completion => _completion.Task;

    public async Task StartAsync(WebRtcMediaConfiguration configuration, CancellationToken ct)
    {
        if (Configuration is not null) throw new InvalidOperationException("Le moteur WebRTC est déjà démarré.");
        WebRtcPrivacy.ValidateConfiguration(configuration);
        Configuration = configuration;
        await StartCoreAsync(configuration, ct);
    }

    protected abstract Task StartCoreAsync(WebRtcMediaConfiguration configuration, CancellationToken ct);
    protected abstract Task SendCommandAsync(string json, CancellationToken ct);
    public abstract ValueTask DisposeAsync();

    public Task HandleSignalAsync(CallSignal signal, CancellationToken ct)
    {
        // Privacy constrains our own addresses, not a peer's public TURN address.
        WebRtcPrivacy.ValidateSignal(signal, false);
        return SendCommandAsync(JsonSerializer.Serialize(new { command = "signal", signal }, JsonOptions), ct);
    }

    protected void ProcessMessage(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        switch (root.GetProperty("type").GetString())
        {
            case "signal":
                var signal = root.GetProperty("signal").Deserialize<CallSignal>(JsonOptions)
                    ?? throw new InvalidOperationException("Signal natif vide.");
                WebRtcPrivacy.ValidateSignal(signal, Configuration!.RelayOnly);
                Signal?.Invoke(signal);
                break;
            case "status":
                var status = root.GetProperty("status").Deserialize<WebRtcMediaStatus>(JsonOptions)
                    ?? throw new InvalidOperationException("Statistiques WebRTC vides.");
                WebRtcPrivacy.ValidateStatus(status, Configuration!.RelayOnly);
                StatusChanged?.Invoke(status);
                break;
            case "error":
                throw new InvalidOperationException(root.GetProperty("message").GetString());
            case "closed":
                _completion.TrySetResult();
                break;
            case "frame":
                OnFrame(root);
                break;
            default:
                throw new InvalidOperationException("Événement natif WebRTC inconnu.");
        }
    }

    protected virtual void OnFrame(JsonElement frame) =>
        throw new InvalidOperationException("Ce moteur ne transmet pas de trames décodées.");

    protected void Fail(Exception error) => _completion.TrySetException(error);
    protected void Closed() => _completion.TrySetResult();
    protected static string StartCommand(WebRtcMediaConfiguration configuration) =>
        JsonSerializer.Serialize(new { command = "start", configuration }, JsonOptions);
}
