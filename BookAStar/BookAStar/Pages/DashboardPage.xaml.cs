using BookAStar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using XLabs.Forms.Behaviors;
using XLabs.Forms.Controls;
using XLabs.Forms.Mvvm;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{
    public partial class DashboardPage : ContentPage
    {
        public RelayGesture UserNameGesture { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            UserNameGesture = new RelayGesture((g, x) =>
            {
                if (g.GestureType == GestureType.LongPress)
                {
                    Resolver.Resolve<INavigationService>().NavigateTo<AccountChooserPage>(true);
                }
            });
        }

        public void OnViewPerformerStatus(object sender, EventArgs e)
        {
            ShowPage<AccountChooserPage>(null, true);
        }

        public void OnViewUserQueries(object sender, EventArgs e)
        {
            ShowPage<BookQueriesPage>(null, true);
        }

        private void ShowPage<T>(object [] args, bool animate=false) where T:Page
        {
            Resolver.Resolve<INavigationService>().NavigateTo<T>(animate, args);
            App.MasterPresented = false;
        }

    }
}
