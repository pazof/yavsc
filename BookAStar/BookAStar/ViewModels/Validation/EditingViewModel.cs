using System;
using XLabs.Forms.Mvvm;
using System.ComponentModel;
using Newtonsoft.Json;

namespace BookAStar.ViewModels.Validation
{
    /// <summary>
    /// Used to make the DataManager know how
    /// to sync local and remote data
    /// </summary>
    public class EditingViewModel<DataType>: ViewModel
    {
        [JsonIgnore]
        public Action<DataType, ModelState> CheckCommand { set; get; }

        public DataType Data { get; set; }

        private ModelState viewModelState = new ModelState();

        public ModelState ViewModelState
        {
            get
            {
                return viewModelState;
            }
            set
            {
                base.SetProperty<ModelState>(ref viewModelState, value);
            }
        }

        public EditingViewModel(DataType data)
        {
            this.Data = data;
            ViewModelState = new ModelState();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            Check();
        }
        public virtual void Check()
        {
            if (CheckCommand != null)
            {
                ViewModelState.Clear();
                CheckCommand(Data, ViewModelState);
            }
        }
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
