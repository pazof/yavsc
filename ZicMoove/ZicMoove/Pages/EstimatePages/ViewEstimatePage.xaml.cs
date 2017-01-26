
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BookAStar.Pages.EstimatePages
{
    using ViewModels.EstimateAndBilling;

    public partial class ViewEstimatePage : ContentPage
    {
        public ViewEstimatePage(EditEstimateViewModel model)
        {
            InitializeComponent();
            billListView.ItemSelected += (sender, e) => {
                ((ListView)sender).SelectedItem = null;
            };
            BindingContext = model;
        }
    }
}
