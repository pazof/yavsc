using System;

using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Gms.Common;
using Android.Util;

using Xamarin.Auth;
using XLabs.Forms;
using XLabs.Platform.Device;
using XLabs.Forms.Services;
using XLabs.Platform.Services.Email;
using XLabs.Platform.Services.Media;
using XLabs.Serialization;

using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using BookAStar.Model.Auth.Account;
using BookAStar.Droid.OAuth;
using Yavsc.Helpers;
using Yavsc.Models.Identity;
using BookAStar.Helpers;
using BookAStar.Interfaces;
using Plugin.DeviceInfo;
using Android.Speech.Tts;
using XLabs.Platform.Services;
using XLabs.Caching.SQLite;
using SQLite.Net.Platform.XamarinAndroid;
using SQLite.Net;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Caching;
using XLabs;
using XLabs.Enums;

namespace BookAStar.Droid
{
    [Activity(Name="fr.pschneider.bas.MainActivity", Label = "BookAStar", Theme = "@style/MainTheme", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity :

        // global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity,
        XFormsCompatApplicationDroid,
        IPlatform, IComponentContext
    {
        protected override void OnCreate(Bundle bundle)
        {

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.SetTitleBarVisibility(Xamarin.Forms.AndroidTitleBarVisibility.Never);

            //var tb = FindViewById<Android.Support.V7.Widget.Toolbar>(ToolbarResource);
            // FIXME tb is null
            var stb = new Android.Support.V7.Widget.Toolbar(this);
            SetSupportActionBar(stb);
            var tb = new Toolbar(this);
            SetActionBar(tb);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
            }

            IXFormsApp<XFormsCompatApplicationDroid> app =null;
            if (!Resolver.IsSet)
            {
                this.SetIoc();
            }
            else
            {
                app = Resolver.Resolve<IXFormsApp>() as IXFormsApp<XFormsCompatApplicationDroid>;
                if (app != null)
                    app.AppContext = this;
            }

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);

            Xamarin.Forms.Forms.ViewInitialized += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.View.StyleId))
                {
                    e.NativeView.ContentDescription = e.View.StyleId;
                }
            };


            LoadApplication(new BookAStar.App(this));
            // TabLayoutResource = Resource.Layout.Tabbar;
            // ToolbarResource = Resource.Layout.Toolbar;
            /*
            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);
            LoadApplication(new App(this));
            /* var x = typeof(Themes.DarkThemeResources);
             x = typeof(Themes.LightThemeResources);
             x = typeof(Themes.Android.UnderlineEffect); */

        }

        private void SetIoc()
        {
            var resolverContainer = new SimpleContainer();

            var app = new XFormsCompatAppDroid();

            app.Init(this);

           var documents = app.AppDataDirectory;
           var pathToDatabase = Path.Combine(documents, "xforms.db");

            resolverContainer.Register<IDevice>(t => AndroidDevice.CurrentDevice)
                .Register<IDisplay>(t => t.Resolve<IDevice>().Display)
                .Register<IFontManager>(t => new FontManager(t.Resolve<IDisplay>()))
                .Register<IEmailService, EmailService>()
                .Register<IMediaPicker, MediaPicker>()
                .Register<ITextToSpeechService, XLabs.Platform.Services.TextToSpeechService>()
                .Register<IDependencyContainer>(resolverContainer)
                .Register<IXFormsApp>(app)
                .Register<ISecureStorage>(t => new KeyVaultStorage(t.Resolve<IDevice>().Id.ToCharArray()))
               
             
                .Register<ICacheProvider>(
                    t => new SQLiteSimpleCache(new SQLitePlatformAndroid(),
                        new SQLiteConnectionString(pathToDatabase, true), t.Resolve<IJsonSerializer>()));
            Resolver.SetResolver(resolverContainer.GetResolver());
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

        public App AppContext
        {
            get;set;
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
            var accounts = accStore.FindAccountsForService(Constants.ApplicationName);

            accStore.Delete(
                    accounts.Where(a => a.Username == userName).FirstOrDefault()
                    , Constants.ApplicationName);
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
            long queryId = Intent.GetLongExtra("BookQueryId",0);

            if (queryId > 0)
            {
                Task.Run(async () =>
                {
                    App.ShowBookQuery(
                       await DataManager.Current.BookQueries.Get(queryId));
                });
            }
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
                        Log.Debug(Constants.ApplicationName, responseText);
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
                return manager.FindAccountsForService(Constants.ApplicationName);
            });
        }
        YaOAuth2Authenticator auth = new YaOAuth2Authenticator(
                            clientId: "d9be5e97-c19d-42e4-b444-0e65863b19e1",
                            clientSecret: "blouh",
                            scope: "profile",
                            authorizeUrl: new Uri("http://dev.pschneider.fr/authorize"),
                            redirectUrl: new Uri("http://dev.pschneider.fr/oauth/success"),
                            accessTokenUrl: new Uri("http://dev.pschneider.fr/token"));
        public void AddAccount()
        {
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
                               var request = new HttpRequestMessage(HttpMethod.Get, Constants.UserInfoUrl);

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
                               accStore.Save(acc, Constants.ApplicationName);
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
            // TODO handle
        }

        public class GCMDeclaration : IGCMDeclaration
        {
            public string DeviceId
            { get; set; }

            public string GCMRegistrationId
            { get; set; }

            public string Model
            { get; set; }

            public string Platform
            { get; set; }

            public string Version
            { get; set; }
        }

        public IGCMDeclaration GetDeviceInfo()
        {
            var devinfo = CrossDeviceInfo.Current;
            return new GCMDeclaration
            {
                DeviceId = devinfo.Id,
                GCMRegistrationId = MainSettings.GoogleRegId,
                Model = devinfo.Model,
                Platform = devinfo.Platform.ToString(),
                Version = devinfo.Version
            };
        }

        [Obsolete("Use RemoteEntity to manage entities from API")]
        public TAnswer InvokeApi<TAnswer>(string method, object arg)
        {
            using (var m =
                new SimpleJsonPostMethod(
                    method, "Bearer "+
                MainSettings.CurrentUser.YavscTokens.AccessToken
                ))
            {
                return  m.Invoke<TAnswer>(arg);
            }
        }

        public object InvokeApi(string method, object arg)
        {
            using (var m =
                new SimpleJsonPostMethod(
                    method, "Bearer " +
                MainSettings.CurrentUser.YavscTokens.AccessToken
                )) 
            {
                return m.InvokeJson(arg);
            }
        }

        public T Resolve<T>()
        {

            throw new NotImplementedException();
        }

        public object Resolve(Type t)
        {
            throw new NotImplementedException();
        }

        public void Init(App context, bool initServices = true)
        {
            throw new NotImplementedException();
        }
    }
}


