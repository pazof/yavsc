using System;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    using Data;
    using Model.Workflow;
    using ViewModels;
    public partial class EditEstimatePage : ContentPage
    {

        public EditEstimatePage(EditEstimateViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ((EditEstimateViewModel)BindingContext).PropertyChanged += EditEstimatePage_PropertyChanged;
        }

        private void EditEstimatePage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DataManager.Current.EstimationCache.SaveCollection();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
             if (width > height)
             {
                 biAnVaLayout.Orientation = StackOrientation.Horizontal;
             }
             else
             {
                 biAnVaLayout.Orientation = StackOrientation.Vertical;
             }
            base.OnSizeAllocated(width, height);
        }

        protected void OnNewCommanLine(object sender, EventArgs e)
        {
            var com = new BillingLine() { Count = 1, UnitaryCost = 0.01m };
            var lineView = new BillingLineViewModel(com);
            var bill =
            ((EditEstimateViewModel)BindingContext).Bill;
            bill.Add(com);
            bill.SaveCollection();
            lineView.PropertyChanged += (s,f) => bill.SaveCollection();
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true,
                new object[] { lineView } );
        }


        protected void OnEstimateValidated(object sender, EventArgs e)
        {
            var evm = (EditEstimateViewModel)BindingContext;
            if (evm.Data.Id == 0)
            {
                DataManager.Current.Estimates.Create(evm.Data);
                // we have to manually add this item in our local collection,
                // since we could prefer to update the whole collection
                // from server, or whatever other scenario
                DataManager.Current.Estimates.Add(evm.Data);
            } else
            {
                DataManager.Current.Estimates.Update(evm.Data);
            }
            DataManager.Current.Estimates.SaveCollection();
            evm.State = LocalState.UpToDate;
        }
    }
}
