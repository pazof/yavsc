#if ANDROID
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Avalonia.Controls;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.Android;
using PostIt.Android.Services;
using PostIt.Services;

namespace PostIt.Tests;

[Activity(Name = "fr.pschneider.postit.ScreenAwakeTestActivity", Exported = true)]
public sealed class AndroidScreenAwakeTestActivity : Activity
{
    protected override async void OnCreate(Bundle? state)
    {
        base.OnCreate(state);
        var result = Path.Combine(FilesDir!.AbsolutePath, "screen-awake-test-result.txt");
        File.WriteAllText(result, "RUNNING");
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await RunAsync(timeout.Token);
            File.WriteAllText(result, "PASS\nplayback,pause,resume,stop,end,teardown,overlap,background,foreground");
        }
        catch (Exception ex) { File.WriteAllText(result, "FAIL\n" + ex); }
        finally { Finish(); }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        StartActivity(new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask));
        await WaitAsync(() => MainActivity.IsForeground && (Avalonia.Application.Current as App)?.View is not null, ct);
        var fixture = Path.Combine(FilesDir!.AbsolutePath, "screen-awake-fixture.mp4");
        if (!File.Exists(fixture)) throw new IOException("An explicit local video fixture is required.");
        var app = (App)Avalonia.Application.Current!;
        var navigation = app.View!.FindControl<NavigationPage>("NavRoot")
            ?? throw new InvalidOperationException("Main navigation unavailable.");
        using var vlc = new LibVLC();
        using var player = new MediaPlayer(vlc);
        AndroidVideoSurface? surface = null;
        IDisposable? capture = null;
        var attached = false;
        try
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                surface = new AndroidVideoSurface(player, false);
                await navigation.PushAsync(new ContentPage { Content = surface });
                attached = true;
            });
            await surface!.Ready.WaitAsync(ct);
            await WaitAsync(() => !ScreenIsKeptOn(), ct);
            using var repeated = new Media(vlc, fixture, FromType.FromPath);
            repeated.AddOption(":input-repeat=100");
            if (!player.Play(repeated)) throw new InvalidOperationException("Playback did not start.");
            await WaitAsync(() => player.IsPlaying && ScreenIsKeptOn(), ct);
            player.Pause();
            await WaitAsync(() => !player.IsPlaying && !ScreenIsKeptOn(), ct);
            if (!player.Play()) throw new InvalidOperationException("Playback did not resume.");
            await WaitAsync(ScreenIsKeptOn, ct);
            capture = Platform.ScreenAwake.Acquire();
            player.Stop();
            await WaitAsync(() => !player.IsPlaying && Platform.ScreenAwake.IsRequested && ScreenIsKeptOn(), ct);
            capture.Dispose();
            capture = null;
            await WaitAsync(() => !ScreenIsKeptOn(), ct);

            using var once = new Media(vlc, fixture, FromType.FromPath);
            var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            player.EndReached += (_, _) => ended.TrySetResult();
            if (!player.Play(once)) throw new InvalidOperationException("Finite playback did not start.");
            await WaitAsync(ScreenIsKeptOn, ct);
            await ended.Task.WaitAsync(ct);
            await WaitAsync(() => !ScreenIsKeptOn(), ct);

            if (!player.Play(repeated)) throw new InvalidOperationException("Teardown playback did not start.");
            await WaitAsync(ScreenIsKeptOn, ct);
            await Dispatcher.UIThread.InvokeAsync(() => surface.Dispose());
            await WaitAsync(() => !ScreenIsKeptOn() && !Platform.ScreenAwake.IsRequested, ct);
            player.Stop();
            capture = Platform.ScreenAwake.Acquire();
            await WaitAsync(ScreenIsKeptOn, ct);
            StartActivity(new Intent(Intent.ActionMain).AddCategory(Intent.CategoryHome).AddFlags(ActivityFlags.NewTask));
            await WaitAsync(() => !MainActivity.IsForeground && !ScreenIsKeptOn(), ct);
            StartActivity(new Intent(this, typeof(MainActivity)).AddFlags(ActivityFlags.NewTask));
            await WaitAsync(() => MainActivity.IsForeground && ScreenIsKeptOn(), ct);
            capture.Dispose();
            capture = null;
            await WaitAsync(() => !ScreenIsKeptOn(), ct);
        }
        finally
        {
            capture?.Dispose();
            await Dispatcher.UIThread.InvokeAsync(() => surface?.Dispose());
            player.Stop();
            if (attached) await Dispatcher.UIThread.InvokeAsync(async () => await navigation.PopAsync());
        }
    }

    private static bool ScreenIsKeptOn()
        => MainActivity.Current?.Window?.Attributes is { } attributes
            && (attributes.Flags & WindowManagerFlags.KeepScreenOn) != 0;

    private static async Task WaitAsync(Func<bool> condition, CancellationToken ct)
    {
        while (!await Dispatcher.UIThread.InvokeAsync(condition))
            await Task.Delay(50, ct);
    }
}
#endif
