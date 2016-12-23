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
using BookAStar.Droid.OAuth;

namespace BookAStar.Droid.Services
{
    [Service(
          Name = "fr.pschneider.bas.AccountChooserService",
          Label = "Yavsc accounts service",
          Icon = "@drawable/icon",
          Exported = true,
          Enabled = true
          )]
    [IntentFilter(new String[] { "android.accounts.AccountAuthenticator" })]

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