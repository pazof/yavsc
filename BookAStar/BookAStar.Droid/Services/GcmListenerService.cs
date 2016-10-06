using System;

namespace BookAStar.Droid.Services
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Gms.Gcm;
    using Model.Social;
    using Newtonsoft.Json;
    using Model;

    namespace ClientApp
    {
        [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
        public class MyGcmListenerService : GcmListenerService
        {
            private Notification.Builder notificationBuilder;

            NotificationManager notificationManager;
            
            public override void OnCreate()
            {
                base.OnCreate();
                notificationBuilder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetAutoCancel(true);
                notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                
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
                if (topic == "BookQuery")
                {
                    DateTime eventdate;
                    
                    var sdatestr = data.GetString("EventDate");
                    DateTime.TryParse(sdatestr, out eventdate);

                    var locationJson = data.GetString("Location");
                    var location = JsonConvert.DeserializeObject<Location>(locationJson);
                    var cid = long.Parse(data.GetString("Id"));
                    var bq = new BookQueryData
                    {
                        Id = cid,
                        Location = location
                    };

                    SendBookQueryNotification(bq);
                }
                else
                {
                    throw new NotImplementedException(topic);
                }
            }

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
            }

            void SendBookQueryNotification(BookQueryData bquery)
            {
                var bookquerynotifications = MainSettings.AddBookQueryNotification(bquery);
                var count = bookquerynotifications.Length;
                var multiple = count > 1;
                var title =
                multiple ? $"{count} demandes" : bquery.Client.UserName;
                var message = $"{bquery.EventDate} {bquery.Client.UserName} {bquery.Location.Address}";

                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.PutExtra("BookQueryId", bquery.Id);
               
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);
                Notification.InboxStyle inboxStyle = new Notification.InboxStyle();
                int maxil = 5;
                for (int cn = 0; cn < count && cn < maxil; cn++)
                {
                    inboxStyle.AddLine(bookquerynotifications[cn].Client.UserName);
                }
                if (count > maxil)
                    inboxStyle.SetSummaryText($"Plus {count - maxil} autres");
                else inboxStyle.SetSummaryText((string)null);
                notificationBuilder.SetContentTitle(title).SetContentText(message)
                    .SetStyle(inboxStyle)
                    .SetContentIntent(pendingIntent);

                var notification = notificationBuilder.Build();
                notificationManager.Notify(bookQueryNotificationId, notification );
            }

            int bookQueryNotificationId=1;
        }
    }
}