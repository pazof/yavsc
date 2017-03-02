using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace ZicMoove.Droid.Services.GCMHandlers
{
    public class HairCutQueryGCMHandler : GCMessageHandler
    {
        public HairCutQueryGCMHandler(Context context,
            NotificationManager notificationManager,
            Notification.Builder notificationBuilder) : base(context, notificationManager, notificationBuilder)
        {
        }

        public override void Handle(string from, Bundle data)
        {
            throw new NotImplementedException();
        }
    }
}