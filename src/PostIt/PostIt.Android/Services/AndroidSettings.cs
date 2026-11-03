using System;
using System.Threading.Tasks;
using PostIt.ViewModels;

namespace PostIt.Android.Services;

internal static class AndroidSettings
{
    internal static Task UpdateAsync(Settings settings, Action<Settings> update)
    {
        if (MainActivity.Current is not { IsDestroyed: false, IsFinishing: false } activity)
        {
            update(settings);
            return Task.CompletedTask;
        }
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        activity.RunOnUiThread(() =>
        {
            try
            {
                update(settings);
                completion.TrySetResult();
            }
            catch (Exception ex) { completion.TrySetException(ex); }
        });
        return completion.Task;
    }

    internal static void ReportError(Settings settings, string message)
    {
        _ = ReportAsync();
        async Task ReportAsync()
        {
            try { await UpdateAsync(settings, s => s.SetActionStatus(message, PostIt.Models.StatusSeverity.Error)); }
            catch (Exception ex) { global::Android.Util.Log.Error("PostIt.Chat", ex.ToString()); }
        }
    }
}
