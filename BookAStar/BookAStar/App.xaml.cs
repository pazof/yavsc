using BookAStar.Model.Auth.Account;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
/*
Glyphish icons from
	http://www.glyphish.com/
under 
	http://creativecommons.org/licenses/by/3.0/us/
support them by buying the full set / Retina versions
*/

namespace BookAStar
{
    public partial class App : Application // superclass new in 1.3
    {
        SearchPage searchPage;
        NavigationPage mp;
        SettingsPage settingsPage;
        PinPage pinPage;
        ContentPage deviceInfoPage;
        public static IPlatform PlateformSpecificInstance { get; set; }
        public static string AppName { get; set; }
        public static App CurrentApp { get { return Current as App; } }
        public App (IPlatform instance)
		{
            deviceInfoPage = new DeviceInfoPage(instance.GetDeviceInfo());

            PlateformSpecificInstance = instance;
			searchPage = new SearchPage { Title = "Trouvez votre artiste"
					 , Icon = "glyphish_07_map_marker.png"
			};
			mp = new NavigationPage (searchPage);
			settingsPage = new SettingsPage { Title = "Settings"
					,  Icon = "ic_corp_icon.png"
			};

            var r = this.Resources;
            //var hasLabelStyle = r.ContainsKey("labelStyle");

            // var stid = this.StyleId;
            // null var appsstyle = settingsPage.Style;
            // appsstyle.CanCascade = true;
            MainPage = mp;
			ToolbarItem tiSetts = new ToolbarItem () { Text = "Settings"
					, Icon = "ic_corp_icon.png"
            };
			mp.ToolbarItems.Add (tiSetts);
			tiSetts.Clicked += (object sender, EventArgs e) => {
				if (settingsPage.Parent==null)	
				mp.Navigation.PushAsync(settingsPage);
				else {
                    settingsPage.Focus();
                }
			};
			ToolbarItem tiMap = new ToolbarItem { Text = "Carte",
				 Icon = "glyphish_07_map_marker.png" 
			};
			mp.ToolbarItems.Add (tiMap);
			pinPage = new PinPage { Title = "Carte",  
				 Icon = "glyphish_07_map_marker.png"
			};
			tiMap.Clicked += (object sender, EventArgs e) => {
                if (pinPage.Parent == null)
                    mp.Navigation.PushAsync(pinPage);
                else pinPage.Focus();

            };

        }

        public void ShowDeviceInfo()
        {
            if (deviceInfoPage.Parent == null)
                mp.Navigation.PushAsync(deviceInfoPage);
            else deviceInfoPage.Focus();
        }

        public async void RegisterThisDevice()
        {
            HttpClient client = new HttpClient();
            // var request = new HttpRequestMessage(HttpMethod.Post, MainSettings.MobileRegistrationUrl);
            // request.Headers.Authorization
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MainSettings.CurrentUser.YavscTokens.AccessToken);
           /* client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                );*/
            
            var info =PlateformSpecificInstance.GetDeviceInfo();

            /* var serSettings = new JsonSerializerSettings(); serSettings.Formatting = Formatting.None; serSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;*/

            client.MaxResponseContentBufferSize = 256000;
            using ( client)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                /*
                var dataString = JsonConvert.SerializeObject(info,serSettings); */
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(MainSettings.MobileRegistrationUrl, content);
                if (response.IsSuccessStatusCode)
                    Debug.WriteLine(@"Device info successfully saved.");
                    
            }
        }


    }
}

