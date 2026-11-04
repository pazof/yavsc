using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Models;
using PostIt.Services;
using PostIt.ViewModels.Chat;
using OperationCanceledException = System.OperationCanceledException;

namespace PostIt.Android.Services;

[Service(Name = "fr.pschneider.postit.ChatConnectionService", Exported = false)]
public sealed class ChatConnectionService : Service
{
    private const string MessageChannel = "postit-chat-messages";
    private const int MessageNotificationId = 4053;
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> Startups = new();
    private static volatile ChatConnectionService? _current;
    private CancellationTokenSource _stop = new();
    private string? _epoch;
    private readonly List<IDisposable> _subscriptions = [];
    private readonly object _observerGate = new();
    private YavscApiClient? _api;
    private ChatHubConnection? _transport;
    private HubConnection? _observedConnection;
    private Task? _loop;
    private volatile bool _destroyed;
    private bool _wasConnected;

    internal static async Task StartAndWaitAsync(Context context, string epoch, string identity, CancellationToken ct)
    {
        var pending = Startups.GetOrAdd(epoch,
            _ => new(TaskCreationOptions.RunContinuationsAsynchronously));
        try
        {
            ct.ThrowIfCancellationRequested();
            if (!AndroidBackgroundChatService.StartNative(context, epoch, identity))
                throw new OperationCanceledException("Demarrage de messagerie obsolete.", ct);
            await pending.Task.WaitAsync(TimeSpan.FromSeconds(30), ct).ConfigureAwait(false);
        }
        catch
        {
            // Cancel only the owner of this attempt, never a later re-enable.
            AndroidBackgroundChatService.Cancel(epoch);
            throw;
        }
        finally
        {
            Startups.TryRemove(epoch, out _);
        }
    }

    internal static void Suspend()
    {
        foreach (var startup in Startups)
            if (!AndroidBackgroundChatService.IsCurrent(startup.Key))
                startup.Value.TrySetException(new OperationCanceledException("Demarrage de messagerie annule."));
        if (_current is not { } current) return;
        current._stop.Cancel();
        current.FailStartup(new OperationCanceledException("Demarrage de messagerie annule."));
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var epoch = intent is null
            ? AndroidBackgroundChatService.Epoch.Restore()
            : intent.GetStringExtra(AndroidBackgroundChatService.EpochExtra);
        try
        {
            if (!AndroidBackgroundChatService.IsCurrent(epoch))
            {
                if (_current == this && AndroidBackgroundChatService.IsCurrent(_epoch))
                    return StartCommandResult.Sticky;
                StopSelfResult(startId);
                return StartCommandResult.NotSticky;
            }
            EnsureChannels();
            var provider = AndroidServiceProvider.GetOrCreate();
            _api = provider.GetRequiredService<YavscApiClient>();
            _transport = provider.GetRequiredService<ChatHubConnection>();
            var identity = intent is null ? ChatBootState.Identity(_api)
                : intent.GetStringExtra(AndroidBackgroundChatService.IdentityExtra);
            if (!ChatBootState.CanStart(_api) || identity is null || identity != ChatBootState.Identity(_api))
                throw new BackgroundChatException(BackgroundChatFailure.SessionRequired,
                    "Connexion de fond inactive : option ou session absente.");
            _api.StartSessionMaintenance();
            if (!AndroidBackgroundChatService.Epoch.Run(epoch, () =>
            {
                if (_epoch == epoch && _loop is { IsCompleted: false } && !_stop.IsCancellationRequested)
                {
                    if (_transport.Connection.State == HubConnectionState.Connected) CompleteStartup(epoch!);
                    return;
                }
                _stop.Cancel();
                _stop = new CancellationTokenSource();
                _epoch = epoch;
                _current = this;
                _wasConnected = false;
                _loop = RunAsync(epoch!, _stop.Token);
            }))
            {
                StopSelfResult(startId);
                return StartCommandResult.NotSticky;
            }
            return StartCommandResult.Sticky;
        }
        catch (Exception ex)
        {
            ReportError(ex);
            if (epoch is not null)
            {
                if (Startups.TryGetValue(epoch, out var pending)) pending.TrySetException(ex);
                AndroidBackgroundChatService.Cancel(epoch);
            }
            if (!AndroidBackgroundChatService.IsCurrent(_epoch)) StopSelfResult(startId);
            return StartCommandResult.NotSticky;
        }
    }

