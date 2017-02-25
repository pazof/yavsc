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
        public static readonly BindableProperty SignForProperty = BindableProperty.Create(
            "SignFor", typeof(string), typeof(MDSigningView), null, BindingMode.OneWay
            );

        public MDSigningView()
        {
            InitializeComponent();
        }
        public string SignFor
        {
            get {
                return GetValue(SignForProperty) as string;
            }
        }
    }
}
