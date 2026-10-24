using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Views;
using Microsoft.Win32.SafeHandles;
using PostIt.Services;

namespace PostIt.Android.Services;

[SupportedOSPlatform("android26.0")]
internal sealed class AndroidPodcastCapture : IPodcastCapture
{
    private readonly PodcastCaptureService _service;
    private readonly SemaphoreSlim _stopGate = new(1);
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Handler _handler = new(Looper.MainLooper!);
    private CameraDevice? _camera;
    private CameraCaptureSession? _session;
    private MediaRecorder? _recorder;
    private AndroidCameraFrameRotator? _rotator;
    private Surface? _surface;
    private ParcelFileDescriptor? _readPipe;
    private ParcelFileDescriptor? _writePipe;
    private Task _pump = Task.CompletedTask;
    private Exception? _failure;
    private bool _stopped;
    private bool _stopRequested;
    private bool _started;
    private bool _disposed;
    public Task Completion => _completion.Task;
    internal string OrientationDiagnostics { get; private set; } = "";
    internal string? TextureTransform => _rotator?.FirstTextureTransform;

    private AndroidPodcastCapture(PodcastCaptureService service) => _service = service;

    private static CameraManager GetManager(Context context)
        => context.GetSystemService(Context.CameraService) as CameraManager
            ?? throw new InvalidOperationException("Service camera Android indisponible.");

    public static PodcastCaptureDevices GetDevices()
    {
        var activity = MainActivity.Current ?? throw new InvalidOperationException("Activite Android indisponible.");
        var manager = GetManager(activity);
        var cameras = new List<CaptureDevice>();
        foreach (var id in manager.GetCameraIdList())
        {
            using var characteristics = manager.GetCameraCharacteristics(id);
            using var facing = characteristics.Get(CameraCharacteristics.LensFacing) as Java.Lang.Integer;
            cameras.Add(new(facing?.IntValue() == (int)LensFacing.Front ? "Camera avant" : "Camera arriere", id));
        }
        return new(cameras, [new("Sans microphone", ""), new("Microphone Android", "microphone")]);
    }

    public static async Task RequestPermissionsAsync(string? microphone, CancellationToken ct)
    {
        var activity = MainActivity.Current ?? throw new InvalidOperationException("Activite Android indisponible.");
        await PodcastPermissions.EnsureAsync(activity, microphone is not null, ct);
        if (!MainActivity.IsForeground)
            throw new InvalidOperationException("Revenez dans PostIt pour demarrer la camera.");
    }

    public static async Task<IPodcastCapture> StartAsync(string camera, string? microphone, string directory, CancellationToken ct)
    {
        await RequestPermissionsAsync(microphone, ct);
        var activity = MainActivity.Current ?? throw new InvalidOperationException("Activite Android indisponible.");
        var service = await PodcastCaptureService.StartAsync(activity, microphone is not null, ct);
        var capture = new AndroidPodcastCapture(service);
        service.Capture = capture;
        try
        {
            ct.ThrowIfCancellationRequested();
            await capture.StartNativeAsync(camera, microphone is not null, directory, ct);
            return capture;
        }
        catch
        {
            await capture.DisposeAsync();
            throw;
        }
    }

    private async Task StartNativeAsync(string cameraId, bool microphone, string directory, CancellationToken ct)
    {
        await _stopGate.WaitAsync(ct);
        try { await ConfigureNativeAsync(cameraId, microphone, directory, ct); }
        finally { _stopGate.Release(); }
    }

