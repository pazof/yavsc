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
using XLabs.Forms.Mvvm;
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
        public static App CurrentApp { get { return Current as App; } }
        public DataManager DataManager { get; set; } = new DataManager();
        public static void Init()
        {
            var app = Resolver.Resolve<IXFormsApp>();
            if (app == null)
            {
                return;
            }
            // navigation registration

            app.Closing += (o, e) => Debug.WriteLine("Application Closing");
            app.Error += (o, e) => Debug.WriteLine("Application Error");
            app.Initialize += (o, e) => Debug.WriteLine("Application Initialized");
            app.Resumed += (o, e) => Debug.WriteLine("Application Resumed");
            app.Rotation += (o, e) => Debug.WriteLine("Application Rotated");
            app.Startup += (o, e) => Debug.WriteLine("Application Startup");
            app.Suspended += (o, e) => Debug.WriteLine("Application Suspended");
            
        }

        public App(IPlatform instance)
        {
            PlatformSpecificInstance = instance;
            InitializeComponent();
            Init();
            MainPage = GetMainPage();
        }

        public static Page GetMainPage()
        {
            ViewFactory.Register<SettingsPage,SettingsViewModel>();
            ViewFactory.Register<BookQueryPage, BookQueryViewModel>(
                resolver => new BookQueryViewModel ()
                );
           
            var bQueriesPage = new BookQueriesPage
            {
                BindingContext = DataManager.Current.BookQueries,
                Title = "Demandes"
            };

            //  var mainPage = new NavigationPage(bQueriesPage);

            var mainPage = new XLabs.Forms.Pages.ExtendedMasterDetailPage() {
                Title="MainPAge"
            };

            mainPage.Master = bQueriesPage;
            mainPage.Detail = new SettingsPage();
            
            Resolver.Resolve<IDependencyContainer>()
                .Register<INavigationService>(t => new NavigationService(mainPage.Navigation));

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
            mainPage.ToolbarItems.Add(tiHome);
            mainPage.ToolbarItems.Add(tiSetts);
            return mainPage;

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


        }

        internal void EditCommandLine(Page parentPage, CommandLine com)
        {
            CommandLineEditorPage editCommandLine = new CommandLineEditorPage
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

        public void ShowBookQuery(BookQueryData data)
        {
            BookQueryPage bookQueryPage = new BookQueryPage
            {
                Title = "Demande de devis",
                BindingContext = data
            };
           ShowPage(bookQueryPage);
        }

        public void EditEstimate(Estimate data)
        {
            throw new NotImplementedException();
            EditEstimatePage editEstimate = new EditEstimatePage
            {
                Title = "Création d'un devis"
            };
            editEstimate.Estimate = data;
            // ShowPage(editEstimate);
        }

        // TODO système de persistance de l'état de l'appli

        /// <summary>
        /// Shows a page asynchronously by locating the default constructor, creating the page,
        /// the pushing it onto the navigation stack.
        /// </summary>
        /// <param name="parentPage">Parent Page</param>
        /// <param name="pageType">Type of page to show</param>
        /// <returns></returns>
        private static async Task ShowPage(VisualElement parentPage, Type pageType)
        {
            // Get all the constructors of the page type.
            var constructors = pageType.GetTypeInfo().DeclaredConstructors;

            foreach ( var page in
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

