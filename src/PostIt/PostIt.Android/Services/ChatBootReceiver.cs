using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.Android.Services;

[BroadcastReceiver(Name = "fr.pschneider.postit.ChatBootReceiver", Enabled = true, Exported = true)]
[IntentFilter([Intent.ActionBootCompleted])]
public sealed class ChatBootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent?.Action != Intent.ActionBootCompleted || !ChatBootState.OptedIn) return;
        var epoch = AndroidBackgroundChatService.Epoch.Restore();
        if (epoch is null) return;
        var pending = GoAsync();
        _ = Task.Run(() =>
        {
            YavscApiClient? api = null;
            try
            {
                api = AndroidServiceProvider.GetOrCreate().GetRequiredService<YavscApiClient>();
                if (!ChatBootState.CanStart(api)) return;
                var identity = ChatBootState.Identity(api);
                if (identity is null || !AndroidBackgroundChatService.StartNative(context, epoch, identity)) return;
                Log.Info("PostIt.Chat", "Best-effort chat restart requested after boot.");
            }
            catch (Exception ex)
            {
                Log.Error("PostIt.Chat", "Boot restart rejected: " + ex);
                if (api is not null)
                    AndroidSettings.ReportError(api.Settings,
                        "Android a refuse le redemarrage de la messagerie : " + ex.Message);
            }
            finally { pending?.Finish(); }
        });
    }
}
