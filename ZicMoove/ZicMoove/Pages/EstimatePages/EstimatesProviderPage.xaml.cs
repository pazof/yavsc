using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace ZicMoove.Pages
{
    using Model.Workflow;
    using ViewModels.EstimateAndBilling;

    public partial class EstimatesProviderPage : ContentPage
    {
        public EstimatesProviderPage()
        {
            InitializeComponent();
        }

        private void OnViewDetail(object sender, ItemTappedEventArgs e)
        {
            Estimate data = e.Item as Estimate;
            App.NavigationService.NavigateTo<EditEstimatePage>(
                true, 
                new EditEstimateViewModel(data));
        }
    }
}
