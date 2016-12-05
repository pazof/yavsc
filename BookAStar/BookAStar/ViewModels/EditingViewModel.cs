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
        bool existsRemotely;
        public bool ExistsRemotely
        {
            get
            {
                return existsRemotely;
            }
            set
            {
                base.SetProperty<bool>(ref existsRemotely, value);
            }
        }

        bool isValid;
        public bool IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                base.SetProperty<bool>(ref isValid, value);
            }
        }

        bool isDirty;
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                base.SetProperty<bool>(ref isDirty, value);
            }
        }
    }
}
