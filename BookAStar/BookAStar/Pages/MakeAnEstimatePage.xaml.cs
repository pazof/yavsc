using BookAStar.Model.Workflow;
using BookAStar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class MakeAnEstimatePage : ContentPage
    {
        public Estimate Estimate { get { return BindingContext as Estimate;  } set {
                BindingContext = value;
            } }

        public static readonly BindableProperty MarkdownProperty =
            BindableProperty.Create("Description", typeof(string), typeof(Estimate),
                null, BindingMode.TwoWay);
        
        public MakeAnEstimatePage(string clientId,long bookQueryId)
        {
            InitializeComponent();
        }
    }
}
