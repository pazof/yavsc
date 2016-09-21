using BookAStar.Model;
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
    public partial class EditEstimatePage : ContentPage
    {
        public Estimate Estimate { get { return BindingContext as Estimate;  } set {
                BindingContext = value;
            } }
        public static readonly BindableProperty MarkdownProperty =
            BindableProperty.Create("Description", typeof(string), typeof(Estimate),
                null, BindingMode.TwoWay);
        public static readonly BindableProperty ClientProperty =
            BindableProperty.Create("Client", typeof(ClientProviderInfo), typeof(Estimate),
                null, BindingMode.OneWay);
        public static readonly BindableProperty QueryProperty =
            BindableProperty.Create("Query", typeof(BookQueryData), typeof(Estimate),
                null, BindingMode.OneWay);

        public EditEstimatePage()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            mdview.Markdown = Estimate?.Description;
        }

        protected void OnDescriptionChanged (object sender, EventArgs e)
        {
            Estimate.Description = mdview.Markdown;
        }
    }
}
