using BookAStar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BookAStar.Pages
{
    public partial class EditBillingLinePage : ContentPage
    {
        public EditBillingLinePage(BillingLineViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
