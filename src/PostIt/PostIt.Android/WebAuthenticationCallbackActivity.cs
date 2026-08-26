using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using PostIt.Droid.Services;

namespace PostIt.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "postit", // Remplacez par votre schéma personnalisé (ex: yavsc ou postit)
    DataHost = "callback")] // Correspond à postit://callback
public class WebAuthenticationCallbackActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Capturer l'URL de redirection OIDC
        var url = Intent?.DataString;

        if (!string.IsNullOrEmpty(url))
        {
            // Transmettre l'URL au gestionnaire partagé pour compléter la Task
            OidcCallbackManager.SetResult(url);
        }

        // Fermer cette activité transparente et ramener l'application au premier plan
        var intent = new Intent(this, typeof(MainActivity));
        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        StartActivity(intent);
        Finish();
    }
}
