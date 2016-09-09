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
        public MakeAnEstimatePage()
        {
            var m = new MarkdownView();
            InitializeComponent();
            var md = this.mdview.Markdown;
        }
    }
}
