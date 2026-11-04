using System;
using Android.Content;
using Android.OS;
using Android.Util;
using PostIt.ViewModels;
using AndroidSettingsIntent = Android.Provider.Settings;

namespace PostIt.Android.Services;

internal static class AndroidBatteryOptimization
{
    internal static Intent? CreateIntent(Context context)
    {
        var packageName = context.PackageName
            ?? throw new InvalidOperationException("Identifiant de l'application Android indisponible.");
        var power = context.GetSystemService(Context.PowerService) as PowerManager
            ?? throw new InvalidOperationException("Gestion de la batterie Android indisponible.");
        return power.IsIgnoringBatteryOptimizations(packageName)
            ? null
            : new Intent(AndroidSettingsIntent.ActionRequestIgnoreBatteryOptimizations)
                .SetData(global::Android.Net.Uri.Parse("package:" + packageName));
    }

    internal static void Request(Settings settings)
    {
        if (MainActivity.Current is not { IsFinishing: false, IsDestroyed: false } activity
            || !MainActivity.IsForeground)
        {
            AndroidSettings.ReportError(settings,
                "Ouvrez PostIt pour demander l'exemption d'optimisation batterie.");
            return;
        }
        activity.RunOnUiThread(() =>
        {
            try
            {
                if (!MainActivity.IsForeground || activity.IsFinishing || activity.IsDestroyed)
                    throw new InvalidOperationException("PostIt doit etre visible pour ouvrir les parametres batterie.");
                if (!settings.AndroidKeepChatConnected) return;
                using var intent = CreateIntent(activity);
                if (intent is not null) activity.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Log.Error("PostIt.Chat", "Battery optimization settings failed: " + ex);
                AndroidSettings.ReportError(settings,
                    "Impossible d'ouvrir les parametres batterie Android : " + ex.Message);
            }
        });
    }
}
