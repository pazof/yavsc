using System;
using XLabs.Forms.Mvvm;
using System.ComponentModel;
using Newtonsoft.Json;

namespace ZicMoove.ViewModels.Validation
{
    /// <summary>
    /// Used to make the DataManager know how
    /// to sync local and remote data
    /// </summary>
    public abstract class EditingViewModel<DataType>: ViewModel
    {

        public DataType Data { get; set; }

        private ViewModelState viewModelState = new ViewModelState();

        public ViewModelState ModelState
        {
            get
            {
                return viewModelState;
            }
            set
            {
                base.SetProperty<ViewModelState>(ref viewModelState, value);
            }
        }

        public EditingViewModel(DataType data)
        {
            this.Data = data;
            ModelState = new ViewModelState();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName != "ModelState")
                Check();
        }
        /// <summary>
        /// Must compute the ModelState property
        /// from the Data one.
        /// </summary>
        public abstract void Check();

        /* NOTE : I had a dream.

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
*/
    }
}
