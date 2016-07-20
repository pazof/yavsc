using System;

using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Gms.Common;
using Android.Util;

using Xamarin.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using BookAStar.Model.Auth.Account;
using BookAStar.Droid.OAuth;

namespace BookAStar.Droid
{
    [Activity(Name="fr.pschneider.bas.MainActivity", Label = "BookAStar", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity,
        IPlatform
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);
            LoadApplication(new App(this));
        }


        public bool EnablePushNotifications(bool enable)
        {
            if (enable)
                return StartNotifications();
            else
                return StopNotifications();
        }

        private string gCMStatusMessage = null;
        public string GCMStatusMessage
        {
            get
            {
                return gCMStatusMessage;
            }
        }

        bool StartNotifications()
        {
            if (IsPlayServicesAvailable(out gCMStatusMessage))
            {
                var intent = new Intent(this, typeof(GcmRegistrationIntentService));
                StartService(intent);
                return true;
            }
            return false;
        }


        bool StopNotifications()
        {
            return StopService(new Intent(this, typeof(GcmRegistrationIntentService)));
        }

        public void RevokeAccount(string userName)
        {

            var accStore = AccountStore.Create(this);
            var accounts = accStore.FindAccountsForService(MainSettings.ApplicationName);

            accStore.Delete(
                    accounts.Where(a => a.Username == userName).FirstOrDefault()
                    , MainSettings.ApplicationName);
            Toast.MakeText(this,
                Resource.String.yavscIdentRemoved
                , ToastLength.Short);
        }

        public void OpenWeb(string Uri)
        {
            var u = global::Android.Net.Uri.Parse(Uri);
            Intent i = new Intent(Intent.ActionView, u);
            StartActivity(i);
        }


        protected override void OnStart()
        {
            base.OnStart();
            if (MainSettings.PushNotifications)
                StartNotifications();
        }


        public bool IsPlayServicesAvailable(out string msgText)
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    msgText = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    msgText = "Sorry, this device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                msgText = "Google Play Services is available.";
                return true;
            }
        }
        public void UploadJson(string data)
        {
            try
            {
                string url = "http://lua.pschneider.fr/api/BackOffice/SetRegistrationId";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                //using GET - request.Headers.Add ("Authorization","Authorizaation value");
                request.ContentType = "application/json";
                HttpWebResponse myResp = (HttpWebResponse)request.GetResponse();
                string responseText;

                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                        Log.Debug(MainSettings.ApplicationName, responseText);
                    }
                }
            }

            catch (WebException exception)
            {
                string responseText;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                    Log.Debug("BookAStar", responseText);
                }
            }
        }

        public async Task<IEnumerable<Account>> GetAndroidAccounts()
        {
            return await Task.Run(() => {
                var manager = AccountStore.Create(this);
                return manager.FindAccountsForService(MainSettings.ApplicationName);
            });
        }

        public void AddAccount()
        {
            var auth = new YaOAuth2Authenticator(
                            clientId: "d9be5e97-c19d-42e4-b444-0e65863b19e1",
                            clientSecret: "blouh",
                            scope: "profile",
                            authorizeUrl: new Uri("http://dev.pschneider.fr/authorize"),
                            redirectUrl: new Uri("http://dev.pschneider.fr/oauth/success"),
                            accessTokenUrl: new Uri("http://dev.pschneider.fr/token"));
            auth.AllowCancel = false;
            Intent loginIntent = auth.GetUI(this);
            var accStore = AccountStore.Create(this);
            auth.Completed += (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    int expires_in = int.Parse(eventArgs.Account.Properties["expires_in"]);
                    var tokens = new Tokens
                    {
                        AccessToken = eventArgs.Account.Properties["access_token"],
                        RefreshToken = eventArgs.Account.Properties["refresh_token"],
                        ExpiresIn = expires_in,
                        TokenType = eventArgs.Account.Properties["token_type"]
                    };
                    RunOnUiThread(

                       async () =>
                       {
                           using (var client = new HttpClient())
                           {

                               // get me
                               // var request = new OAuth2Request("GET", new Uri(Constants.UserInfoUrl), null, eventArgs.Account);
                               var request = new HttpRequestMessage(HttpMethod.Get, MainSettings.UserInfoUrl);

                               request.Headers.Authorization =
                               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

                               var response = await client.SendAsync(request);

                               response.EnsureSuccessStatusCode();

                               string userJson = await response.Content.ReadAsStringAsync();
                               JObject jactiveUser = JObject.Parse(userJson);
                               Account acc = eventArgs.Account;

                               var uid = jactiveUser["Id"].Value<string>();
                               var username = jactiveUser["UserName"].Value<string>();
                               var roles = jactiveUser["Roles"].Values<string>().ToList();
                               var emails = jactiveUser["EMails"].Values<string>().ToList();


                               var newuser = new User
                               {
                                   UserName = username,
                                   EMails = emails,
                                   Roles = roles,
                                   Id = uid,
                                   YavscTokens = tokens
                               };
                               
                               MainSettings.SaveUser(newuser);
                               accStore.Save(acc, MainSettings.ApplicationName);
                           }
                       }

                        );

                }
            };
            auth.Error += Auth_Error;
            StartActivity(loginIntent);
        }

        public static void PopulateUserWithAccount(User u, Account a)
        {
            u.YavscTokens = new Model.Auth.Account.Tokens
            {
                AccessToken =
                        a.Properties["access_token"],
                RefreshToken =
                           a.Properties["refresh_token"],
                ExpiresIn =
                    int.Parse(a.Properties["expires_in"]),
                TokenType = a.Properties["token_type"]
            };
            u.UserName = a.Username;
        }

        private void Auth_Error(object sender, AuthenticatorErrorEventArgs e)
        {
            throw new NotImplementedException("Auth_Error");
        }
        

        public GoogleCloudMobileDeclaration GetDeviceInfo()
        {
            var devinfo = DeviceInfo.Plugin.CrossDeviceInfo.Current;
            return new GoogleCloudMobileDeclaration
            {
                DeviceId = devinfo.Id,
                Model = devinfo.Model,
                Platform = devinfo.Platform.ToString(),
                Version = devinfo.Version,
                GCMRegistrationId = MainSettings.GoogleRegId
            };
        }

        public void PostDeviceInfo()
        {
            throw new NotImplementedException();
        }
    }
}


