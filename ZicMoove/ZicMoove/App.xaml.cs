using System;
using Microsoft.AspNet.SignalR.Client;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;
using XLabs.Forms.Pages;
using XLabs.Forms.Services;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Platform.Services;
using XLabs.Settings;
using XLabs;
using XLabs.Enums;

namespace ZicMoove
{
    using Data;
    using Interfaces;
    using Model;
    using Pages;
    using Plugin.Connectivity;
    using Model.Social.Messaging;
    using ViewModels.Messaging;
    using ViewModels.UserProfile;
    using Pages.UserProfile;
    using ViewModels.EstimateAndBilling;
    using Pages.EstimatePages;
    using ViewModels;
    using Pages.Chat;
    using System.Collections.Generic;
    using Model.Social;
    using Settings;

    public partial class App : Application // superclass new in 1.3
    {
        public static IPlatform PlatformSpecificInstance { get; set; }
        public static string AppName { get; set; }

        [Obsolete("Instead using this, use new static properties.")]
        public static App CurrentApp { get { return Current as App; } }

        private static ExtendedMasterDetailPage masterDetail;
        public static bool MasterPresented
        {
            get
            { return App.masterDetail.IsPresented; }
            internal set
            { masterDetail.IsPresented = value; }
        }

        public void Init()
        {
            var app = Resolver.Resolve<IXFormsApp>();

            if (app == null)
            {
                return;
            }
            Configure(app);
            app.Closing += OnClosing;
            app.Error += OnError;
            app.Initialize += OnInitialize;
            app.Resumed += OnAppResumed;
            app.Rotation += OnRotation;
            app.Startup += OnStartup;
            app.Suspended += OnSuspended;
            MainSettings.UserChanged += MainSettings_UserChanged;
            CrossConnectivity.Current.ConnectivityChanged += (conSender, args) =>
            { App.IsConnected = args.IsConnected; };
            SetupHubConnection();
            MainSettings_UserChanged(this, null);
            
                StartConnexion();
        }

        // omg
        private void OnError(object sender, EventArgs e)
        {

        }

        // Called on rotation after OnSuspended
        private void OnClosing(object sender, EventArgs e)
        {
        }

        // FIXME Never called.
        private void OnInitialize(object sender, EventArgs e)
        {

        }

        // called on app startup, not on rotation
        private void OnStartup(object sender, EventArgs e)
        {
            // TODO special startup pages as
            // notification details or wizard setup page
        }
        private static INavigation Navigation
        {
            get
            {
                return masterDetail.Detail.Navigation;
            }
        }
        // Called on rotation
        private void OnSuspended(object sender, EventArgs e)
        {
            StopConnection();
            int position = 0;
            DataManager.Instance.AppState.Clear();
            foreach (Page page in Navigation.NavigationStack)
            {
                DataManager.Instance.AppState.Add(
                    new PageState
                    {
                        Position = position++,
                        PageType = page.GetType().FullName,
                        BindingContext = page.BindingContext
                    });
            }
            DataManager.Instance.AppState.SaveEntity();
        }

        // called on app startup, after OnStartup, not on rotation
        private void OnAppResumed(object sender, EventArgs e)
        {
            StartConnexion();
            // TODO restore the navigation stack 
            base.OnResume();
            foreach (var pageState in DataManager.Instance.AppState)
            {
                if (pageState.PageType != null)
                {
                    var pageType = Type.GetType(pageState.PageType);
                    if (pageState.BindingContext != null)
                        NavigationService.NavigateTo(
                           pageType, false, pageState.BindingContext);
                    else NavigationService.NavigateTo(
                        pageType, false);
                }
            }
            DataManager.Instance.AppState.Clear();
            DataManager.Instance.AppState.SaveEntity();
        }

        // FIXME Not called? 
        private void OnRotation(object sender, EventArgs<Orientation> e)
        {

        }

        public static GenericConfigSettingsMgr ConfigManager { protected set; get; }

