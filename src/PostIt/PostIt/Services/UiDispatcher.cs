using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace PostIt.Services;

/// <summary>
/// Tiny marshalling helper around <see cref="Dispatcher.UIThread"/> so
/// the rest of the codebase does not have to import Avalonia.Threading
/// directly. We want exactly one place that decides "is the current
/// thread the Avalonia UI thread, and if not, post there" so that
/// <see cref="ObservableObject"/>-derived types (Settings, the various
/// ViewModels) can fire <c>PropertyChanged</c> safely from background
/// work — which is exactly the cross-thread case that previously blew
/// up inside <c>DataValidationErrors.SetErrors</c> on Avalonia 11.
///
/// The helper is intentionally tiny: a sync post when we are off the
/// UI thread, a no-op when we are already on it, and an async fire-
/// and-forget variant for places where awaiting would deadlock the
/// caller (e.g. <c>Settings.Load</c> continuation paths).
/// </summary>
public static class UiDispatcher
{
    /// <summary>
    /// True when the calling thread is the Avalonia UI thread. Property
    /// setters that touch bindings should check this before mutating
    /// state; the safe path is <see cref="InvokeIfNeeded"/>.
    /// </summary>
    public static bool IsOnUiThread => Dispatcher.UIThread.CheckAccess();

    /// <summary>
    /// Run <paramref name="action"/> on the UI thread. If the caller is
    /// already on the UI thread, run synchronously to preserve stack
    /// traces and ordering; otherwise post to the dispatcher and wait.
    /// Never throws on shutdown — a missing dispatcher is treated as
    /// "best-effort skipped", matching Avalonia's own behaviour when
    /// the application lifetime has been torn down.
    /// </summary>
    public static void InvokeIfNeeded(Action action)
    {
        if (action is null) return;
        if (IsOnUiThread) { action(); return; }
        try { Dispatcher.UIThread.Post(action, DispatcherPriority.Normal); }
        catch (InvalidOperationException) { /* dispatcher gone, nothing to do */ }
    }

    /// <summary>
    /// Fire-and-forget variant: schedules <paramref name="action"/> on
    /// the UI thread but does not block the caller. Use this from
    /// background workers (OIDC discovery, HTTP callbacks, file I/O)
    /// where awaiting the dispatcher would deadlock the calling sync
    /// context.
    /// </summary>
    public static void Post(Action action)
    {
        if (action is null) return;
        try { Dispatcher.UIThread.Post(action, DispatcherPriority.Normal); }
        catch (InvalidOperationException) { /* dispatcher gone */ }
    }

    /// <summary>
    /// Awaitable variant. Useful inside <c>async</c> ViewModel methods
    /// that must touch bindings only after the dispatcher has processed
    /// a queued update (e.g. "load file then refresh observable state").
    /// </summary>
    public static Task InvokeAsync(Action action)
    {
        if (action is null) return Task.CompletedTask;
        if (IsOnUiThread) { action(); return Task.CompletedTask; }
        return Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal).GetTask();
    }
}
