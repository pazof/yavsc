﻿using System;
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
            Model.CheckCommand = new Action<Estimate, ViewModels.Validation.ModelState>(
                (e, m) =>
                {
                    foreach (var line in model.Bill)
                    {
                        line.Check();
                        if (!line.ViewModelState.IsValid)
                            model.ViewModelState.AddError("Bill", "invalid line");
                    }
                });

            InitializeComponent();
            Model.Check();
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
            var evm = ((EditEstimateViewModel)BindingContext);
            // update the validation command, that 
            // was creating a new line in the bill at creation time,
            // now one only wants to update the line
            line.ValidateCommand = new Command(() =>
            {
                evm.Check();
                DataManager.Current.EstimationCache.SaveEntity();
            });
            // and setup a removal command, that was not expected at creation time
            line.RemoveCommand = new Command(() =>
            {
                evm.Bill.Remove(line);
                evm.Check();
                DataManager.Current.EstimationCache.SaveEntity();
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
                    DataManager.Current.EstimationCache.Remove(evm);
                    DataManager.Current.EstimationCache.SaveEntity();
                }
                await thisPage.Navigation.PopAsync();
            });
            var response = await App.DisplayActionSheet(
                Strings.SignOrNot, Strings.DonotsignEstimate, 
                Strings.CancelValidation, new string[] { Strings.Sign  });
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