        private void Configure(IXFormsApp app)
        {
            ViewFactory.EnableCache = true;
            ViewFactory.Register<DashboardPage, DashboardViewModel>(
                 resolver => new DashboardViewModel());
            ViewFactory.Register<BookQueryPage, BookQueryViewModel>();
            ViewFactory.Register<BookQueriesPage, BookQueriesViewModel>();
            ViewFactory.Register<EditBillingLinePage, BillingLineViewModel>();
            ViewFactory.Register<EditEstimatePage, EditEstimateViewModel>();
            ViewFactory.Register<UserFiles, DirectoryInfoViewModel>();
            ViewFactory.Register<UserProfilePage, UserProfileViewModel>();
            ViewFactory.Register<EstimateSigningPage, EditEstimateViewModel>();
            ConfigManager = new GenericConfigSettingsMgr(s =>
           MainSettings.AppSettings.GetValueOrDefault<string>(s, MainSettings.SettingsDefault), null);
        }

        public App(IPlatform instance)
        {
            // This declaration became obsolete by introduction
            // of the XLabs App that 
            // refers this instance with
            // its application context property
            // and is obtained using the `Resolver`
            PlatformSpecificInstance = instance;
            // Xaml
            InitializeComponent();
            // Static properties construction
            Init();
            // Builds the Main page
            BuildMainPage();

        }

        BookQueriesPage bQueriesPage;
        AccountChooserPage accChooserPage;
        HomePage homePage;

        private static UserProfilePage userProfilePage;

        public static UserProfilePage UserProfilePage
           { get { return userProfilePage; } }

        ChatPage chatPage;
        PinPage pinPage;

        public static void ShowPage(Page page)
        {
            if (Navigation.NavigationStack.Contains(page))
            {
                if (Navigation.NavigationStack.Last() == page) return;
                Navigation.RemovePage(page);
                page.Parent = null;
            }
            Navigation.PushAsync(page);
        }

        private void BuildMainPage()
        {
            // TODO
            // in case of App resume, 
            // do not create new BindingContext's,
            // but use those from the AppState property
            accChooserPage = new AccountChooserPage();

            var bookQueries = new BookQueriesViewModel();

            var userprofile = new UserProfileViewModel();

            bQueriesPage = new BookQueriesPage
            {
                Title = "Demandes",
                Icon = "icon.png",
                BindingContext = bookQueries
            };

            homePage = new HomePage() {
                Title = "Accueil",
                Icon = "icon.png" };

            homePage.BindingContext = new HomeViewModel {
                BookQueries = bookQueries,
                UserProfile = userprofile };

            userProfilePage = new UserProfilePage {
                Title = "Profile utilisateur",
                Icon = "ic_corp_icon.png",
                BindingContext = userprofile
            };

            chatPage = new ChatPage
            {
                Title = "Chat",
                Icon = "",
                BindingContext = new ChatViewModel()
            };

            pinPage = new PinPage();

            // var mainPage = new NavigationPage(bQueriesPage);

            masterDetail = new ExtendedMasterDetailPage()
            {
                Title = "MainPage"
            };

            masterDetail.Master = new DashboardPage
            {
                Title = "Bookingstar",
                BindingContext = new DashboardViewModel()
            };


            masterDetail.Detail = new NavigationPage(homePage);
            ToolbarItem tiSetts = new ToolbarItem()
            {
                // FIXME what for? Priority = 0, 
                Text = "Paramètres",
                Icon = "ic_corp_icon.png",
                Command = new Command(
                    () =>
                    {
                        ShowPage(userProfilePage);
                    } ) 
            };

            ToolbarItem tiHome = new ToolbarItem()
            {
                Text = "Accueil",
                Icon = "icon.png",
                Command = new Command(
                    () => {
                        ShowPage(homePage);
                    })
            };

            ToolbarItem tiPubChat= new ToolbarItem()
            {
                Text = "Chat",
                Icon = "chat_icon_s.png",
                Command = new Command(
                    () => { ShowPage(chatPage); }
                    )
            };
            ToolbarItem tiPinPage = new ToolbarItem()
            {
                Text = "Map",
                Icon = "glyphish_103_map.png",
                Command = new Command(
                    () => { ShowPage(pinPage); }
                    )
            };
            masterDetail.ToolbarItems.Add(tiHome);
            masterDetail.ToolbarItems.Add(tiSetts);
            masterDetail.ToolbarItems.Add(tiPubChat);
            this.MainPage = masterDetail;
            
            NavigationService = new NavigationService(Navigation);
        }
        public static Task<string> DisplayActionSheet(string title, string cancel, string destruction, string [] buttons)
        {
            var currentPage = Navigation.NavigationStack.Last();
            return currentPage.DisplayActionSheet(title, cancel, destruction, buttons);
        }

