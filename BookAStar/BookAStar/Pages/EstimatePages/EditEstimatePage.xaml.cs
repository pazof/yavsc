using System;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    using Data;
    using Model.Workflow;
    using ViewModels.EstimateAndBilling;

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
            DataManager.Current.EstimationCache.SaveEntity();
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
            var bill = ((EditEstimateViewModel)BindingContext).Bill;
            var lineView = new BillingLineViewModel(com)
            { ValidateCommand = new Command(() => {
                bill.Add(com);
                DataManager.Current.EstimationCache.SaveEntity();
            })};
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, lineView );
        }
        protected void OnEditLine(object sender, ItemTappedEventArgs e)
        {
            var line = (BillingLine)e.Item;
            var bill = ((EditEstimateViewModel)BindingContext).Bill;
            var lineView = new BillingLineViewModel(line)
            {
                ValidateCommand = new Command(() => {
                    DataManager.Current.EstimationCache.SaveEntity();
                })
            };
            lineView.PropertyChanged += LineView_PropertyChanged;
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, lineView );
        }

        private void LineView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DataManager.Current.EstimationCache.SaveEntity();
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
            DataManager.Current.Estimates.SaveEntity();
            DataManager.Current.EstimationCache.Remove(evm);
            DataManager.Current.EstimationCache.SaveEntity();
            Navigation.PopAsync();
        }
    }
}
