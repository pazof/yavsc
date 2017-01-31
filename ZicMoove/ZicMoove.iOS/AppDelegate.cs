using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using ZicMoove;
using ZicMoove.Interfaces;
using Yavsc.Models.Identity;

namespace App2.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate,
        IPlatform
    {
        public string GCMStatusMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddAccount()
        {
            throw new NotImplementedException();
        }

        public bool EnablePushNotifications(bool enable)
        {
            throw new NotImplementedException();
        }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App(this));

            return base.FinishedLaunching(app, options);
        }

        public IGCMDeclaration GetDeviceInfo()
        {
            throw new NotImplementedException();
        }

        public object InvokeApi(string method, object arg)
        {
            throw new NotImplementedException();
        }

        public TAnswer InvokeApi<TAnswer>(string method, object arg)
        {
            throw new NotImplementedException();
        }

        public void OpenWeb(string Uri)
        {
            throw new NotImplementedException();
        }

        public void RevokeAccount(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
