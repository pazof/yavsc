using BookAStar.Helpers;
using BookAStar.Interfaces;
using BookAStar.Model;
using BookAStar.Model.Workflow;
using BookAStar.Pages;
using BookAStar.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;
using XLabs.Forms.Pages;
using XLabs.Forms.Services;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Platform.Services;
using XLabs.Settings;

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
            app.Closing += (o, e) => Debug.WriteLine("Application Closing");
            app.Error += (o, e) => Debug.WriteLine("Application Error");
            app.Initialize += (o, e) => Debug.WriteLine("Application Initialized");
            app.Resumed += (o, e) => Debug.WriteLine("Application Resumed");
            app.Rotation += (o, e) => Debug.WriteLine("Application Rotated");
            app.Startup += (o, e) => Debug.WriteLine("Application Startup");
            app.Suspended += (o, e) => Debug.WriteLine("Application Suspended");
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
            PlatformSpecificInstance = instance;
            InitializeComponent();
            Init();
            BuildMainPage();

            NavigationPage.SetHasNavigationBar(MainPage, false);
            NavigationPage.SetHasBackButton(MainPage, false);
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
                Icon = "ic_corp_icon.png"
            };

            ToolbarItem tiHome = new ToolbarItem()
            {
                Text = "Accueil",
                Icon = "icon.png"
            };

            /* 
            var navPage = new NavigationPage(masterDetail) {
                Title = "Navigation",
                Icon = "icon.png"
            } ;
            //var navPage = new NavigationPage(mainTab);
            
            navPage.ToolbarItems.Add(tiHome);
            navPage.ToolbarItems.Add(tiSetts);
            */
            this.MainPage = masterDetail;

            Resolver.Resolve<IDependencyContainer>()
                .Register<INavigationService>(t => new NavigationService(masterDetail.Detail.Navigation))
                ;
        }


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
        }
    }
}

