using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace PostIt.Android.Services;

[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeCamera | ForegroundService.TypeMicrophone)]
[SupportedOSPlatform("android26.0")]
public sealed class PodcastCaptureService : Service
{
    private const string Channel = "podcast-broadcast";
    internal const string StopAction = "fr.pschneider.postit.STOP_PODCAST";
    private const int NotificationId = 4044;
    private static TaskCompletionSource<PodcastCaptureService>? _pending;
    private PowerManager.WakeLock? _wakeLock;
    internal AndroidPodcastCapture? Capture { get; set; }

    public static async Task<PodcastCaptureService> StartAsync(Activity activity, bool microphone, CancellationToken ct)
    {
        if (_pending is not null) throw new InvalidOperationException("Un direct Android est deja en cours.");
        var pending = new TaskCompletionSource<PodcastCaptureService>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending = pending;
        try
        {
            var intent = new Intent(activity, typeof(PodcastCaptureService)).PutExtra("microphone", microphone);
            activity.StartForegroundService(intent);
            return await pending.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
        catch
        {
            activity.StopService(new Intent(activity, typeof(PodcastCaptureService)));
            _pending = null;
            throw;
        }
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (intent?.Action == StopAction)
        {
            if (Capture is { } capture) capture.RequestStop();
            else
            {
                _pending?.TrySetException(new System.OperationCanceledException("Demarrage du direct annule depuis la notification."));
                StopSelf();
            }
            return StartCommandResult.NotSticky;
        }
        try
        {
            var manager = GetSystemService(NotificationService) as NotificationManager
                ?? throw new InvalidOperationException("Notifications Android indisponibles.");
            manager.CreateNotificationChannel(new NotificationChannel(Channel, "Diffusion podcast", NotificationImportance.Low));
            var launch = PackageManager?.GetLaunchIntentForPackage(PackageName!)
                ?? throw new InvalidOperationException("Application Android indisponible.");
            var open = PendingIntent.GetActivity(this, 0, launch, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            var stop = PendingIntent.GetService(this, 1,
                new Intent(this, typeof(PodcastCaptureService)).SetAction(StopAction),
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            var builder = new NotificationCompat.Builder(this, Channel);
            builder.SetSmallIcon(global::Android.Resource.Drawable.IcMenuCamera);
            builder.SetContentTitle("Podcast en direct");
            builder.SetContentText("Camera active. Toucher pour revenir au direct.");
            builder.SetContentIntent(open);
            builder.SetOngoing(true);
            builder.AddAction(global::Android.Resource.Drawable.IcMediaPause, "Arreter et publier", stop);
            var notification = builder.Build();
            var type = (ForegroundService)0;
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                type = ForegroundService.TypeCamera;
                if (intent?.GetBooleanExtra("microphone", false) == true) type |= ForegroundService.TypeMicrophone;
            }
            ServiceCompat.StartForeground(this, NotificationId, notification, (int)type);
            var power = GetSystemService(PowerService) as PowerManager
                ?? throw new InvalidOperationException("Gestion d'alimentation Android indisponible.");
            _wakeLock = power.NewWakeLock(WakeLockFlags.Partial, "PostIt:Podcast")
                ?? throw new InvalidOperationException("Le verrou d'alimentation Android est indisponible.");
            _wakeLock.Acquire();
            _pending?.TrySetResult(this);
        }
        catch (Exception ex)
        {
            _pending?.TrySetException(ex);
            StopSelf();
        }
        return StartCommandResult.NotSticky;
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnDestroy()
    {
        _pending?.TrySetException(new InvalidOperationException("Le service de diffusion Android a ete arrete."));
        Capture?.RequestStop();
        Capture = null;
        if (_wakeLock?.IsHeld == true) _wakeLock.Release();
        _wakeLock?.Dispose();
        _wakeLock = null;
        _pending = null;
        base.OnDestroy();
    }
}
