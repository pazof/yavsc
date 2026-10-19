using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace PostIt.Android.Services;

internal static class PodcastPermissions
{
    private const int RequestCode = 4043;
    private static TaskCompletionSource<bool>? _pending;
    private static string[] _requested = [];

    public static async Task EnsureAsync(Activity activity, bool microphone, CancellationToken ct)
    {
        var requested = new List<string> { Manifest.Permission.Camera };
        if (microphone) requested.Add(Manifest.Permission.RecordAudio);
        if (OperatingSystem.IsAndroidVersionAtLeast(33)) requested.Add(Manifest.Permission.PostNotifications);
        var permissions = requested.ToArray();
        if (permissions.All(p => ContextCompat.CheckSelfPermission(activity, p) == Permission.Granted)) return;
        if (_pending is not null) throw new InvalidOperationException("Une demande de permissions est deja en cours.");
        _requested = permissions;
        var pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending = pending;
        ActivityCompat.RequestPermissions(activity, permissions, RequestCode);
        if (!await pending.Task.WaitAsync(ct))
            throw new UnauthorizedAccessException("Autorisez la camera, les notifications et, si selectionne, le microphone pour diffuser.");
    }

    public static bool HandleResult(int requestCode, string[]? permissions, Permission[]? results)
    {
        if (requestCode != RequestCode) return false;
        var granted = permissions is not null && results is not null && permissions.Length == results.Length
            && _requested.All(p => Array.IndexOf(permissions, p) is var index && index >= 0
                && results[index] == Permission.Granted);
        var pending = _pending;
        _pending = null;
        pending?.TrySetResult(granted);
        return true;
    }
}
