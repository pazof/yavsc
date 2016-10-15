using System;
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

namespace BookAStar
{
    using Data;
    using Interfaces;
    using Model;
    using Model.UI;
    using Pages;
    using ViewModels;

    public partial class App : Application // superclass new in 1.3
    {
        public static IPlatform PlatformSpecificInstance { get; set; }
        public static string AppName { get; set; }


        // Exists in order to dispose of a static instance strongly typed,
        // It Makes smaller code.
        public static App CurrentApp { get { return Current as App; } }
       
        public static bool MasterPresented
        {
            get
            { return CurrentApp.masterDetail.IsPresented; }
            internal set
            { CurrentApp.masterDetail.IsPresented = value; }
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
        }

        // omg
        private void OnError(object sender, EventArgs e)
        {
            
        }
        
        // Called on rotation after OnSuspended
        private void OnClosing(object sender, EventArgs e)
        {
            
        }

        // Called Once, at app init
        private void OnInitialize(object sender, EventArgs e)
        {
           
        }

        // called on app startup, not on rotation
        private void OnStartup(object sender, EventArgs e)
        {
            // TODO special startup pages as
            // notification details or wizard setup page
        }

        // Called on rotation
        private void OnSuspended(object sender, EventArgs e)
        {
            // TODO save the navigation stack
            int position = 0;
            foreach (Page page in MainPage.Navigation.NavigationStack)
            {

                DataManager.Current.AppState.Add(
                    new PageState
                    {
                        Position = position++,
                        PageType = page.GetType().FullName,
                        BindingContext = page.BindingContext
                    });
            }
            DataManager.Current.AppState.SaveCollection();
        }

        // called on app startup, after OnStartup, not on rotation
        private void OnAppResumed(object sender, EventArgs e)
        {
            // TODO restore the navigation stack 
            base.OnResume();
            foreach (var pageState in DataManager.Current.AppState)
            {
                var pageType = Type.GetType(pageState.PageType);
                NavigationService.NavigateTo(
                    pageType, true, pageState.BindingContext);
            }
            DataManager.Current.AppState.Clear();
            DataManager.Current.AppState.SaveCollection();
        }

        // FIXME Not called? 
        private void OnRotation(object sender, EventArgs<Orientation> e)
        {
            
        }

        public static GenericConfigSettingsMgr ConfigManager { protected set; get; }

        private void Configure(IXFormsApp app)
        {
            ViewFactory.EnableCache = true;
            ViewFactory.Register<ChatPage, ChatViewModel>(
                r=> new ChatViewModel { UserName = MainSettings.UserName }
                );
            ViewFactory.Register<DashboardPage, DashboardViewModel>(
                 resolver => new DashboardViewModel());
            ViewFactory.Register<BookQueryPage, BookQueryViewModel>();
            ViewFactory.Register<BookQueriesPage, BookQueriesViewModel>();
            ViewFactory.Register<EditBillingLinePage, BillingLineViewModel>();
            ViewFactory.Register<EditEstimatePage, EditEstimateViewModel>();
            ConfigManager = new XLabs.Settings.GenericConfigSettingsMgr(s =>
           MainSettings.AppSettings.GetValueOrDefault<string>(s, MainSettings.SettingsDefault), null);
            
        }

        ExtendedMasterDetailPage masterDetail;

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
        HomePage home;

        private void BuildMainPage()
        {
            accChooserPage = new AccountChooserPage();

            bQueriesPage = new BookQueriesPage
            {
                Title = "Demandes",
                Icon = "icon.png",
                BindingContext = new BookQueriesViewModel()
            };

            home = new HomePage() { Title = "Accueil", Icon = "icon.png" };

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

            // masterDetail.Detail = home;

            masterDetail.Detail = new NavigationPage(home);
            ToolbarItem tiSetts = new ToolbarItem()
            {
                Text = "Paramètres",
                Icon = "ic_corp_icon.png",
                Command = new Command(
                    () => { NavigationService.NavigateTo<AccountChooserPage>(); }
                    ) 
            };

            ToolbarItem tiHome = new ToolbarItem()
            {
                Text = "Accueil",
                Icon = "icon.png"
            };
            ToolbarItem tiPubChat= new ToolbarItem()
            {
                Text = "Chat",
                Icon = "chat_icon_s.png"
            };
            tiPubChat.Clicked += TiPubChat_Clicked;
            masterDetail.ToolbarItems.Add(tiHome);
            masterDetail.ToolbarItems.Add(tiSetts);
            masterDetail.ToolbarItems.Add(tiPubChat);
            this.MainPage = masterDetail;
            NavigationService = new NavigationService(masterDetail.Detail.Navigation);
        }

        private void TiPubChat_Clicked(object sender, EventArgs e)
        {
            NavigationService.NavigateTo<ChatPage>();
        }

        public static INavigationService NavigationService { protected set; get; }
        public void PostDeviceInfo()
        {
            var res = PlatformSpecificInstance.InvokeApi(
                "gcm/register",
                PlatformSpecificInstance.GetDeviceInfo());
        }

        public static void ShowBookQuery (BookQueryData query)
        {
            var page = ViewFactory.CreatePage<BookQueryViewModel
                 , BookQueryPage>((b, p) => p.BindingContext = new BookQueryViewModel(query));
            App.Current.MainPage.Navigation.PushAsync(page as Page);
        }
    }
}

