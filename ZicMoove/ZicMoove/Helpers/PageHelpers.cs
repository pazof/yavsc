using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove.Helpers
{
    public  static class PageHelpers
    {
        public static async Task<bool> Confirm(this Page page, string title, string procedure)
        {
            string yes = "Oui", no = "Non";
            var answer = await page.DisplayAlert(title,
               procedure, yes, no);
            return answer;
        }
    }
}