    private async Task ConfigureNativeAsync(string cameraId, bool microphone, string directory, CancellationToken ct)
    {
        var manager = GetManager(_service);
        using var characteristics = manager.GetCameraCharacteristics(cameraId);
        using var formats = characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap) as StreamConfigurationMap
            ?? throw new InvalidOperationException("Formats camera indisponibles.");
        var sizes = formats.GetOutputSizes(Java.Lang.Class.FromType(typeof(MediaRecorder)))
            ?? throw new InvalidOperationException("Aucun format video disponible.");
        var size = sizes.Where(s => s.Width <= 640 && s.Height <= 480)
            .OrderByDescending(s => s.Width * s.Height).FirstOrDefault()
            ?? throw new InvalidOperationException("La camera ne propose aucun format jusqu'a 480p.");
        using var sensor = characteristics.Get(CameraCharacteristics.SensorOrientation) as Java.Lang.Integer
            ?? throw new InvalidOperationException("Orientation du capteur camera indisponible.");
        using var facing = characteristics.Get(CameraCharacteristics.LensFacing) as Java.Lang.Integer
            ?? throw new InvalidOperationException("Position du capteur camera indisponible.");
        var activity = MainActivity.Current ?? throw new InvalidOperationException("Activite Android indisponible.");
#pragma warning disable CS0618, CA1422
        var display = OperatingSystem.IsAndroidVersionAtLeast(30)
            ? activity.Display : activity.WindowManager?.DefaultDisplay;
#pragma warning restore CS0618, CA1422
        var displayRotation = display?.Rotation
            ?? throw new InvalidOperationException("Orientation de l'ecran Android indisponible.");
        var displayDegrees = displayRotation switch
        {
            SurfaceOrientation.Rotation0 => 0,
            SurfaceOrientation.Rotation90 => 90,
            SurfaceOrientation.Rotation180 => 180,
            SurfaceOrientation.Rotation270 => 270,
            _ => throw new InvalidOperationException($"Rotation d'ecran Android inconnue : {displayRotation}.")
        };
        var rotation = CameraFrameOrientation.GetRotation(sensor.IntValue(), displayDegrees,
            facing.IntValue() == (int)LensFacing.Front);
        global::Android.Util.Log.Info("PostIt.Podcast",
            $"Camera orientation: sensor={sensor.IntValue()}, display={displayDegrees}, facing={facing.IntValue()}, rotation={rotation}");
        OrientationDiagnostics = $"sensor={sensor.IntValue()}, display={displayDegrees}, facing={facing.IntValue()}, rotation={rotation}";
#pragma warning disable CS0618
        _recorder = OperatingSystem.IsAndroidVersionAtLeast(31)
            ? new MediaRecorder(_service) : new MediaRecorder();
#pragma warning restore CS0618
        _recorder.Error += OnRecorderError;
        if (microphone) _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetVideoSource(VideoSource.Surface);
        _recorder.SetOutputFormat(OutputFormat.Mpeg2Ts);
        _recorder.SetVideoSize(rotation is 90 or 270 ? size.Height : size.Width,
            rotation is 90 or 270 ? size.Width : size.Height);
        _recorder.SetVideoFrameRate(30);
        _recorder.SetVideoEncodingBitRate(1200000);
        _recorder.SetVideoEncoder(VideoEncoder.H264);
        if (microphone)
        {
            _recorder.SetAudioSamplingRate(48000);
            _recorder.SetAudioChannels(1);
            _recorder.SetAudioEncodingBitRate(96000);
            _recorder.SetAudioEncoder(AudioEncoder.Aac);
        }
        var pipe = ParcelFileDescriptor.CreatePipe()
            ?? throw new IOException("Creation du canal MPEG-TS impossible.");
        _readPipe = pipe[0];
        _writePipe = pipe[1];
        _recorder.SetOutputFile(_writePipe.FileDescriptor);
        await Task.Run(_recorder.Prepare, ct);
        ct.ThrowIfCancellationRequested();
        if (_stopRequested) throw new System.OperationCanceledException("Demarrage de la capture annule.");
        _surface = _recorder.Surface ?? throw new InvalidOperationException("Surface encodeur indisponible.");
        _rotator = await AndroidCameraFrameRotator.CreateAsync(_surface, size.Width, size.Height, rotation, Fail);
        ct.ThrowIfCancellationRequested();
        if (_stopRequested) throw new System.OperationCanceledException("Demarrage de la capture annule.");
        var cameraSurface = _rotator.CameraSurface;
        var opened = new TaskCompletionSource<CameraDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
        manager.OpenCamera(cameraId, new DeviceCallback(this, opened), _handler);
        _camera = await opened.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        var configured = new TaskCompletionSource<CameraCaptureSession>(TaskCreationOptions.RunContinuationsAsynchronously);
#pragma warning disable CS0618, CA1422
        _camera.CreateCaptureSession(new List<Surface> { cameraSurface }, new SessionCallback(this, configured), _handler);
#pragma warning restore CS0618, CA1422
        _session = await configured.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        ct.ThrowIfCancellationRequested();
        if (_stopRequested) throw new System.OperationCanceledException("Demarrage de la capture annule.");
        using var request = _camera.CreateCaptureRequest(CameraTemplate.Record);
        request.AddTarget(cameraSurface);
        using var built = request.Build();
        var handle = new SafeFileHandle((IntPtr)_readPipe.Fd, ownsHandle: false);
        var stream = new FileStream(handle, FileAccess.Read);
        _pump = Task.Run(async () =>
        {
            using (handle)
            using (stream)
            {
                try
                {
                    await new TransportStreamCapture(directory).RunAsync(stream, CancellationToken.None);
                    if (_failure is not null) throw new IOException("Capture Android interrompue.", _failure);
                    if (!_stopped) throw new IOException("Le flux de capture Android s'est ferme prematurement.");
                    _completion.TrySetResult();
                }
                catch (Exception ex)
                {
                    _completion.TrySetException(ex);
                    // Drain until MediaRecorder stops, so its writer cannot block Stop().
                    await stream.CopyToAsync(System.IO.Stream.Null);
                }
            }
        });
        await Task.Run(_recorder.Start, ct);
        _started = true;
        await _rotator.StartAsync();
        _session.SetRepeatingRequest(built, null, _handler);
        _writePipe.Close();
        _writePipe.Dispose();
        _writePipe = null;
    }

    private sealed class DeviceCallback(AndroidPodcastCapture capture, TaskCompletionSource<CameraDevice> opened)
        : CameraDevice.StateCallback
    {
        public override void OnOpened(CameraDevice camera)
        {
            if (capture._stopRequested || opened.Task.IsCompleted)
            {
                camera.Close();
                opened.TrySetException(new IOException("Ouverture camera annulee."));
            }
            else
            {
                capture._camera = camera;
                opened.TrySetResult(camera);
            }
        }

        public override void OnDisconnected(CameraDevice camera) => Fail(camera, "Camera Android deconnectee.");
        public override void OnError(CameraDevice camera, CameraError error) => Fail(camera, $"Erreur camera Android : {error}.");
        private void Fail(CameraDevice camera, string message)
        {
            camera.Close();
            var error = new IOException(message);
            opened.TrySetException(error);
            capture.Fail(error);
        }
    }

    private sealed class SessionCallback(AndroidPodcastCapture capture, TaskCompletionSource<CameraCaptureSession> configured)
        : CameraCaptureSession.StateCallback
    {
        public override void OnConfigured(CameraCaptureSession session)
        {
            if (capture._stopRequested || configured.Task.IsCompleted)
            {
                session.Close();
                configured.TrySetException(new IOException("Configuration camera annulee."));
            }
            else
            {
                capture._session = session;
                configured.TrySetResult(session);
            }
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            session.Close();
            var error = new IOException("Configuration de la capture Android impossible.");
            configured.TrySetException(error);
            capture.Fail(error);
        }
    }

    private void OnRecorderError(object? sender, MediaRecorder.ErrorEventArgs e)
        => Fail(new IOException($"Erreur encodeur Android : {e.What}/{e.Extra}."));

    private void Fail(Exception error)
    {
        if (_stopped) return;
        _failure = error;
        _completion.TrySetException(error);
        RequestStop();
    }

    internal async void RequestStop()
    {
        try { await StopAsync(); }
        catch (Exception ex) { global::Android.Util.Log.Error("PostIt.Podcast", ex.ToString()); }
    }

    public async Task StopAsync()
    {
        _stopRequested = true;
        await _stopGate.WaitAsync();
        try
        {
            if (_stopped) return;
            _stopped = true;
            await Task.Run(StopNative);
            await _pump;
        }
        finally { _stopGate.Release(); }
    }

    private void StopNative()
    {
        try
        {
            // Keep frames flowing until MediaRecorder has drained its encoder.
            if (_started) _recorder?.Stop();
        }
        catch (Java.Lang.RuntimeException ex)
        {
            _failure = ex;
            _completion.TrySetException(new IOException("L'encodeur Android n'a pas pu finaliser la capture.", ex));
        }
        finally
        {
            _started = false;
            _session?.Close();
            _session?.Dispose();
            _session = null;
            _camera?.Close();
            _camera?.Dispose();
            _camera = null;
            _rotator?.Dispose();
            _rotator = null;
            if (_recorder is not null)
            {
                _recorder.Error -= OnRecorderError;
                _recorder.Release();
                _recorder.Dispose();
                _recorder = null;
            }
            _surface?.Dispose();
            _surface = null;
            _writePipe?.Close();
            _writePipe?.Dispose();
            _writePipe = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try { await StopAsync(); }
        finally
        {
            _readPipe?.Dispose();
            _readPipe = null;
            _service.Capture = null;
            _service.StopSelf();
            _handler.Dispose();
            _stopGate.Dispose();
        }
    }
}
