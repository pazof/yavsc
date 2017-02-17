using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using ZicMoove.Data;

namespace ZicMoove.Pages.ClientPages
{
    public partial class ActivityPage 
    {
        public ActivityPage()
        {
            InitializeComponent();
            ItemsSource = DataManager.Instance.Activities;
            
        }
        public ActivityPage(object context)
        {
            BindingContext = context;
        }
    }
}
