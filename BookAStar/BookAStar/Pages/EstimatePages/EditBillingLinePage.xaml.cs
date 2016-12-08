using BookAStar.Data;
using BookAStar.ViewModels;
using BookAStar.ViewModels.EstimateAndBilling;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class EditBillingLinePage : ContentPage
    {
        public EditBillingLinePage(BillingLineViewModel model)
        {
            InitializeComponent();
            foreach
                (string du in Enum.GetNames(typeof(BillingLineViewModel.DurationUnits)))
                picker.Items.Add(du);
            BindingContext = model;
        }

        public void OnDeleteClicked(object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }

        public void OnValidateClicked (object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            var bvm = (BillingLineViewModel)BindingContext;
            bvm.ValidateCommand?.Execute(null);
            return base.OnBackButtonPressed();
        }
    }
}
