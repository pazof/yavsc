using BookAStar.Interfaces;
using BookAStar.Model;
using BookAStar.Pages;
using BookAStar.ViewModels;
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
    public partial class App : Application // superclass new in 1.3
    {
        public static IPlatform PlatformSpecificInstance { get; set; }
        public static string AppName { get; set; }


        // Exists in order to dispose of a static instance strongly typed
        // TODO : replace all references to this field
        // by Views resolution, and then, drop it
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

        // FIXME Not called
        private void OnInitialize(object sender, EventArgs e)
        {
           
        }

        // called on app startup, not on rotation
        private void OnStartup(object sender, EventArgs e)
        {
            // TODO special starup pages as
            // notification details or wizard setup page
        }

        // Called on rotation
        private void OnSuspended(object sender, EventArgs e)
        {
            // TODO the navigation stack persistence (save)
            
        }

        // called on app startup, after OnStartup, not on rotation
        private void OnAppResumed(object sender, EventArgs e)
        {
            // TODO the navigation stack persistence (restore)
            base.OnResume();
        }

        // FIXME Not called ... see OnSuspended
        private void OnRotation(object sender, EventArgs<Orientation> e)
        {
            // TODO the navigation stack persistence (restore?)
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
            ViewFactory.Register<EditEstimatePage, EstimateViewModel>();
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
            masterDetail.ToolbarItems.Add(tiHome);
            masterDetail.ToolbarItems.Add(tiSetts);
            this.MainPage = masterDetail;
            NavigationService = new NavigationService(masterDetail.Detail.Navigation);
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

        // TODO système de persistance de l'état de l'appli
        /*
        /// <summary>
        /// Shows a page asynchronously by locating the default constructor, creating the page,
        /// the pushing it onto the navigation stack.
        /// </summary>
        /// <param name="parentPage">Parent Page</param>
        /// <param name="pageType">Type of page to show</param>
        /// <returns></returns>
        public static async Task ShowPage(VisualElement parentPage, Type pageType)
        {
            // Get all the constructors of the page type.
            var constructors = pageType.GetTypeInfo().DeclaredConstructors;

            foreach (var page in
                    from constructor in constructors
                    where constructor.GetParameters().Length == 0
                    select (Page)constructor.Invoke(null))
            {
                await parentPage.Navigation.PushAsync(page);
                break;
            }
        }*/
    }
}

