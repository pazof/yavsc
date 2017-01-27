using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Util;
using Android.Widget;
using ZicMoove.Settings;

namespace ZicMoove.Droid
{
	[Service]
	public class MyGcmIntentService : IntentService
	{
		static PowerManager.WakeLock sWakeLock;
		static object LOCK = new object();

		public static void RunIntentInService(Context context, Intent intent)
		{
			lock (LOCK)
			{
				if (sWakeLock == null)
				{
					// This is called from BroadcastReceiver, there is no init.
					var pm = PowerManager.FromContext(context);
					sWakeLock = pm.NewWakeLock(
						WakeLockFlags.Partial, "My WakeLock Tag");
				}
			}

			sWakeLock.Acquire();
			intent.SetClass(context, typeof(MyGcmIntentService));
			context.StartService(intent);
		}

		static object locker = new object();

		protected override void OnHandleIntent(Intent intent)
		{
			try
			{
				Log.Info ("MyIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
					string action = intent.Action;
					if (action!=null)
					if (action.Equals("com.google.android.c2dm.intent.REGISTRATION"))
					{
						HandleRegistration(intent);
					}
					else if (action.Equals("com.google.android.c2dm.intent.RECEIVE"))
					{
						HandleMessage(intent);
					}
				}
			}
			finally
			{
				lock (LOCK)
				{
					//Sanity check for null as this is a public method
					if (sWakeLock != null)
						sWakeLock.Release();
				}
			}
		}

		private void HandleMessage(Intent intent)
		{
			// get the notification type id:
			string ntft = intent.GetStringExtra("type");
			string msg = intent.GetStringExtra("Description");
			var position = intent.GetSerializableExtra ("Location");
			SendNotification (msg);
		}

		void SendNotification (string message)
		{
           /* Bundle valuesForActivity = new Bundle();
            valuesForActivity.PutInt("count", count); */

            var intent = new Intent (this, typeof(MainActivity));
			intent.AddFlags (ActivityFlags.ClearTop);
			var pendingIntent = PendingIntent.GetActivity (this, 0, intent, PendingIntentFlags.OneShot);
            // Construct a back stack for cross-task navigation:
            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(intent);

            // Create the PendingIntent with the back stack:            
            PendingIntent resultPendingIntent =
                stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new Notification.Builder(this)
                .SetAutoCancel(true)
				.SetSmallIcon (Resource.Drawable.icon)
				.SetContentTitle ("GCM Message")
				.SetContentText (message)
                .SetContentIntent(resultPendingIntent)  // Start 2nd activity when the intent is clicked.
                ;

			var notificationManager = (NotificationManager) GetSystemService(Context.NotificationService);
			notificationManager.Notify (0, notificationBuilder.Build());
		}

		private void HandleRegistration(Intent intent)
		{
			string registrationId = intent.GetStringExtra("registration_id");
			string error = intent.GetStringExtra("error");
			string unregistration = intent.GetStringExtra("unregistered");
		}

		void SubscribeGCM ()
		{
			Context context = this.ApplicationContext;
			string senders = MainSettings.GoogleSenderId; 
			// Resources.GetString(GoogleSenderId);
			Intent intent = new Intent ("com.google.android.c2dm.intent.REGISTER");
			intent.SetPackage ("com.google.android.gsf");
			intent.PutExtra ("app", PendingIntent.GetBroadcast (context, 0, new Intent (), 0));
			intent.PutExtra ("sender", senders);
			context.StartService (intent);
		}

		void UnsubscribeGCM ()
		{
			Context context = this.ApplicationContext;
			Intent intent = new Intent("com.google.android.c2dm.intent.UNREGISTER");
			intent.PutExtra("app", PendingIntent.GetBroadcast(context, 0, new Intent(), 0));
			context.StartService (intent);
		}


	}
}

