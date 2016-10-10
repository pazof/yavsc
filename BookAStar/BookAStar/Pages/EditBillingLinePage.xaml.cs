using BookAStar.ViewModels;
using System;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class EditBillingLinePage : ContentPage
    {
        public EditBillingLinePage(BillingLineViewModel model)
        {
            BindingContext = model;
            InitializeComponent();
        }

        public void OnValidateClicked (object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }
    }
}
