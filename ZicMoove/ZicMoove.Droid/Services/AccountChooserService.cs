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
using ZicMoove.Droid.OAuth;

namespace ZicMoove.Droid.Services
{
    [Service(
          Name = Constants.ApplicationName + ".AccountChooserService",
          Label = Constants.ApplicationLabel + " accounts service",
          Icon = "@drawable/icon",
          Exported = true,
          Enabled = true
          )]
    [IntentFilter(new String[] { "android.accounts.AccountAuthenticator" })]
    [MetaData("android.accounts.AccountAuthenticator",Resource = "@xml/authenticator")]
    class AccountChooserService : Service
    {
        public static YaOAuth2Authenticator authenticator; 
        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}