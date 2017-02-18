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

namespace ZicMoove.Droid
{
    [Activity(Label = "SendFileActivity")]
    [MetaData("android.service.chooser.chooser_target_service", Value = Constants.ApplicationName + ".YavscChooserTargetService")]
    [IntentFilter(new[] { "android.intent.action.SEND" },
        Categories = new[] { "android.intent.category.DEFAULT" },
        Icon = "@drawable/icon",
        Label = Constants.SendToApp)]
    public class SendFileActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }
    }
}