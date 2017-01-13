using SignaturePad.Forms;
using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace BookAStar.Pages.EstimatePages
{
    using Data;
    using ViewModels.EstimateAndBilling;
    using ViewModels.Signing;

    public partial class EstimateSigningPage : ContentPage
    {
        
        public EstimateSigningPage(EstimateSigningViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
       
       private async void OnValidate (object sender, EventArgs ev)
       {
            btnValidate.IsEnabled = false;
            if (DataManager.Instance.Estimates.IsExecuting)
            {
                await App.DisplayAlert(Strings.OperationPending, Strings.oups);
                return;
            }

            var evm = (EditEstimateViewModel)BindingContext;
            var estimate = evm.Data;
            var pngStream = await padView.GetImageStreamAsync(SignatureImageFormat.Png);
            pngStream.Seek(0, SeekOrigin.Begin);
            DataManager.Instance.Estimates.SignAsProvider(estimate, pngStream);
            DataManager.Instance.Estimates.SaveEntity();
            await Navigation.PopAsync();

            var ParentValidationCommand = ((EstimateSigningViewModel)BindingContext).ValidationCommand;
            if (ParentValidationCommand != null)
                ParentValidationCommand.Execute(true);
        }

       
        private async void OnChangeTheme(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Change Theme", "Cancel", null, "White", "Black", "Aqua");
            switch (action)
            {
                case "White":
                    padView.BackgroundColor = Color.White;
                    padView.StrokeColor = Color.Black;
                    padView.ClearTextColor = Color.Black;
                    padView.ClearText = "Clear Markers";
                    break;

                case "Black":
                    padView.BackgroundColor = Color.Black;
                    padView.StrokeColor = Color.White;
                    padView.ClearTextColor = Color.White;
                    padView.ClearText = "Clear Chalk";
                    break;

                case "Aqua":
                    padView.BackgroundColor = Color.Aqua;
                    padView.StrokeColor = Color.Red;
                    padView.ClearTextColor = Color.Black;
                    padView.ClearText = "Clear The Aqua";
                    break;
            }
            
        }
    }
}
