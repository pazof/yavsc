using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Android.Content;
using PostIt.Services;

namespace PostIt.Android.Services;

internal static class ChatBootState
{
    private static ISharedPreferences Preferences =>
        global::Android.App.Application.Context.GetSharedPreferences("postit-chat-service", FileCreationMode.Private)!;

    internal static bool OptedIn => Preferences.GetBoolean("enabled", false);

    internal static void Save(YavscApiClient api)
    {
        var identity = Identity(api);
        var saved = Preferences.Edit()!
            .PutBoolean("enabled", api.Settings.AndroidKeepChatConnected)!
            .PutBoolean("signed-in", !api.IsSigningOut && api.HasValidSession && identity is not null)!
            .PutString("identity", identity)!.Commit();
        if (!saved) throw new IOException("Impossible d'enregistrer l'etat Android de la messagerie.");
    }

    internal static void MarkSignedOut()
    {
        if (!Preferences.Edit()!.PutBoolean("signed-in", false)!.Commit())
            throw new IOException("Impossible d'enregistrer la deconnexion Android.");
    }

    internal static bool CanStart(YavscApiClient api)
    {
        if (!OptedIn || !api.Settings.AndroidKeepChatConnected || api.IsSigningOut || !api.HasValidSession)
            return false;

        var identity = Identity(api);
        if (identity is null) return false;

        var signedIn = Preferences.GetBoolean("signed-in", false);
        var storedIdentity = Preferences.GetString("identity", null);

        // A valid refresh-capable session can coexist with stale native prefs
        // (for example after app relaunch or an interrupted token renewal).
        // Treat missing/lagging prefs as recoverable instead of rejecting the
        // start outright, but keep strict identity matching when both values exist.
        if (!signedIn && string.IsNullOrEmpty(storedIdentity)) return true;
        if (signedIn && !string.IsNullOrEmpty(storedIdentity)) return identity == storedIdentity;
        return true;
    }

    internal static string? Identity(YavscApiClient api)
    {
        var token = api.CurrentIdToken ?? api.CurrentAccessToken;
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            if (!json.RootElement.TryGetProperty("sub", out var subject)
                || string.IsNullOrEmpty(subject.GetString())) return null;
            return $"{api.Settings.ApiUrl.TrimEnd('/')}\n{api.Settings.Authentication.Authority.TrimEnd('/')}\n{subject.GetString()}";
        }
        catch (Exception ex) when (ex is FormatException or JsonException or InvalidOperationException)
        {
            global::Android.Util.Log.Warn("PostIt.Chat", "Session identity unavailable for boot restart.");
            return null;
        }
    }
}
