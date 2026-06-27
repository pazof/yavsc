using System;

namespace PostIt.Services;

/// <summary>
/// Pure detection helper for the custom URI scheme used by the OIDC
/// callback hand-off (RFC 8252 §7.1). Extracted from the platform
/// entry points (PostIt.Desktop.Program.Main,
/// PostIt.App.OnFrameworkInitializationCompleted) so the matching
/// logic can be unit-tested without dragging in Avalonia or
/// performing the actual <c>Environment.Exit</c> side effect.
///
/// The check is a single string prefix match: <paramref name="args"/>
/// is scanned for the first entry that starts with
/// <c>scheme + "://"</c> (case-insensitive). The scheme itself is
/// exposed as <see cref="Platform.CustomScheme"/> so callers don't
/// have to know whether we're on the postit:// (Desktop), android://
/// (Android), or a third scheme (a future Web variant).
/// </summary>
public static class SchemeUrlDetector
{
    /// <summary>
    /// Returns the first command-line argument that looks like a
    /// custom-scheme callback URL (e.g. <c>postit://callback?code=***</c>),
    /// or <c>null</c> when none is present. Pure: no I/O, no process
    /// exit. Safe to call from any platform entry point.
    /// </summary>
    public static string? FindCallbackUrl(string[] args)
    {
        if (args is null) return null;
        var scheme = Platform.CustomScheme;
        if (string.IsNullOrEmpty(scheme)) return null;
        var prefix = scheme + "://";
        foreach (var arg in args)
        {
            if (arg is null) continue;
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return arg;
        }
        return null;
    }
}
