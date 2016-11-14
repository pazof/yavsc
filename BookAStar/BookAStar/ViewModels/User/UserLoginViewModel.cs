using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    class UserLoginViewModel : ViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
