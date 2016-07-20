using BookAStar.Model.Auth.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace BookAStar
{
    public class DeviceInfoPage : ContentPage
    {
        public DeviceInfoPage(GoogleCloudMobileDeclaration infos)
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
