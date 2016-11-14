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
        

        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            // Assert ((DashboardViewModel)BindingContext!=null)
            ((DashboardViewModel)BindingContext).UserNameGesture
                = new RelayGesture( (gesture,arg) => {
                        ShowPage<AccountChooserPage>(null, true);
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
            App.NavigationService.NavigateTo<T>(animate, args);
            App.MasterPresented = false;
        }

    }
}
