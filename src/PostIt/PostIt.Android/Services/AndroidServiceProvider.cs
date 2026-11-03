using System;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;

namespace PostIt.Android.Services;

internal static class AndroidServiceProvider
{
    private static readonly Lazy<IServiceProvider> Provider = new(() =>
    {
        var provider = new ServiceCollection().BuildPostItServices();
        var api = provider.GetRequiredService<YavscApiClient>();
        api.Settings.Saved += (_, _) =>
        {
            if (!api.Settings.AndroidKeepChatConnected) AndroidBackgroundChatService.Invalidate();
            ChatBootState.Save(api);
        };
        api.SigningOut += () =>
        {
            AndroidBackgroundChatService.Invalidate();
            try { ChatBootState.MarkSignedOut(); }
            catch (Exception ex)
            {
                global::Android.Util.Log.Error("PostIt.Chat", ex.ToString());
                AndroidSettings.ReportError(api.Settings, ex.Message);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        };
        return provider;
    });

    internal static IServiceProvider GetOrCreate() => Provider.Value;
}
