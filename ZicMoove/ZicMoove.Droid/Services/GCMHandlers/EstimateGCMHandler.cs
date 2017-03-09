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
using Newtonsoft.Json;
using ZicMoove.Model.Social;
using ZicMoove.Model;
using ZicMoove.Model.Workflow;
using ZicMoove.Data;
using ZicMoove.Settings;

namespace ZicMoove.Droid.Services.GCMHandlers
{
    class EstimateGCMHandler: GCMessageHandler
    {
        public EstimateGCMHandler(Context context,
            NotificationManager manager,
            Notification.Builder builder) : 
            base(context,manager,builder)
        {

        }

        public override void Handle(string from, Bundle data)
        {
            var locationJson = data.GetString("Location");
            var location = JsonConvert.DeserializeObject<Location>(locationJson);
            var eid = long.Parse(data.GetString("Id"));
            var clientJson = data.GetString("Client");
            var client = JsonConvert.DeserializeObject<ClientProviderInfo>(clientJson);
            var estimate = new Estimate
            {
                Id = eid,
                CommandType = data.GetString("CommandType")
            };
            var dateString = data.GetString("ProviderValidationDate");
            DateTime evDate;
            if (DateTime.TryParse(dateString, out evDate))
            {
                estimate.ProviderValidationDate = evDate;
            }
            Notify(estimate);
        }

        void Notify (Estimate estimate)
        {
            // do merge the data, even when no user is active
            DataManager.Instance.Estimates.Merge(estimate);
            if (MainSettings.CurrentUser == null) return;
            var estimatenotifications = DataManager.Instance.Estimates.Where(
                e => e.ClientApprouvalDate == default(DateTime) &&
                e.ClientId == MainSettings.CurrentUser.Id
                ).OrderByDescending(e=>e.ProviderValidationDate).ToArray();
            var count = estimatenotifications.Length;
            var multiple = count > 1;
            string title;
            string message;
            if (multiple)
            {
                StringBuilder tb = new StringBuilder();
                int nc = 0;
                foreach (var pro in estimatenotifications.Select(
                    e=>e.Owner
                    ).Distinct())
                {
                    nc++;
                    tb.Append($"{pro.UserName}");
                    if (nc > 3)
                    {
                        tb.Append(" et {count-nc} autres");
                        break;
                    }
                    else tb.Append(", ");
                }
                tb.Append("attendent votre validation de leur devis");
                title = tb.ToString();
                message =
                    string.Join("\n",
                    estimatenotifications.Select(
                        n => $"{n.Title} [{n.Owner.UserName}]\n").ToArray());
            }
            else
            {
                title = $"{estimate.Owner.UserName} attend votre validation de son devis";
                message = $"{estimate.Title} ({estimate.Total} euro)\n({estimate.Query.Reason})";
            }

            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("EstimateId", estimate.Id);

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.OneShot);
            Notification.InboxStyle inboxStyle = new Notification.InboxStyle();
            int maxil = 5;
            for (int cn = 0; cn < count && cn < maxil; cn++)
            {
                inboxStyle.AddLine(estimatenotifications[cn].Owner.UserName);
            }
            if (count > maxil)
                inboxStyle.SetSummaryText($"Plus {count - maxil} autres");
            else inboxStyle.SetSummaryText((string)null);
            notificationBuilder.SetContentTitle(title).SetContentText(message)
                .SetStyle(inboxStyle)
                .SetContentIntent(pendingIntent);

            var notification = notificationBuilder.Build();
            notificationManager.Notify(notificationId, notification);
        }

        int notificationId = 2;
    }
}