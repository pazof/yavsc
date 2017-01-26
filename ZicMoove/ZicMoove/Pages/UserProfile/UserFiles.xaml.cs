using Xamarin.Forms;

namespace ZicMoove.Pages.UserProfile
{
    using ViewModels.UserProfile;
    using Data;
    using System.Windows.Input;

    public partial class UserFiles : ContentPage
    {
        protected DirectoryInfoViewModel model;
        public UserFiles()
        {
            InitializeComponent();
            var current = DataManager.Instance.RemoteFiles.CurrentItem;
            if (current != null)
                BindingContext = new DirectoryInfoViewModel(current);
            else BindingContext = new DirectoryInfoViewModel
            {
                UserName = MainSettings.UserName
            };
        }

        public UserFiles(DirectoryInfoViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnBindingContextChanged()
        {
            model = BindingContext as DirectoryInfoViewModel;
            if (model != null)
                model.RefreshCommand = new Command(() =>
            {
                DataManager.Instance.RemoteFiles.Execute(null);
                var item = DataManager.Instance.RemoteFiles.CurrentItem;
                if (item != null)
                    model.InnerModel = item;
           //     this.dirlist.EndRefresh();
           //     this.filelist.EndRefresh();
            });
            base.OnBindingContextChanged();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            model.RefreshCommand.Execute(model.SubPath);
        }

    }
}
