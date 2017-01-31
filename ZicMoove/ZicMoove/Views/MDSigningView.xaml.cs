using SignaturePad.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ZicMoove.Views
{
    public partial class MDSigningView : ContentView
    {
        public MDSigningView()
        {
            InitializeComponent();
        }

        private async void OnChangeTheme(object sender, EventArgs e)
        {
            var action = await App.DisplayActionSheet( 
                "Change Theme", "Cancel", null,
                new string[] { "White", "Black", "Aqua" } );
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

        private async void OnGetStats(object sender, EventArgs e)
        {
            var points = padView.Points.ToArray();
            var image = await padView.GetImageStreamAsync(SignatureImageFormat.Png);

            var pointCount = points.Count();
            var imageSize = image.Length / 1000;
            var linesCount = points.Count(p => p == Point.Zero) + (points.Length > 0 ? 1 : 0);

            image.Dispose();

            await App.DisplayAlert("Stats", $"The signature has {linesCount} lines or {pointCount} points, and is {imageSize:#,###.0}KB (in memory) when saved as a PNG.", "Cool");
        }
    }
}
