using System;
using Android.App;
using Android.Content;
using Android.Util;
using System.Net;
using System.IO;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using Android.OS;
using Android;

namespace BookAStar.Droid
{

	[Service(Exported = false)]
	class GcmRegistrationIntentService : IntentService
	{
		static object locker = new object();

		public GcmRegistrationIntentService() : base("RegistrationIntentService") {
			
		}


		static PowerManager.WakeLock sWakeLock;
		static object LOCK = new object();

        public override void OnCreate()
        {
            base.OnCreate();
            sWakeLock = PowerManager.FromContext(this).NewWakeLock(WakeLockFlags.Partial,
                "BookAStar");
            sWakeLock.Acquire();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            sWakeLock.Release();
        }
        protected override void OnHandleIntent (Intent intent)
		{
			try
			{
				Log.Info ("RegistrationIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
                    
                    var instanceID = InstanceID.GetInstance (this);
					var senderid = MainSettings.GoogleSenderId;
					var token = instanceID.GetToken ( senderid,
						 GoogleCloudMessaging.InstanceIdScope, null);

					Log.Info ("RegistrationIntentService", "GCM Registration Token: " + token);
					SendRegistrationToAppServer (token);
					Subscribe (token);
				}
			}
			catch (WebException e) {
				Log.Debug ("RegistrationIntentService", "Failed to get a registration token");
				if (e.Response!=null)
				using (var s = e.Response.GetResponseStream ()) {
					using (var r = new StreamReader (s)) {
						var t = r.ReadToEnd ();
						Log.Debug("RegistrationIntentService",t);
					}
				}
				return;
			}
			catch (Exception e)
			{
				Log.Error ("RegistrationIntentService", "Failed to get a registration token");
				Log.Error ("RegistrationIntentService", e.Message);
				return;
			}
		}

		void SendRegistrationToAppServer (string token)
		{
            MainSettings.GoogleRegId = token;
		}

		void Subscribe (string token)
		{
			var pubSub = GcmPubSub.GetInstance(this);
			pubSub.Subscribe(token, "/topics/global", null);
			pubSub.Subscribe (token, "/topics/jobs", null);
		}
	}
	
}

