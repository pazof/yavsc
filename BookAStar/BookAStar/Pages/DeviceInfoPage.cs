using BookAStar.Model.Auth.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Yavsc.Models.Identity;

namespace BookAStar.Pages
{
    public class DeviceInfoPage : ContentPage
    {
        public DeviceInfoPage(IGCMDeclaration infos)
        {
            Content = new StackLayout
            {
                Padding = 50,
                VerticalOptions = LayoutOptions.Center,
                Children = {
              new Label{ Text = "Id: " + infos.DeviceId},
              new Label{ Text = "Model: " + infos.Model},
              new Label{ Text = "Platform: " + infos.Platform},
              new Label{ Text = "Version: " + infos.Version},
                    }
            };
      
        }
    }
}
