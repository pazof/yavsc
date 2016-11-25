using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.Signing
{
    class DocSigningViewModel: ViewModel
    {
        /// <summary>
        /// The doc to sign, in Markdown format 
        /// </summary>
        public string Document { get; set;  }
    }
}
