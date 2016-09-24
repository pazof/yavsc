using BookAStar.Helpers;
using BookAStar.Interfaces;
using BookAStar.Model;
using BookAStar.Model.Workflow;
using BookAStar.Pages;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
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
        SearchPage searchPage = new SearchPage
        {
            Title = "Trouvez votre artiste",
            Icon = "glyphish_07_map_marker.png"
        };
        NavigationPage mp;

        SettingsPage settingsPage = new SettingsPage
            {
                Title = "Paramètres",
                Icon = "ic_corp_icon.png"
            };

        PinPage pinPage = new PinPage
        {
            Title = "Carte",
            Icon = "glyphish_07_map_marker.png"
        };
        

        

        

        public static IPlatform PlatformSpecificInstance { get; set; }
        public static string AppName { get; set; }
        public static App CurrentApp { get { return Current as App; } }

        public DataManager DataManager { get; set; } = new DataManager();

        internal void EditCommandLine(CommandLine com)
        {
            CommandLineEditorPage editCommandLine = new CommandLineEditorPage
            {
                Title = "Edition d'une ligne de facture",
                BindingContext = com
            };
            mp.Navigation.PushAsync(editCommandLine);
        }

        public App(IPlatform instance)
        {
            InitializeComponent();

            PlatformSpecificInstance = instance;
            mp = new NavigationPage(searchPage)
            {
                BackgroundColor = (Color)Application.Current.Resources["PageBackgroundColor"]
            };
            var color = Application.Current.Resources["PageBackgroundColor"];
            //var hasLabelStyle = r.ContainsKey("labelStyle");
            // var stid = this.StyleId;
            // null var appsstyle = settingsPage.Style;
            // appsstyle.CanCascade = true;
            MainPage = mp;
            ToolbarItem tiSetts = new ToolbarItem()
            {
                Text = "Settings",
                Icon = "ic_corp_icon.png"
            };
            mp.ToolbarItems.Add(tiSetts);
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
            mp.ToolbarItems.Add(tiQueries);
            ToolbarItem tiMap = new ToolbarItem
            {
                Text = "Carte",
                Icon = "glyphish_07_map_marker.png"
            };
            mp.ToolbarItems.Add(tiMap);
            tiMap.Clicked += (object sender, EventArgs e) =>
            {
                ShowPage(pinPage);
            };
           
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
                Title = "Demande de devis"
            };
            bookQueryPage.BindingContext = data;
            ShowPage(bookQueryPage);
        }

        public void EditEstimate(Estimate data)
        {
            EditEstimatePage editEstimate = new EditEstimatePage
            {
                Title = "Création d'un devis"
            };
            editEstimate.Estimate = data;
            ShowPage(editEstimate);
        }

        // TODO système de persistance de l'état de l'appli
        private void ShowPage(Page p) 
        {
            if (p.Parent!=null)
            {
                mp.Navigation.RemovePage(p);
                p.Parent = null;
            }
            mp.Navigation.PushAsync(p);
        }
    }
}

