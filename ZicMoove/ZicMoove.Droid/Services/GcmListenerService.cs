
using Android.App;
using Android.Content;
using Android.OS;
using Android.Gms.Gcm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZicMoove.Droid.Services
{
    using Model.Social;
    using Model;
    using Data;
    using Interfaces;
    using GCMHandlers;

    namespace ClientApp
    {
        [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
        public class MyGcmListenerService : GcmListenerService
        {
            private Notification.Builder notificationBuilder;

            NotificationManager notificationManager;
            Dictionary<string, IGCMessageHandler> Handlers;
            public override void OnCreate()
            {
                base.OnCreate();
                notificationBuilder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetAutoCancel(true);
                notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                Handlers = new Dictionary<string, IGCMessageHandler>
                {
                    {"RdvQuery", new BookQueryGCMHandler(this,notificationManager,notificationBuilder) },
                    {"Estimation", new EstimateGCMHandler(this,notificationManager,notificationBuilder) },
                    {"HairCutQuery", new HairCutQueryGCMHandler(this,notificationManager,notificationBuilder) },

                };
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                notificationManager.Dispose();
                notificationManager = null;
                notificationBuilder.Dispose();
                notificationBuilder = null;
            }

            public override void OnMessageReceived(string from, Bundle data)
            {
                var topic = data.GetString("Topic");
                if (Handlers.ContainsKey(topic))
                {
                    Handlers[topic].Handle(from, data);
                }
                else
                {
                    throw new NotImplementedException(topic);
                }
            }
            /* TODO cleaning
            void SendNotification(string title, string message)
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

                var notificationBuilder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent);

                notificationManager.Notify(0, notificationBuilder.Build());
            }*/
        }
            
    }
}