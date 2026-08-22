using Microsoft.Maui.Controls;

namespace PostIt.Maui;

/// <summary>
/// Empty MAUI application registered as the embedding host for
/// <c>Avalonia.Maui</c>. <c>Avalonia.Maui</c> expects a
/// <see cref="Microsoft.Maui.Controls.Application"/>-derived type so that
/// its <c>.UseMaui&lt;TMauiApp&gt;(activity)</c> extension can build the
/// embedding pipeline (services, handlers, <c>IPlatformApplication</c>).
///
/// We never actually display any MAUI controls — the embedding sits in front
/// of Avalonia so that Avalonia's Android platform layer (which is otherwise
/// bound to a stub on Avalonia 12 / present-but-broken on Avalonia 11 without
/// MAUI) gets the real Android <c>Context</c> and runs the surface view
/// lifecycle correctly. This class exists to satisfy the generic constraint
/// of <c>UseMaui&lt;TMauiApp&gt;</c> and gives MAUI something to attach its
/// handler tree to.
///
/// Override <c>CreateWindow</c> with a no-op or leave the default; the
/// virtual MAUI window is consumed by <c>Avalonia.Maui</c>'s embedding and
/// never surfaces to the user (Avalonia owns the visual tree).
/// </summary>
public class MauiEmbeddingApp : Application
{
    public MauiEmbeddingApp()
    {
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // No-op window: Avalonia.Maui wraps it and never shows it.
        return new Window();
    }
}