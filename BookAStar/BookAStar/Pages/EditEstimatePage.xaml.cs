using BookAStar.Model;
using BookAStar.Model.Workflow;
using BookAStar.ViewModels;
using BookAStar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XLabs.Forms.Services;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{
    public partial class EditEstimatePage : ContentPage
    {

        public EditEstimatePage(EditEstimateViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            // FIXME WAZA
           if (BindingContext != null) {
                var e = ((EditEstimateViewModel)BindingContext);
                mdview.Markdown = e.Description;
              //  BillListView.ItemsSource = e.Bill;
            }
        }

        protected void OnDescriptionChanged (object sender, EventArgs e)
        {
            // FIXME Why the Binding don't work?
            ((EditEstimateViewModel)BindingContext).Description = mdview.Markdown;
            InvalidateMeasure();
        }

        protected void OnNewCommanLine(object sender, EventArgs e)
        {
            var com = new BillingLine() { Count = 1, UnitaryCost = 0.01m };
            var lineView = new BillingLineViewModel(com)
            {
                ValidateCommand = new Command(() => {
                    ((EditEstimateViewModel)BindingContext).Bill.
                    Add(com);
                })
            };

            App.NavigationService.NavigateTo<EditBillingLinePage>(
                true,
                new object[] { lineView } );
        }
        
    }
}
