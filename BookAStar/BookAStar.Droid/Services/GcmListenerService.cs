using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BookAStar.Droid.Services
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Gms.Gcm;
    using Android.Util;

    namespace ClientApp
    {
        [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
        public class MyGcmListenerService : GcmListenerService
        {
            public override void OnMessageReceived(string from, Bundle data)
            {
                var message = data.GetString("message");
                Log.Debug("MyGcmListenerService", "From:    " + from);
                Log.Debug("MyGcmListenerService", "Message: " + message);
                SendNotification(message);
            }

            void SendNotification(string message)
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

                var notificationBuilder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetContentTitle("GCM Message")
                    .SetContentText(message)
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent);

                var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                notificationManager.Notify(0, notificationBuilder.Build());
            }
        }
    }
}