    private async Task RunAsync(string epoch, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && AndroidBackgroundChatService.IsCurrent(epoch))
        {
            if (_api is null || _transport is null || !ChatBootState.CanStart(_api))
            {
                FailStartup(new BackgroundChatException(BackgroundChatFailure.SessionRequired,
                    "La connexion de fond necessite une session active."), epoch);
                AndroidBackgroundChatService.Cancel(epoch);
                return;
            }
            try
            {
                var connection = await _transport.GetConnectionAsync().ConfigureAwait(false);
                if (ct.IsCancellationRequested || !AndroidBackgroundChatService.IsCurrent(epoch)) return;
                Observe(connection);
                if (connection.State == HubConnectionState.Disconnected)
                {
                    using var attempt = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    attempt.CancelAfter(TimeSpan.FromSeconds(20));
                    await _transport.StartAsync(attempt.Token).ConfigureAwait(false);
                }
                if (connection.State == HubConnectionState.Connected)
                {
                    if (!_wasConnected)
                        await AndroidSettings.UpdateAsync(_api.Settings,
                            s =>
                            {
                                if (!_destroyed && AndroidBackgroundChatService.IsCurrent(epoch)
                                    && ChatBootState.CanStart(_api))
                                    s.SetActionStatus(NotificationPermission.IsGranted(this)
                                        ? "Connexion de fond active sans notification permanente. Android peut l'interrompre."
                                        : "SignalR actif sans notification permanente. Alertes desactivees dans Android.",
                                        NotificationPermission.IsGranted(this) ? StatusSeverity.Info : StatusSeverity.Warning);
                            })
                            .ConfigureAwait(false);
                    ct.ThrowIfCancellationRequested();
                    if (!AndroidBackgroundChatService.IsCurrent(epoch)) return;
                    if (!ChatBootState.CanStart(_api)) { AndroidBackgroundChatService.Cancel(epoch); return; }
                    _wasConnected = true;
                    CompleteStartup(epoch);
                }
                else
                {
                    _wasConnected = false;
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                if (!AndroidBackgroundChatService.IsCurrent(epoch)) return;
                _wasConnected = false;
                var failure = new BackgroundChatException(BackgroundChatFailure.ConnectionFailed,
                    "Connexion de fond indisponible ; nouvelle tentative automatique. " + ex.Message, ex);
                ReportError(failure);
                FailStartup(failure, epoch);
            }
            try { await Task.Delay(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
        }
    }

    private void Observe(HubConnection connection)
    {
        lock (_observerGate)
        {
            if (_destroyed || ReferenceEquals(connection, _observedConnection)) return;
            ClearSubscriptions();
            _observedConnection = connection;
            _subscriptions.Add(connection.On<string, string, string>("notifyUser", (type, _, _) =>
            {
                if (type is "pv" or "private" or "private_message" or "PrivateMessage")
                    NotifyMessage("Nouveau message prive. Ouvrez PostIt.");
            }));
            // The current Yavsc hub delivers private chat under addPV.
            _subscriptions.Add(connection.On<string, string>("addPV", (_, _) =>
                NotifyMessage("Nouveau message prive. Ouvrez PostIt.")));
            _subscriptions.Add(connection.On<ChatMessage>("ReceiveMessage", _ =>
                NotifyMessage("Nouveau message dans un salon. Ouvrez PostIt.")));
            _subscriptions.Add(connection.On<string, string>("push", (_, _) =>
                NotifyMessage("Nouvelle notification d'activite. Ouvrez PostIt.")));
        }
    }

    private void NotifyMessage(string text)
    {
        if (_destroyed || _stop.IsCancellationRequested || !AndroidBackgroundChatService.IsCurrent(_epoch)
            || MainActivity.IsForeground
            || _api is null || !ChatBootState.CanStart(_api)
            || !NotificationPermission.IsGranted(this)) return;
        try
        {
            var builder = new NotificationCompat.Builder(this, MessageChannel);
            builder.SetSmallIcon(Yavsc.Resource.Drawable.ic_stat_postit);
            builder.SetContentTitle("PostIt");
            builder.SetContentText(text);
            builder.SetVisibility(NotificationCompat.VisibilityPrivate);
            builder.SetContentIntent(OpenApp());
            builder.SetAutoCancel(true);
            builder.SetOnlyAlertOnce(false);
            GetNotificationManager().Notify(MessageNotificationId, builder.Build());
        }
        catch (Exception ex) { ReportError(ex); }
    }

    private void EnsureChannels()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)) return;
        var manager = (NotificationManager?)GetSystemService(NotificationService)
            ?? throw new InvalidOperationException("Notifications Android indisponibles.");
        manager.CreateNotificationChannel(new NotificationChannel(MessageChannel,
            "Messages et activite", NotificationImportance.Default));
    }

    private PendingIntent? OpenApp()
    {
        var launch = PackageManager?.GetLaunchIntentForPackage(PackageName!);
        return launch is null ? null : PendingIntent.GetActivity(this, 4053, launch,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
    }

    private NotificationManagerCompat GetNotificationManager() =>
        NotificationManagerCompat.From(this)
            ?? throw new InvalidOperationException("Notifications Android indisponibles.");

    private void ReportError(Exception ex)
    {
        Log.Error("PostIt.Chat", ex.ToString());
        if (_api is not null) AndroidSettings.ReportError(_api.Settings, ex.Message);
    }

    private static void CompleteStartup(string epoch)
    {
        if (Startups.TryGetValue(epoch, out var pending)) pending.TrySetResult(true);
    }

    private void FailStartup(Exception ex, string? epoch = null)
    {
        epoch ??= _epoch;
        if (epoch is not null && Startups.TryGetValue(epoch, out var pending)) pending.TrySetException(ex);
    }

    private void ClearSubscriptions()
    {
        lock (_observerGate)
        {
            foreach (var subscription in _subscriptions) subscription.Dispose();
            _subscriptions.Clear();
            _observedConnection = null;
        }
    }

    public override void OnDestroy()
    {
        _destroyed = true;
        _stop.Cancel();
        ClearSubscriptions();
        if (ReferenceEquals(_current, this))
        {
            _current = null;
            FailStartup(new OperationCanceledException("Service de messagerie arrete."));
        }
        // The socket belongs to the shared process session, not this service.
        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent) => null;
}
