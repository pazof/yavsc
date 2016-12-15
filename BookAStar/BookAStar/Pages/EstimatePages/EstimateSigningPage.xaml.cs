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

        private async void OnGetStats(object sender, EventArgs e)
        {
            var points = padView.Points.ToArray();
            using (var image = await padView.GetImageStreamAsync(SignatureImageFormat.Png))
            {
                var pointCount = points.Count();
                var imageSize = image.Length / 1000;
                var linesCount = points.Count(p => p == Point.Zero) + (points.Length > 0 ? 1 : 0);
                await DisplayAlert("Stats", $"The signature has {linesCount} lines or {pointCount} points, and is {imageSize:#,###.0}KB (in memory) when saved as a PNG.", "Cool");
            }
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
