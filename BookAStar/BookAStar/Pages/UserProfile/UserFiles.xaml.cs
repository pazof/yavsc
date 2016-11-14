using Xamarin.Forms;

namespace BookAStar.Pages.UserProfile
{
    using ViewModels.UserProfile;
    using Data;
    public partial class UserFiles : ContentPage
    {
        protected DirectoryInfoViewModel model;

        public UserFiles()
        {
            InitializeComponent();

            var currentDir = DataManager.Current.RemoteFiles.CurrentItem;

            BindingContext = new DirectoryInfoViewModel(currentDir)
            {
                RefreshCommand = new Command(() =>
                {
                    DataManager.Current.RemoteFiles.Execute(null);

                    this.dirlist.EndRefresh();
                    this.filelist.EndRefresh();
                })
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }
    }
}
