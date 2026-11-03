#if DEBUG
using System;
using System.Threading;
using System.Threading.Tasks;
using PostIt.Android.Services;

namespace PostIt.Android.Tests;

/// <summary>Platform-free deterministic launch/disable races; also invoked by the Android debug activity.</summary>
internal static class ChatServiceEpochTests
{
    internal static async Task VerifyAsync()
    {
        foreach (var reason in new[] { "Sync(false)", "logout", "saved opt-out" })
        {
            var policy = new ChatServiceEpoch();
            var queued = policy.Begin();
            var stopped = 0;
            policy.Invalidate(() => stopped++);
            Require(!policy.Run(queued, () => throw new Exception("Stale launch executed")), reason);
            Require(policy.Restore() is null, reason + " restored in same process");
            Require(stopped == 1, reason + " did not stop synchronously");
        }

        var owner = new ChatServiceEpoch();
        var old = owner.Begin();
        owner.Invalidate(() => { });
        var newer = owner.Begin();
        var laterStopped = false;
        Require(!owner.Cancel(old, () => laterStopped = true), "old timeout cancelled later enable");
        Require(!laterStopped && owner.IsCurrent(newer), "new generation lost");
        Require(owner.Cancel(newer, () => laterStopped = true), "current cancellation not applied");
        Require(laterStopped && !owner.IsCurrent(newer), "timed-out generation still launchable");
        Require(!owner.Run(newer, () => throw new Exception("Timed-out intent executed")), "timeout leak");

        var firstProcess = new ChatServiceEpoch();
        var staleIntent = firstProcess.Begin();
        var freshProcess = new ChatServiceEpoch();
        var restored = freshProcess.Restore();
        Require(restored is not null && freshProcess.IsCurrent(restored), "fresh boot restore failed");
        Require(!freshProcess.IsCurrent(staleIntent), "previous-process intent accepted");

        var queuedPolicy = new ChatServiceEpoch();
        var beforeSemaphore = queuedPolicy.Begin();
        using var release = new SemaphoreSlim(0, 1);
        var launched = false;
        var delayedLaunch = Task.Run(async () =>
        {
            await release.WaitAsync();
            queuedPolicy.Run(beforeSemaphore, () => launched = true);
        });
        queuedPolicy.Invalidate(() => { });
        release.Release();
        await delayedLaunch;
        Require(!launched, "queued semaphore/permission continuation launched after disable");

        var concurrent = new ChatServiceEpoch();
        var concurrentEpoch = concurrent.Begin();
        var nativeLaunch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishLaunch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var launch = Task.Run(() => concurrent.Run(concurrentEpoch, () =>
        {
            nativeLaunch.TrySetResult();
            finishLaunch.Task.GetAwaiter().GetResult();
        }));
        await nativeLaunch.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var stop = Task.Run(() => concurrent.Invalidate(() => { }));
        finishLaunch.TrySetResult();
        await Task.WhenAll(launch, stop).WaitAsync(TimeSpan.FromSeconds(5));
        Require(!concurrent.IsCurrent(concurrentEpoch), "native launch escaped concurrent disable");
        Require(!concurrent.Run(concurrentEpoch, () => throw new Exception("Delayed Android intent executed")),
            "delayed Android intent accepted");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
#endif
