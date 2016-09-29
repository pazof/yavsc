using BookAStar.Helpers;
using BookAStar.Interfaces;
using BookAStar.Model;
using BookAStar.Model.Workflow;
using BookAStar.Pages;
using BookAStar.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Forms.Controls;
using XLabs.Forms.Mvvm;
using XLabs.Forms.Pages;
using XLabs.Forms.Services;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Platform.Services;
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

        public void ShowQueries()
        {
            masterDetail.Detail = bQueriesPage;
            masterDetail.SendBackButtonPressed();
        }
        public void ShowAccounts()
        {
            masterDetail.Detail = accChooserPage;
            masterDetail.SendBackButtonPressed();
        }
        // Exists in order to dispose of a static instance strongly typed
        // TODO : replace all references to this field
        // by Views resolution, and then, drop it
        public static App CurrentApp { get { return Current as App; } }

        public static void Init()
        {
            var app = Resolver.Resolve<IXFormsApp>();
            
            if (app == null)
            {
                return;
            }

            app.Closing += (o, e) => Debug.WriteLine("Application Closing");
            app.Error += (o, e) => Debug.WriteLine("Application Error");
            app.Initialize += (o, e) => Debug.WriteLine("Application Initialized");
            app.Resumed += (o, e) => Debug.WriteLine("Application Resumed");
            app.Rotation += (o, e) => Debug.WriteLine("Application Rotated");
            app.Startup += (o, e) => Debug.WriteLine("Application Startup");
            app.Suspended += (o, e) => Debug.WriteLine("Application Suspended");

        }
        public void Configure(IXFormsApp app)
        {
            ViewFactory.EnableCache = true;
            ViewFactory.Register<DashboardPage, DashboardViewModel>(
                 resolver => new DashboardViewModel());
            ViewFactory.Register<BookQueryPage, BookQueryViewModel>();
            ViewFactory.Register<EditBillingLinePage, BillingLineViewModel>();
            ViewFactory.Register<EditEstimatePage, EstimateViewModel>();
        }


        ExtendedMasterDetailPage masterDetail;

        public App(IPlatform instance)
        {
            PlatformSpecificInstance = instance;
            InitializeComponent();
            Init();
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
                BindingContext = DataManager.Current.BookQueries,
                Title = "Demandes"
            };

            home = new HomePage() { Title = "Accueil" };

            // var mainPage = new NavigationPage(bQueriesPage);
            /*
            masterDetail = new ExtendedMasterDetailPage() {
                Title="MainPAge"
            };

            masterDetail.Master = new DashboardPage {
                Title = "Bookingstar",
                BindingContext = new DashboardViewModel() };
            masterDetail.Detail = new HomePage { Title = "Accueil" };

            */
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

          




            /*     searchPage = new SearchPage
                 {
                     Title = "Trouvez votre artiste",
                     Icon = "glyphish_07_map_marker.png"
                 };

                  settingsPage = new SettingsPage
                 {
                     Title = "Paramètres",
                     Icon = "ic_corp_icon.png"
                 };

                  pinPage = new PinPage
                 {
                     Title = "Carte",
                     Icon = "glyphish_07_map_marker.png"
                 };
                 PlatformSpecificInstance = instance;
                 Navigation = new NavigationPage(searchPage);

                 //var hasLabelStyle = r.ContainsKey("labelStyle");
                 // var stid = this.StyleId;
                 // null var appsstyle = settingsPage.Style;
                 // appsstyle.CanCascade = true;
                 MainPage = Navigation;
                 ToolbarItem tiSetts = new ToolbarItem()
                 {
                     Text = "Settings",
                     Icon = "ic_corp_icon.png"
                 };
                 Navigation.ToolbarItems.Add(tiSetts);
                 tiSetts.Clicked += (object sender, EventArgs e) =>
                 {
                     ShowPage (settingsPage);
                 };
                 ToolbarItem tiQueries = new ToolbarItem
                 {
                     Text = "Demandes"
                 };

                 tiQueries.Clicked += (object sender, EventArgs e) =>
                 {
                     BookQueriesPage bookQueriesPage = new BookQueriesPage
                     {
                         Title = "Demandes de devis"
                     };
                     bookQueriesPage.BindingContext = DataManager.BookQueries;
                     ShowPage(bookQueriesPage);
                 };
                 Navigation.ToolbarItems.Add(tiQueries);
                 ToolbarItem tiMap = new ToolbarItem
                 {
                     Text = "Carte",
                     Icon = "glyphish_07_map_marker.png"
                 };
                 Navigation.ToolbarItems.Add(tiMap);
                 tiMap.Clicked += (object sender, EventArgs e) =>
                 {
                     ShowPage(pinPage);
                 };
                 MainPage = Navigation;
                 */
            var mainTab = new ExtendedTabbedPage()
            {
                Title = "XLabs",
                SwipeEnabled = true,
                TintColor = Color.White,
                BarTintColor = Color.Blue,
                Badges = { "1", "2", "3" },
                TabBarBackgroundImage = "visuel_sexion.png",
                TabBarSelectedImage = "icon.png",
            };

            var navPage = new NavigationPage(bQueriesPage);
            //var navPage = new NavigationPage(mainTab);
            
            navPage.ToolbarItems.Add(tiHome);
            navPage.ToolbarItems.Add(tiSetts);

            this.MainPage = navPage;

            Resolver.Resolve<IDependencyContainer>()
                .Register<INavigationService>(t => new NavigationService(navPage.Navigation))
                ;
        }

        internal void EditCommandLine(Page parentPage, BillingLine com)
        {
            EditBillingLinePage editCommandLine = new EditBillingLinePage
            {
                Title = "Edition d'une ligne de facture",
                BindingContext = com
            };
            parentPage.Navigation.PushAsync(editCommandLine);
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
                , BookQueryPage>(null, new BookQueryViewModel(query));
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

