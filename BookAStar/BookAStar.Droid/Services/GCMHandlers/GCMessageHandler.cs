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
using BookAStar.Droid.Interfaces;

namespace BookAStar.Droid.Services.GCMHandlers
{
    abstract class GCMessageHandler : IGCMessageHandler
    {
        protected Context context;
        protected NotificationManager notificationManager;
        protected Notification.Builder notificationBuilder;
        public GCMessageHandler(Context context,
            NotificationManager notificationManager,
            Notification.Builder notificationBuilder)
        {
            this.context = context;
            this.notificationBuilder = notificationBuilder;
            this.notificationManager = notificationManager;
        }

        public abstract void Handle(string from, Bundle data);
    }
}