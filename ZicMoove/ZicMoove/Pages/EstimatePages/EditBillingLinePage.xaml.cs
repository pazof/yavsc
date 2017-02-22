using ZicMoove.Data;
using ZicMoove.ViewModels;
using ZicMoove.ViewModels.EstimateAndBilling;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ZicMoove.Model.Workflow;

namespace ZicMoove.Pages
{
    public partial class EditBillingLinePage : ContentPage
    {
        public void Initialize()
        {
            InitializeComponent();
            foreach (string du in Enum.GetNames(typeof(BillingLineViewModel.DurationUnits)))
                picker.Items.Add(du);
            BindingContext = new BillingLineViewModel(new BillingLine());
        }

        public EditBillingLinePage()
        {
            Initialize();
        }

        public EditBillingLinePage(BillingLineViewModel model)
        {
            Initialize();
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