        public static Task<bool> DisplayAlert(string title, string message, string yes = "OK", string no = null)
        {
            var currentPage = ((NavigationPage)Current.MainPage).CurrentPage;
            if (no == null)
            {
                return currentPage.DisplayAlert(title, message, yes).ContinueWith(task => true);
            }
            else
            {
                return currentPage.DisplayAlert(title, message, yes, no);
            }

        }


        public static INavigationService NavigationService { protected set; get; }
        public static bool isConnected;
        public static bool IsConnected { get { return isConnected; }
            private set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    if (isConnected)
                    {
                        // TODO Start all cloud related stuff 
                        StartConnexion();
                    }
                    
                }
            }
        }
        private static HubConnection chatHubConnection = null;
        public static HubConnection ChatHubConnection
        {
            get
            {
                return chatHubConnection;
            }
        }
        // Start the Hub connection
        public static async void StartConnexion ()
        {
            if (CrossConnectivity.Current.IsConnected)
                try
            {
                    if (chatHubConnection.State == ConnectionState.Disconnected)
                await chatHubConnection.Start();
            }
            catch (WebException  )
            {
                // TODO use webex, set this cx down status somewhere,
                // & display it & maybe try again later.
            }
            catch (Exception )
            {
                // TODO use ex
            }
        }

        public void SetupHubConnection()
        {
            if (chatHubConnection != null)
                chatHubConnection.Dispose();
            chatHubConnection = new HubConnection(Constants.SignalRHubsUrl);
            chatHubConnection.Error += ChatHubConnection_Error;

            chatHubProxy = chatHubConnection.CreateHubProxy("ChatHub");
            chatHubProxy.On<string, string>("addPV", (n, m) => {
                var msg = new ChatMessage
                {
                    Message = m,
                    SenderId = n,
                    Date = DateTime.Now
                };
                DataManager.Instance.PrivateMessages.Add(
                    msg

                    );
                DataManager.Instance.PrivateMessages.SaveEntity();
                DataManager.Instance.ChatUsers.OnPrivateMessage(msg);
            });
        }
        public static  void StopConnection()
        {
            try
            {
                if (chatHubConnection.State != ConnectionState.Disconnected)
                chatHubConnection.Stop();
            }
            catch (WebException)
            {
                // TODO use webex, set this cx down status somewhere,
                // & display it & maybe try again later.
            }
            catch (Exception)
            {
                // TODO use ex
            }
        }
        private void MainSettings_UserChanged(object sender, EventArgs e)
        {
            StopConnection();
            if (MainSettings.CurrentUser != null)
            {
                var token = MainSettings.CurrentUser.YavscTokens.AccessToken;
                if (chatHubConnection.Headers.ContainsKey("Authorization"))
                    chatHubConnection.Headers.Remove("Authorization");
                chatHubConnection.Headers.Add("Authorization", $"Bearer {token}");
            }
            StartConnexion();
        }

        private void ChatHubConnection_Error(Exception obj)
        {
            // TODO log in debug binaries
        }

        private static IHubProxy chatHubProxy = null;
        public static IHubProxy ChatHubProxy
        {
            get
            {
                return chatHubProxy;
            }
        }
        public static Task<bool> HasSomeCloud {
            get
            {
                return CrossConnectivity.Current.IsReachable(Constants.YavscHomeUrl, Constants.CloudTimeout);
            }
        }

        public static void PostDeviceInfo()
        {
            var info = PlatformSpecificInstance.GetDeviceInfo();
            if (!string.IsNullOrWhiteSpace(info.GCMRegistrationId))
                PlatformSpecificInstance.InvokeApi("gcm/register", info);
        }

        public static void ShowBookQuery (BookQuery query)
        {
            var page = new BookQueryPage
            { BindingContext = new BookQueryViewModel(query) };
            ShowPage(page);
        }
    }
}

