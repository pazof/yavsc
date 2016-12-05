using System;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    using Data;
    using EstimatePages;
    using Model.Workflow;
    using ViewModels.EstimateAndBilling;
    using ViewModels.Signing;

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
                bill.Add(new BillingLineViewModel(com));
                DataManager.Current.EstimationCache.SaveEntity();
            })};
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, lineView );
        }
        protected void OnEditLine(object sender, ItemTappedEventArgs e)
        {
            var line = (BillingLineViewModel)e.Item;
            line.ValidateCommand = new Command(() =>
            {
                DataManager.Current.EstimationCache.SaveEntity();
            });

            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, line );
        }

        protected async void OnEstimateValidated(object sender, EventArgs e)
        {
            var thisPage = this;
            var evm = (EditEstimateViewModel) BindingContext;
            var cmd = new Command<bool>( async (validated) =>
            {
                if (validated) { 
                    DataManager.Current.EstimationCache.Remove(evm);
                    DataManager.Current.EstimationCache.SaveEntity();
                }
                await thisPage.Navigation.PopAsync();
            });
            var response = await App.DisplayActionSheet(Strings.SignOrNot, Strings.DonotsignEstimate, Strings.CancelValidation, new string[] { Strings.Sign  });
            if (response == Strings.Sign)
            {
                App.NavigationService.NavigateTo<EstimateSigningPage>(true, 
                    new EstimateSigningViewModel(evm.Data) { ValidationCommand = cmd });
            }
            else if (response == Strings.CancelValidation)
                return;
            else cmd.Execute(true);
            
        }
    }
}
