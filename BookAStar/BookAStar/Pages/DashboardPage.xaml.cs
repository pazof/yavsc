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
            App.CurrentApp.ShowAccounts();
        }

        public void OnViewUserQueries(object sender, EventArgs e)
        {
            App.CurrentApp.ShowQueries();

        }


    }
}
