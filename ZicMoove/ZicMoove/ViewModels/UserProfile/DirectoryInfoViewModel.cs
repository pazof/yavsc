
using System.Collections.ObjectModel;
using System.Windows.Input;
using XLabs.Forms.Mvvm;

namespace ZicMoove.ViewModels.UserProfile
{
    using System.ComponentModel;
    using Model.FileSystem;

    public class DirectoryInfoViewModel :  ViewModel
    {
        private string subPath;
        public string SubPath
        {
            get { return subPath; }
            set { SetProperty<string>(ref subPath, value); }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { SetProperty<string>(ref userName, value); }
        }

        private ObservableCollection<string> subDirectories;
        public ObservableCollection<string> SubDirectories
        {
            get { return subDirectories; }
            set { SetProperty< ObservableCollection < string >>( ref subDirectories, value) ; }
        }

        private ObservableCollection<UserFileInfo> fileInfo;
        public ObservableCollection<UserFileInfo> FileInfo
        {
            get { return fileInfo; }
            set { SetProperty<ObservableCollection<UserFileInfo>> ( ref fileInfo, value ); }
        }

        UserDirectoryInfo model;
        public UserDirectoryInfo InnerModel {
            get { return model; }
            set {
                if (SetProperty<UserDirectoryInfo>(ref model, value))
                if (model == null)
                {
                    SubDirectories = new ObservableCollection<string>();
                    FileInfo = new ObservableCollection<UserFileInfo>();
                    SubPath = "<no path>";
                    UserName = "<no user>";
                }
                else
                {
                    SubDirectories = new ObservableCollection<string>(model.SubDirectories);
                    FileInfo = new ObservableCollection<UserFileInfo>(model.Files);
                    SubPath = model.SubPath;
                    UserName = model.UserName;
                }
            }
        }

        public DirectoryInfoViewModel(UserDirectoryInfo model = null)
        {
            this.InnerModel = model;
        }

        private ICommand refreshCommand;
        public ICommand RefreshCommand
        {
            get { return refreshCommand; }
            set { SetProperty<ICommand>(ref refreshCommand, value); }
        }
    }
}
