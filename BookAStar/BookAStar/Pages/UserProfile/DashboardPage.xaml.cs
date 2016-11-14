using System;
using Xamarin.Forms;
using XLabs.Forms.Behaviors;

namespace BookAStar.Pages.UserProfile
{
    using Data;
    using ViewModels.UserProfile;
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
        public void OnManageFiles(object sender, EventArgs e)
        {
            ShowPage<UserFiles>(new object[] { new DirectoryInfoViewModel() }, true);
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
