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
        public static readonly BindableProperty TotalProperty =
            BindableProperty.Create("Total", typeof(decimal), typeof(EditEstimatePage),
               (decimal) 0, BindingMode.OneWay);
        public static readonly BindableProperty DescriptionProperty =
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
            // FIXME Why the Binding don't work?
            mdview.Markdown = Estimate.Description;

        }

        protected void OnDescriptionChanged (object sender, EventArgs e)
        {
            // FIXME Why the Binding don't work?
            Estimate.Description = mdview.Markdown;
        }
        protected void OnNewCommanLine(object sender, EventArgs e)
        {
            var com = new CommandLine();
            App.CurrentApp.EditCommandLine(com);
        }
       
    }
}
