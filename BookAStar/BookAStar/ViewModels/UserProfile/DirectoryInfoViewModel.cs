
using System.Collections.ObjectModel;
using System.Windows.Input;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.UserProfile
{
    using Model.FileSystem;

    public class DirectoryInfoViewModel :  ViewModel
    {
        public ObservableString SubPath { get; set; }
        public ObservableCollection<string> SubDirectories { get; set; }
        public ObservableCollection<UserFileInfo> FileInfo { get; set; }
        public ICommand RefreshCommand { get; set; }
        public DirectoryInfoViewModel (UserDirectoryInfo model=null)
        {
            Data.DataManager.Current.RemoteFiles.CollectionChanged += RemoteFiles_CollectionChanged;

            if (model == null)
                model = Data.DataManager.Current.RemoteFiles.CurrentItem;
            SubDirectories = new ObservableCollection<string> (model.SubDirectories);
            SubPath = new ObservableString (model.SubPath);
            FileInfo = new ObservableCollection<Model.FileSystem.UserFileInfo> (model.Files);
        }

        private void RemoteFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
