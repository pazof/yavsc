using SignaturePad.Forms;
using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace ZicMoove.Pages.EstimatePages
{
    using Data;
    using Settings;
    using ViewModels.EstimateAndBilling;
    using ViewModels.Signing;

    public partial class EstimateSigningPage : ContentPage
    {
        
        public EstimateSigningPage(EstimateSigningViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        private async void OnValidate(object sender, EventArgs ev)
        {
            btnValidate.IsEnabled = false;
            if (DataManager.Instance.Estimates.IsExecuting)
            {
                await App.DisplayAlert(Strings.OperationPending, Strings.oups);
                return;
            }
            IsBusy = true;
            var evm = (EditEstimateViewModel)BindingContext;
            var estimate = evm.Data;

            using (var stream = await padView.GetImageStreamAsync(SignatureImageFormat.Png))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await DataManager.Instance.Estimates.SignAsProvider(estimate, stream);
                DataManager.Instance.Estimates.SaveEntity();
            }
            IsBusy = false;

            var ParentValidationCommand = ((EstimateSigningViewModel)BindingContext).ValidationCommand;
            if (ParentValidationCommand != null)
                ParentValidationCommand.Execute(true);
            await Navigation.PopAsync();
        }

       
        
    }
}
