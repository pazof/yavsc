using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Browser.CustomTabs;
using IdentityModel.OidcClient.Browser;

namespace PostIt.Android.Services;

/// <summary>
/// <see cref="IBrowser"/> implementation that drives Chrome Custom Tabs for
/// the OIDC Authorization Code + PKCE flow. The identity provider redirects
/// to <c>android://postit-signin?code=...&amp;state=...</c>, which Android
/// routes back to the running PostIt instance via the activity-alias
/// declared in <c>AndroidManifest.xml</c>; the resulting Intent URI is
/// handed back through <see cref="MainActivity.AndroidOidcCallbackSink"/>.
/// </summary>
public sealed class AndroidSystemBrowser : IBrowser
{
    private readonly Activity _activity;

    public AndroidSystemBrowser(Activity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, System.Threading.CancellationToken cancellationToken = default)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.StartUrl))
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = "BrowserOptions.StartUrl is empty."
            };
        }

        var uri = global::Android.Net.Uri.Parse(options.StartUrl)!;

        var callbackTask = MainActivity.AndroidOidcCallbackSink.AwaitNextCallbackAsync();

        var tabsIntent = new CustomTabsIntent.Builder()
            .SetShowTitle(true)
            .Build();
        tabsIntent.LaunchUrl(_activity, uri);

        string responseUri;
        try
        {
            responseUri = await callbackTask.WaitAsync(cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }
        catch (Exception ex)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UnknownError,
                Error = $"Failed to await OIDC callback: {ex.Message}"
            };
        }

        if (string.IsNullOrEmpty(responseUri))
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel
            };
        }

        return new BrowserResult
        {
            ResultType = BrowserResultType.Success,
            Response = responseUri
        };
    }
}