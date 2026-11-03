using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.Android.Services;

internal enum BackgroundChatFailure { SessionRequired, StartRejected, ConnectionFailed }

internal sealed class BackgroundChatException(BackgroundChatFailure failure, string message, Exception? inner = null)
    : InvalidOperationException(message, inner)
{
    internal BackgroundChatFailure Failure { get; } = failure;
}

internal sealed class AndroidBackgroundChatService : IBackgroundChatService
{
    internal static readonly AndroidBackgroundChatService Instance = new();
    internal static readonly ChatServiceEpoch Epoch = new();
    internal const string EpochExtra = "postit-chat-epoch";
    internal const string IdentityExtra = "postit-chat-identity";
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SynchronizeAsync(YavscApiClient api, bool enabled, CancellationToken ct = default)
    {
        // This runs before the first await: cancellation cannot queue a stop
        // behind an in-flight permission dialog or a native start.
        if (!enabled || !api.Settings.AndroidKeepChatConnected)
        {
            Invalidate();
            return;
        }
        ct.ThrowIfCancellationRequested();
        if (api.IsSigningOut)
        {
            Invalidate();
            throw new BackgroundChatException(BackgroundChatFailure.SessionRequired, "Deconnexion en cours.");
        }
        var epoch = Epoch.Begin();
        var identity = ChatBootState.Identity(api);
        var entered = false;
        try
        {
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            entered = true;
            Validate(api, epoch, identity, ct);
            if (!Epoch.Run(epoch, () => ChatBootState.Save(api))) throw new OperationCanceledException(ct);
            if (!ChatBootState.CanStart(api))
                throw new BackgroundChatException(BackgroundChatFailure.SessionRequired, "Connectez-vous avant d'activer la connexion de fond.");
            var context = global::Android.App.Application.Context;
            await ChatConnectionService.StartAndWaitAsync(context, epoch, identity!, ct).ConfigureAwait(false);
            Validate(api, epoch, identity, ct);
            if (!NotificationPermission.IsGranted(context))
            {
                if (MainActivity.Current is { } activity && MainActivity.IsForeground)
                {
                    try
                    {
                        var granted = await NotificationPermission.RequestAsync(activity, ct).ConfigureAwait(false);
                        Validate(api, epoch, identity, ct);
                        await AndroidSettings.UpdateAsync(api.Settings, s => s.SetActionStatus(granted
                            ? "Connexion de fond active sans notification permanente. Android peut l'interrompre."
                            : "Notifications refusees : SignalR reste actif, mais les alertes ne seront pas affichees.",
                            granted ? StatusSeverity.Info : StatusSeverity.Warning)).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Log.Error("PostIt.Chat", "Notification permission request failed: " + ex);
                        AndroidSettings.ReportError(api.Settings,
                            "SignalR reste actif ; demande d'autorisation des alertes impossible : " + ex.Message);
                    }
                }
            }
            Validate(api, epoch, identity, ct);
        }
        catch (OperationCanceledException)
        {
            Cancel(epoch);
            throw;
        }
        catch (Exception ex)
        {
            Cancel(epoch);
            Log.Error("PostIt.Chat", ex.ToString());
            try
            {
                await AndroidSettings.UpdateAsync(api.Settings, s => s.SetActionStatus(ex.Message, StatusSeverity.Error))
                    .ConfigureAwait(false);
            }
            catch (Exception statusError) { Log.Error("PostIt.Chat", statusError.ToString()); }
            if (ex is BackgroundChatException) throw;
            throw new BackgroundChatException(BackgroundChatFailure.StartRejected,
                "Android n'a pas pu demarrer le service de messagerie : " + ex.Message, ex);
        }
        finally { if (entered) _gate.Release(); }
    }

    public Task DisableAsync(YavscApiClient api, CancellationToken ct = default)
    {
        Invalidate();
        // Logout calls this before deleting tokens. Never stop the shared
        // socket here: the process-owned messaging session manages it.
        try { ChatBootState.MarkSignedOut(); }
        catch (Exception ex)
        {
            Log.Error("PostIt.Chat", ex.ToString());
            AndroidSettings.ReportError(api.Settings, ex.Message);
        }
        return Task.CompletedTask;
    }

    private static void Validate(YavscApiClient api, string epoch, string? identity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!Epoch.IsCurrent(epoch) || api.IsSigningOut || !api.Settings.AndroidKeepChatConnected
            || !api.HasValidSession || identity is null || identity != ChatBootState.Identity(api))
            throw new OperationCanceledException("Demarrage de messagerie obsolete.", ct);
    }

    internal static bool StartNative(Context context, string epoch, string identity) =>
        Epoch.Run(epoch, () =>
        {
            var intent = new Intent(context, typeof(ChatConnectionService))
                .PutExtra(EpochExtra, epoch).PutExtra(IdentityExtra, identity);
            context.StartService(intent);
        });

    internal static void Invalidate() => Epoch.Invalidate(Stop);
    internal static void Cancel(string epoch) => Epoch.Cancel(epoch, Stop);

    internal static bool IsCurrent(string? epoch) => Epoch.IsCurrent(epoch);

    private static void Stop()
    {
        ChatConnectionService.Suspend();
        global::Android.App.Application.Context.StopService(
            new Intent(global::Android.App.Application.Context, typeof(ChatConnectionService)));
    }
}
