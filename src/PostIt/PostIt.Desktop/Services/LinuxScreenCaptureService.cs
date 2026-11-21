using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using PostIt.Interfaces;
using Tmds.DBus.Protocol;

namespace MonApp.Services.Linux;


[SupportedOSPlatform("linux")]
public class LinuxScreenCaptureService : IScreenCaptureService
{
    private const string PortalBusName = "org.freedesktop.portal.Desktop";
    private const string PortalObjectPath = "/org/freedesktop/portal/desktop";
    private const string ScreenCastInterface = "org.freedesktop.portal.ScreenCast";
    private const string RequestInterface = "org.freedesktop.portal.Request";


    private DBusConnection? _dbusConnection;
    private Process? _gstProcess;
    private Task<string>? _gstErrorOutput;
    private SafeFileHandle? _pipewireFd;
    private CancellationTokenSource? _timerCts;

    public bool IsRecording { get; private set; }
    public event EventHandler<TimeSpan>? RecordingDurationUpdated;

    [DllImport("libc", SetLastError = true)]
    private static extern int fcntl(int fd, int cmd, int arg);

    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int sig);

    private const int F_GETFD = 1;
    private const int F_SETFD = 2;
    private const int FD_CLOEXEC = 1;
    private const int SIGINT = 2;

    public string? LatestRestoreToken { get; private set; }


    private static void MakeFdInheritable(int fd)
    {
        int flags = fcntl(fd, F_GETFD, 0);
        if (flags >= 0)
        {
            fcntl(fd, F_SETFD, flags & ~FD_CLOEXEC);
        }
    }

    public async Task StartRecordingAsync(CaptureOptions options, CancellationToken cancellationToken = default)
    {
        if (IsRecording) throw new InvalidOperationException("Un enregistrement est déjà en cours.");

        LatestRestoreToken = options.RestoreToken;

        // 1. Connexion D-Bus avec DBusConnection et DBusAddress
        _dbusConnection = new DBusConnection(DBusAddress.Session!);
        await _dbusConnection.ConnectAsync();

        string senderName = _dbusConnection.UniqueName!.TrimStart(':').Replace('.', '_');

        // 2. Création de la session ScreenCast
        string sessionToken = $"monapp_sess_{Guid.NewGuid():N}";
        string createToken = $"monapp_req_{Guid.NewGuid():N}";
        string expectedCreatePath = $"/org/freedesktop/portal/desktop/request/{senderName}/{createToken}";

        var createResponse = await SendPortalRequestAsync(
            expectedCreatePath,
            () => CallCreateSessionAsync(createToken, sessionToken),
            cancellationToken
        );
        string sessionHandle = createResponse["session_handle"].GetString();

        // 3. Sélection de la source
        ScreenCaptureSource effectiveSource = options.TargetWindowHandle.HasValue
            ? ScreenCaptureSource.Window
            : options.Source;
        uint sourceType = effectiveSource switch
        {
            ScreenCaptureSource.Screen => 1u,
            ScreenCaptureSource.Window => 2u,
            ScreenCaptureSource.Prompt => 3u,
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.Source, "Source de capture inconnue.")
        };
        uint cursorMode = options.CaptureCursor ? 2u : 1u;

        string selectToken = $"monapp_req_{Guid.NewGuid():N}";
        string expectedSelectPath = $"/org/freedesktop/portal/desktop/request/{senderName}/{selectToken}";

        await SendPortalRequestAsync(
            expectedSelectPath,
            () => CallSelectSourcesAsync(
                sessionHandle,
                selectToken,
                sourceType,
                cursorMode,
                effectiveSource == ScreenCaptureSource.Screen ? LatestRestoreToken : null),
            cancellationToken
        );

        // 4. Démarrage du flux (Popup système)
        string startToken = $"monapp_req_{Guid.NewGuid():N}";
        string expectedStartPath = $"/org/freedesktop/portal/desktop/request/{senderName}/{startToken}";

        var startResponse = await SendPortalRequestAsync(
            expectedStartPath,
            () => CallStartAsync(sessionHandle, startToken),
            cancellationToken
        );

        if (startResponse.TryGetValue("restore_token", out var tokenVariant))
        {
            LatestRestoreToken = tokenVariant.GetString();
        }

        var streams = startResponse["streams"];
        if (streams.Count == 0)
            throw new OperationCanceledException("Aucune source vidéo sélectionnée.");

        uint pipewireNodeId = streams.GetItem(0).GetItem(0).GetUInt32();

        // 5. Récupération du File Descriptor PipeWire

        _pipewireFd = await CallOpenPipeWireRemoteAsync(sessionHandle);
        int rawFd = _pipewireFd.DangerousGetHandle().ToInt32();

        MakeFdInheritable(rawFd); // Indispensable pour que gst-launch-1.0 voie le flux PipeWire

        await StartGStreamerPipelineAsync(rawFd, pipewireNodeId, options);

        IsRecording = true;
        StartDurationTimer();
    }
    private void StartDurationTimer()
    {
        _timerCts = new CancellationTokenSource();
        var sw = Stopwatch.StartNew();
        _ = Task.Run(async () =>
        {
            while (!_timerCts.Token.IsCancellationRequested)
            {
                RecordingDurationUpdated?.Invoke(this, sw.Elapsed);
                await Task.Delay(500, _timerCts.Token);
            }
        });
    }

    private async Task StartGStreamerPipelineAsync(int fd, uint nodeId, CaptureOptions options)
    {
        // Pipeline GStreamer :
        // pipewiresrc lit directement les textures GPU (DMA-BUF)
        // videoconvert adapte le format pixel
        // vaapih264enc (encodage matériel Intel/AMD) ou x264enc (fallback CPU)
        string encoder = HasVaapiSupport()
            ? "vaapipostproc ! vaapih264enc rate-control=cbr bitrate=8000"
            : "videoconvert ! video/x-raw,format=I420 ! x264enc speed-preset=ultrafast tune=zerolatency";

        string args = $"-e pipewiresrc fd={fd} path={nodeId} do-timestamp=true keepalive-time=1000 ! " +
                      $"videorate ! video/x-raw,framerate={options.Fps}/1 ! " +
                      $"{encoder} ! h264parse ! queue ! mp4mux name=mux ! " +
                      $"filesink location=\"{EscapeGStreamerString(options.OutputFilePath)}\"";

        if (options.CaptureAudio)
        {
            string audioDevice = await GetDefaultAudioMonitorAsync();
            args += $" pulsesrc device=\"{EscapeGStreamerString(audioDevice)}\" do-timestamp=true ! " +
                    "queue ! audioconvert ! audioresample ! avenc_aac bitrate=192000 ! " +
                    "aacparse ! queue ! mux.";
        }

        _gstProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gst-launch-1.0",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _gstProcess.Start();
        _gstErrorOutput = _gstProcess.StandardError.ReadToEndAsync();
    }

    private static string EscapeGStreamerString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
             .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static async Task<string> GetDefaultAudioMonitorAsync()
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pactl",
                Arguments = "get-default-sink",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3));

            string output = (await outputTask).Trim();
            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                string error = (await errorTask).Trim();
                throw new InvalidOperationException(
                    $"Impossible de déterminer la sortie audio par défaut.{FormatProcessError(error)}");
            }

            return output + ".monitor";
        }
        catch (TimeoutException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            throw new InvalidOperationException("La détection de la sortie audio a expiré.");
        }
        finally
        {
            process.Dispose();
        }
    }

    private static bool HasVaapiSupport()
    {
        try
        {
            var p = Process.Start(new ProcessStartInfo("gst-inspect-1.0", "vaapih264enc")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            p?.WaitForExit(1000);
            return p?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private Task CallStartAsync(string sessionHandle, string handleToken)
    {
        var writer = _dbusConnection!.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: PortalBusName,
            path: PortalObjectPath,
            @interface: ScreenCastInterface,
            member: "Start",
            signature: "osa{sv}"
        );

        writer.WriteObjectPath(sessionHandle);
        writer.WriteString(""); // Parent window identifier (optionnel)
        writer.WriteDictionary(new Dictionary<string, VariantValue>
        {
            ["handle_token"] = VariantValue.String(handleToken)
        });

        return _dbusConnection.CallMethodAsync(writer.CreateMessage());
    }

    public async Task StopRecordingAsync()
    {
        if (!IsRecording && _gstProcess is null) return;

        IsRecording = false;
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;

        Process? process = _gstProcess;
        if (process is null) return;

        string errorOutput;
        int exitCode;
        try
        {
            if (!process.HasExited && kill(process.Id, SIGINT) != 0)
            {
                throw new InvalidOperationException(
                    $"Impossible d'envoyer SIGINT à GStreamer (errno {Marshal.GetLastPInvokeError()}).");
            }

            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            errorOutput = _gstErrorOutput is null ? string.Empty : await _gstErrorOutput;
            exitCode = process.ExitCode;
        }
        catch (TimeoutException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            throw new InvalidOperationException(
                "GStreamer n'a pas finalisé le fichier MP4 dans les dix secondes.");
        }
        catch
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            throw;
        }
        finally
        {
            _pipewireFd?.Dispose();
            _pipewireFd = null;
            _dbusConnection?.Dispose();
            _dbusConnection = null;
            process.Dispose();
            _gstProcess = null;
            _gstErrorOutput = null;
        }

        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"GStreamer s'est arrêté avec le code {exitCode}.{FormatProcessError(errorOutput)}");
        }
    }

    private static string FormatProcessError(string errorOutput) =>
        string.IsNullOrWhiteSpace(errorOutput) ? string.Empty : $" Détail : {errorOutput.Trim()}";

    private async Task<Dictionary<string, VariantValue>> SendPortalRequestAsync(
        string expectedRequestPath,
        Func<Task> dbusCall,
        CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<Dictionary<string, VariantValue>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Interface = RequestInterface,
            Member = "Response",
            Path = expectedRequestPath
        };

        using var observer = await _dbusConnection!.AddMatchAsync(
            rule,
            static (Message message, object? state) =>
            {
                var reader = message.GetBodyReader();
                uint responseCode = reader.ReadUInt32();
                var results = reader.ReadDictionaryOfStringToVariantValue();
                return (responseCode, results);
            },
            (Notification<(uint responseCode, Dictionary<string, VariantValue> results)> notification) =>
            {
                if (notification.HasValue)
                {
                    var (code, results) = notification.Value;
                    if (code == 0)
                        tcs.TrySetResult(results);
                    else
                        tcs.TrySetException(new OperationCanceledException($"Requête annulée (code {code})."));
                }
                else if (notification.CompletionReason != null)
                {
                    tcs.TrySetException(new OperationCanceledException($"Requête annulée ({notification.CompletionReason})."));
                }
            }
        );

        await dbusCall();

        using (ct.Register(() => tcs.TrySetCanceled()))
        {
            return await tcs.Task;
        }
    }

    // ... (Les méthodes CallCreateSessionAsync, CallSelectSourcesAsync, CallStartAsync,  restent identiques à la réponse précédente)

    private Task CallCreateSessionAsync(string handleToken, string sessionToken)
    {
        using var writer = _dbusConnection!.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: PortalBusName,
            path: PortalObjectPath,
            @interface: ScreenCastInterface,
            member: "CreateSession",
            signature: "a{sv}"
        );

        writer.WriteDictionary(new Dictionary<string, VariantValue>
        {
            ["handle_token"] = VariantValue.String(handleToken),
            ["session_handle_token"] = VariantValue.String(sessionToken)
        });

        return _dbusConnection.CallMethodAsync(writer.CreateMessage());
    }


    private Task CallSelectSourcesAsync(
        string sessionHandle,
        string handleToken,
        uint types,
        uint cursorMode,
        string? restoreToken)
    {
        using var writer = _dbusConnection!.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: PortalBusName,
            path: PortalObjectPath,
            @interface: ScreenCastInterface,
            member: "SelectSources",
            signature: "oa{sv}"
        );

        var optionsDict = new Dictionary<string, VariantValue>
        {
            ["handle_token"] = VariantValue.String(handleToken),
            ["types"] = VariantValue.UInt32(types),
            ["cursor_mode"] = VariantValue.UInt32(cursorMode),
            ["multiple"] = VariantValue.Bool(false),
            // 0 = Ne pas persister
            // 1 = Persister tant que l'application reste ouverte
            // 2 = Persister de façon permanente (jusqu'à révocation explicite)
            ["persist_mode"] = VariantValue.UInt32(2)
        };

        if (!string.IsNullOrEmpty(restoreToken))
        {
            optionsDict["restore_token"] = VariantValue.String(restoreToken);
        }

        writer.WriteObjectPath(sessionHandle);
        writer.WriteDictionary(optionsDict);

        return _dbusConnection.CallMethodAsync(writer.CreateMessage());
    }

    private Task<SafeFileHandle> CallOpenPipeWireRemoteAsync(string sessionHandle)
    {
        using var writer = _dbusConnection!.GetMessageWriter();
        writer.WriteMethodCallHeader(
            destination: PortalBusName,
            path: PortalObjectPath,
            @interface: ScreenCastInterface,
            member: "OpenPipeWireRemote",
            signature: "oa{sv}"
        );

        writer.WriteObjectPath(sessionHandle);
        writer.WriteDictionary(new Dictionary<string, VariantValue>());

        return _dbusConnection.CallMethodAsync(
            writer.CreateMessage(),
            static (Message reply, object? state) => reply.GetBodyReader().ReadHandle<SafeFileHandle>()!
        );
    }

    public void Dispose()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();

        if (_gstProcess is not null)
        {
            if (!_gstProcess.HasExited)
            {
                kill(_gstProcess.Id, SIGINT);
                if (!_gstProcess.WaitForExit(10_000))
                {
                    _gstProcess.Kill(entireProcessTree: true);
                    _gstProcess.WaitForExit();
                }
            }

            _gstProcess.Dispose();
        }

        _pipewireFd?.Dispose();
        _dbusConnection?.Dispose();
    }
}
