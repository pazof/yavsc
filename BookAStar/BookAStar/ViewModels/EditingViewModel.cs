using BookAStar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    /// <summary>
    /// Used to make the DataManager know how
    /// to sync local and remote data
    /// </summary>
    public class EditingViewModel: ViewModel
    {
        private LocalState state;
        public LocalState State {
            get
            {
                return state;
            }
            set
            {
                base.SetProperty<LocalState>(ref state, value);
            }
        } 
    }
}
