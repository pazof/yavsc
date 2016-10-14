using System;
using Xamarin.Forms;

namespace BookAStar.Pages
{
    using Model.Workflow;
    using ViewModels;
    public partial class EditEstimatePage : ContentPage
    {

        public EditEstimatePage(EditEstimateViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
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

        protected void OnEstimateValidated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
