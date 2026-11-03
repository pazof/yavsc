using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace PostIt.Android.Services;

/// <summary>Runtime POST_NOTIFICATIONS request (Android 13+), routed through MainActivity.</summary>
internal static class NotificationPermission
{
    private const int RequestCode = 4051;
    private const string AskedKey = "notification-permission-asked";
    private const string PermissionName = "android.permission.POST_NOTIFICATIONS";
    private static TaskCompletionSource<bool>? _pending;

    internal static bool IsGranted(Context context)
        => (!OperatingSystem.IsAndroidVersionAtLeast(33)
                || ContextCompat.CheckSelfPermission(context, PermissionName) == Permission.Granted)
            && NotificationManagerCompat.From(context)?.AreNotificationsEnabled() == true;

    internal static async Task<bool> RequestAsync(Activity activity, CancellationToken ct)
    {
        if (IsGranted(activity)) return true;
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return false; // Disabled from system settings: no runtime prompt exists.

        if (!MainActivity.IsForeground || activity.IsFinishing || activity.IsDestroyed)
            return false;
        ct.ThrowIfCancellationRequested();
        if (_pending is { } existing)
            return await existing.Task.WaitAsync(ct).ConfigureAwait(false);
        var preferences = activity.GetSharedPreferences("postit-chat-service", FileCreationMode.Private)!;
        if (preferences.GetBoolean(AskedKey, false))
            return false; // Subsequent retries must use Android notification settings.

        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (Interlocked.CompareExchange(ref _pending, pending, null) is { } inFlight)
            return await inFlight.Task.WaitAsync(ct).ConfigureAwait(false);

        activity.RunOnUiThread(() =>
        {
            try
            {
                if (!MainActivity.IsForeground || activity.IsFinishing || activity.IsDestroyed)
                {
                    Interlocked.CompareExchange(ref _pending, null, pending);
                    pending.TrySetResult(false);
                    return;
                }
                if (!preferences.Edit()!.PutBoolean(AskedKey, true)!.Commit())
                    throw new System.IO.IOException("Impossible d'enregistrer la demande d'autorisation Android.");
                ActivityCompat.RequestPermissions(activity, [PermissionName], RequestCode);
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref _pending, null, pending);
                pending.TrySetException(ex);
            }
        });

        // Cancellation cancels only this waiter, not the OS request. Retain the
        // request until its callback so retries cannot open a second dialog.
        return await pending.Task.WaitAsync(ct).ConfigureAwait(false);
    }

    internal static bool HandleResult(int requestCode, string[]? permissions, Permission[]? results)
    {
        if (requestCode != RequestCode) return false;
        var granted = false;
        if (permissions is not null && results is not null)
        {
            var index = Array.IndexOf(permissions, PermissionName);
            granted = index >= 0 && index < results.Length && results[index] == Permission.Granted;
        }
        Interlocked.Exchange(ref _pending, null)?.TrySetResult(granted);
        return true;
    }

    internal static void OnActivityDestroyed() =>
        Interlocked.Exchange(ref _pending, null)?.TrySetResult(false);
}
