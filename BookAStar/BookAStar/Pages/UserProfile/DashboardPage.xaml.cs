using System;
using Xamarin.Forms;
using XLabs.Forms.Behaviors;

namespace BookAStar.Pages.UserProfile
{
    using Data;
    using Helpers;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.Net.Http;
    using ViewModels.UserProfile;
    using XLabs.Forms.Controls;

    public partial class DashboardPage : ContentPage
    {

        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        public async void OnRefreshQuery(object sender, EventArgs e)
        {
            // TODO disable the button when current user is not registered
            if (MainSettings.CurrentUser==null)
                ShowPage<AccountChooserPage>(null, true);
            else
            {
                IsBusy = true;
                using (var client = UserHelpers.CreateJsonClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, Constants.UserInfoUrl))
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            string userJson = await response.Content.ReadAsStringAsync();
                            JObject jactiveUser = JObject.Parse(userJson);
                            var username = jactiveUser["UserName"].Value<string>();
                            var roles = jactiveUser["Roles"].Values<string>().ToList();
                            var emails = jactiveUser["EMails"].Values<string>().ToList();
                            var avatar = jactiveUser["Avatar"].Value<string>();
                            var address = jactiveUser["Avatar"].Value<string>();
                            var me = MainSettings.CurrentUser;
                            me.Address = address;
                            me.Avatar = avatar;
                            me.EMails = emails;
                            me.UserName = username;
                            me.Roles = roles;
                            MainSettings.SaveUser(me);
                        }
                    }
                }
                IsBusy = false;
            }
            
        }

        public void OnManageFiles(object sender, EventArgs e)
        {
            ShowPage<UserFiles>(null, true);
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
