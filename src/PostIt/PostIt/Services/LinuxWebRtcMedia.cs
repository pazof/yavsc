using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace PostIt.Services;

public sealed class LinuxWebRtcMedia : WebRtcNativeMedia
{
    private readonly Image _local = new() { Stretch = Stretch.Uniform };
    private readonly Image _remote = new() { Stretch = Stretch.Uniform };
    private readonly SemaphoreSlim _input = new(1);
    private readonly CancellationTokenSource _lifetime = new();
    private readonly StringBuilder _errors = new();
    private readonly object _frameLock = new();
    private byte[]? _localFrame;
    private byte[]? _remoteFrame;
    private bool _frameScheduled;
    private Bitmap? _localBitmap;
    private Bitmap? _remoteBitmap;
    private Process? _process;
    private Task? _reader;
    private Task? _stderr;
    private string? _script;
    private bool _disposed;

    public override Control LocalVideo => _local;
    public override Control RemoteVideo => _remote;

    public static PodcastCaptureDevices GetDevices() => new(
        Directory.GetFiles("/dev", "video*").OrderBy(p => p).Select(p => new CaptureDevice(p, p)).ToArray(),
        [new("Microphone système (PulseAudio/PipeWire)", "default")]);

    protected override async Task StartCoreAsync(WebRtcMediaConfiguration configuration, CancellationToken ct)
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("GStreamer requiert Linux.");
        _script = Path.Combine(Path.GetTempPath(), $"postit-webrtc-{Guid.NewGuid():N}.py");
        await using (var resource = typeof(LinuxWebRtcMedia).Assembly.GetManifestResourceStream("PostIt.WebRtc.gstreamer_peer.py")
            ?? throw new InvalidOperationException("Adaptateur GStreamer introuvable."))
        await using (var file = new FileStream(_script, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await resource.CopyToAsync(file, ct);
        _process = new Process
        {
            StartInfo = new ProcessStartInfo("python3")
            {
                ArgumentList = { "-u", _script },
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        if (!_process.Start()) throw new InvalidOperationException("Démarrage du moteur GStreamer impossible.");
        _stderr = ReadErrorsAsync(_process);
        _reader = ReadAsync(_process);
        await SendCommandAsync(StartCommand(configuration), ct);
    }

    protected override async Task SendCommandAsync(string json, CancellationToken ct)
    {
        await _input.WaitAsync(ct);
        try
        {
            if (_process is null || _process.HasExited)
                throw new InvalidOperationException("Le moteur GStreamer est arrêté. Vérifiez Python GI, GStreamer WebRTC et libnice.");
            await _process.StandardInput.WriteLineAsync(json.AsMemory(), ct);
            await _process.StandardInput.FlushAsync(ct);
        }
        finally { _input.Release(); }
    }

    private async Task ReadErrorsAsync(Process process)
    {
        while (await process.StandardError.ReadLineAsync(_lifetime.Token) is { } line)
        {
            lock (_errors)
            {
                _errors.AppendLine(line);
                if (_errors.Length > 4096) _errors.Remove(0, _errors.Length - 4096);
            }
        }
    }

    private async Task ReadAsync(Process process)
    {
        try
        {
            while (await process.StandardOutput.ReadLineAsync(_lifetime.Token) is { } line)
            {
                if (line.Length > 1500000) throw new InvalidOperationException("Trame native trop volumineuse.");
                ProcessMessage(line);
            }
            await process.WaitForExitAsync(_lifetime.Token);
            if (!_disposed)
            {
                if (_stderr is not null) await _stderr;
                string details;
                lock (_errors) details = _errors.ToString();
                throw new InvalidOperationException($"Le moteur GStreamer s'est arrêté ({process.ExitCode}). {details}");
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) { Fail(error); }
    }

    protected override void OnFrame(JsonElement frame)
    {
        var image = frame.GetProperty("data").GetBytesFromBase64();
        var target = frame.GetProperty("target").GetString();
        if (target is not ("local" or "remote"))
            throw new InvalidOperationException("Destination de trame WebRTC inconnue.");
        lock (_frameLock)
        {
            if (_disposed) return;
            if (target == "local") _localFrame = image;
            else _remoteFrame = image;
            if (_frameScheduled) return;
            _frameScheduled = true;
        }
        Dispatcher.UIThread.Post(DisplayFrames);
    }

    private void DisplayFrames()
    {
        byte[]? local;
        byte[]? remote;
        lock (_frameLock)
        {
            _frameScheduled = false;
            local = _localFrame;
            remote = _remoteFrame;
            _localFrame = _remoteFrame = null;
        }
        if (_disposed) return;
        try
        {
            if (local is not null) Display(_local, local, ref _localBitmap);
            if (remote is not null) Display(_remote, remote, ref _remoteBitmap);
        }
        catch (Exception error) { Fail(error); }
    }

    private static void Display(Image view, byte[] bytes, ref Bitmap? previous)
    {
        using var stream = new MemoryStream(bytes, writable: false);
        var image = new Bitmap(stream);
        view.Source = image;
        previous?.Dispose();
        previous = image;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        Exception? stopError = null;
        try
        {
            if (_process is { HasExited: false } process)
            {
                try { await SendCommandAsync("{\"command\":\"close\"}", CancellationToken.None); }
                catch (Exception error) when (error is IOException or InvalidOperationException)
                {
                    stopError = error;
                }
                try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (TimeoutException)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                    throw new TimeoutException("Le moteur GStreamer n'a pas terminé son arrêt ; processus interrompu.");
                }
            }
        }
        finally
        {
            _lifetime.Cancel();
            if (_reader is not null) await _reader;
            if (_stderr is not null)
            {
                try { await _stderr; }
                catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
            }
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _local.Source = _remote.Source = null;
                _localBitmap?.Dispose();
                _remoteBitmap?.Dispose();
            });
            _process?.Dispose();
            if (_script is not null) File.Delete(_script);
            _input.Dispose();
            _lifetime.Dispose();
            Closed();
        }
        if (stopError is not null)
            throw new InvalidOperationException("La commande d'arret GStreamer a echoue.", stopError);
    }
}
