using System;
using Android.App;
using Android.Gms.Gcm.Iid;
using Android.Content;

namespace ZicMoove.Droid
{
	[Service(Exported = false), IntentFilter(new[] { "com.google.android.gms.iid.InstanceID" })]
	class MyInstanceIDListenerService : InstanceIDListenerService
	{
		public override void OnTokenRefresh()
		{
			var intent = new Intent (this, typeof (GcmRegistrationIntentService));
			StartService (intent);
		}
	}
}

