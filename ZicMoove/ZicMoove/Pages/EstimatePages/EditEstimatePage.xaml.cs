using System;
using Xamarin.Forms;

namespace ZicMoove.Pages
{
    using Data;
    using EstimatePages;
    using Model.Workflow;
    using Settings;
    using ViewModels.EstimateAndBilling;
    using ViewModels.Signing;

    public partial class EditEstimatePage : ContentPage
    {
        public EditEstimateViewModel Model
        {
            get
            {
                return (EditEstimateViewModel)BindingContext;
            }
        }
        public EditEstimatePage(EditEstimateViewModel model)
        {
            BindingContext = model;

            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (Model == null) return;
            Model.PropertyChanged += EditEstimatePage_PropertyChanged;
            Model.Check();
        }

        private void EditEstimatePage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DataManager.Instance.EstimationCache.SaveEntity();
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
            var model = (EditEstimateViewModel)BindingContext;
            var com = new BillingLine() { Count = 1, UnitaryCost = 0.01m, EstimateId = model.Data.Id };
            var bill = model.Bill;
            var lineView = new BillingLineViewModel(com)
            { ValidateCommand = new Command(() => {
                bill.Add(new BillingLineViewModel(com));
                DataManager.Instance.EstimationCache.SaveEntity();
            })};
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, lineView );
        }
        protected void OnEditLine(object sender, ItemTappedEventArgs e)
        {

            var line = (BillingLineViewModel)e.Item;
            var evm = ((EditEstimateViewModel)BindingContext);
            // update the validation command, that 
            // was creating a new line in the bill at creation time,
            // now one only wants to update the line
            line.ValidateCommand = new Command(() =>
            {
                evm.Check();
                DataManager.Instance.EstimationCache.SaveEntity();
            });
            // and setup a removal command, that was not expected at creation time
            line.RemoveCommand = new Command(() =>
            {
                evm.Bill.Remove(line);
                evm.Check();
                DataManager.Instance.EstimationCache.SaveEntity();
            });
            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true, line );
            BillListView.SelectedItem = null;
        }

        protected async void OnEstimateValidated(object sender, EventArgs e)
        {
            
            var thisPage = this;
            var evm = (EditEstimateViewModel) BindingContext;
            var cmd = new Command<bool>( async (validated) =>
            {
                if (validated) { 
                    DataManager.Instance.EstimationCache.Remove(evm);
                    DataManager.Instance.EstimationCache.SaveEntity();
                }
                await thisPage.Navigation.PopAsync();
            });
            var response = await App.DisplayActionSheet(
                Strings.SignOrNot, Strings.DonotsignEstimate, 
                Strings.CancelValidation, new string[] { Strings.Sign  });
            if (response == Strings.Sign)
            {
                App.NavigationService.NavigateTo<EstimateSigningPage>(true,
                    new EstimateSigningViewModel(evm.Data) { ValidationCommand = cmd,
                        IsProviderView = true });
            }
            else if (response == Strings.CancelValidation)
                return;
            else cmd.Execute(true);
            
        }
    }
}
