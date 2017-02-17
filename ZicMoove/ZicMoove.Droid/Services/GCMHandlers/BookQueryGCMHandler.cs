using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZicMoove.Droid.Services.GCMHandlers
{
    using Interfaces;
    using Model.Social;
    using Data;
    using Model;
    using Model.Musical;

    class BookQueryGCMHandler : GCMessageHandler
    {
        public BookQueryGCMHandler(Context context,
            NotificationManager manager,
            Notification.Builder builder) : base(context,manager,builder)
        {

        }

        /// <summary>
        /// Prend en charge le message push 
        /// contenant une nouvelle demande de rendez-vous
        /// </summary>
        /// <param name="from"></param>
        /// <param name="data"></param>
        public override void Handle(string from, Bundle data)
        {
            var locationJson = data.GetString("Location");
            var location = JsonConvert.DeserializeObject<Location>(locationJson);
            var cid = long.Parse(data.GetString("Id"));
            var clientJson = data.GetString("Client");
            var client = JsonConvert.DeserializeObject<ClientProviderInfo>(clientJson);
            var bq = new BookQuery
            {
                Id = cid,
                Location = location,
                Client = client,
                Reason = data.GetString("Reason")
            };
            var dateString = data.GetString("EventDate");
            DateTime evDate;
            if (DateTime.TryParse(dateString, out evDate))
            {
                bq.EventDate = evDate;
            }
            SendBookQueryNotification(bq);
        }
        /// <summary>
        /// Notifie la demande
        /// </summary>
        /// <param name="bquery"></param>
        void SendBookQueryNotification(BookQuery bquery)
        {
            DataManager.Instance.BookQueries.Merge(bquery);
            var bookquerynotifications = DataManager.Instance.BookQueries.Where(
                q => !q.Read && q.EventDate > DateTime.Now
                ).ToArray();
            var count = bookquerynotifications.Length;
            var multiple = count > 1;
            var title = multiple ? $"{count} demandes" : bquery.Client.UserName;
            var message = $"{bquery.EventDate} {bquery.Client.UserName} {bquery.Location.Address}\n {bquery.Reason}";

            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("BookQueryId", bquery.Id);

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.OneShot);
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
            notificationManager.Notify(bookQueryNotificationId, notification);
        }

        int bookQueryNotificationId = 1;
    }
}
