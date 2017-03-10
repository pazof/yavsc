using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using XLabs.Forms.Behaviors;
using XLabs.Forms.Controls;
using XLabs.Forms.Mvvm;

namespace ZicMoove.ViewModels.UserProfile
{
    using Data;
    using Helpers;
    using Model.Auth.Account;
    using Pages.UserProfile;
    using Settings;

    public class UserProfileViewModel : ViewModel
    {

        public bool IsAPerformer
        {
            get { return User?.Roles?.Contains("Performer") ?? false; }
        }

        public string UserFilesLabel
        {
            get; set;
        }
        // TODO implementation
        int rating ;
        public int Rating
        {
            get
            {
                return rating;
            }
            set 
            {
                SetProperty<int>(ref rating, value, "Rating");
            }
        }

        private bool allowUseMyPosition = MainSettings.AllowGPSUsage;
        public bool AllowUseMyPosition
        {
            get
            {
                return allowUseMyPosition;
            }
            set
            {
                SetProperty<bool>(ref allowUseMyPosition, value);
                MainSettings.AllowGPSUsage = value;
            }
        }

        private bool allowProBookingOnly = MainSettings.AllowProBookingOnly;
        public bool AllowProBookingOnly
        {
            get
            {
                return allowProBookingOnly;
            }
            set
            {
                SetProperty<bool>(ref allowUseMyPosition, value);
                MainSettings.AllowProBookingOnly = value;
            }
        }
        bool receivePushNotifications = MainSettings.PushNotifications;
        public bool ReceivePushNotifications
        {
            get
            {
                return receivePushNotifications;
            }
            set
            {
                SetProperty<bool>(ref receivePushNotifications, value);
                MainSettings.PushNotifications = value;
            }
        }
        private long queryCount;
        private User user;

        public long QueryCount
        {
            get
            {
                return queryCount;
            }
        }

        public User User
        {
            get { return user; }
            protected set
            {
                SetProperty<User>(ref user, value, "User");
                if (user!=null)
                {
                    user.PropertyChanged += User_PropertyChanged;

                }

                UpdateUserMeta();

            }
        }
        private ImageSource avatar;
        public ImageSource Avatar {  get {
                return avatar;
            } }

        public ObservableCollection<User> Accounts { get; protected set; }
        private string performerStatus;
        public string PerformerStatus
        {
            get
            {
                return performerStatus;
            }
        }
        string userQueries;
        public string UserQueries
        {
            get
            {
                return userQueries;
            }
        }
        public string UserName
        {
            get
            {
                return User?.UserName;
            }
        }

        public UserProfileViewModel()
        {
            Accounts = MainSettings.AccountList;
            User = MainSettings.CurrentUser;
            UpdateUserMeta();
            Rating = 2;
            UserNameGesture = new RelayGesture((g, x) =>
            {
                if (g.GestureType == GestureType.LongPress)
                {
                    NavigationService.NavigateTo("accountChooser");
                }
            });
            MainSettings.UserChanged += MainSettings_UserChanged;

        }

        private void MainSettings_UserChanged(object sender, System.EventArgs e)
        {
            User = MainSettings.CurrentUser;
            UpdateUserMeta();
        }

        bool haveAnUser;

        public bool HaveAnUser
        {
            get { return User!=null; }
        }


        private void UpdateUserMeta ()
        {
            string newStatusString;
            long newQueryCount;
            bool newUserIsPro;
            ImageSource newAvatar;
            string newQueriesButtonText;
            bool newHaveAnUser = user == null;
            if (newHaveAnUser) {
                newQueryCount = 0;
                newUserIsPro = false;
                newStatusString = null;
                newAvatar = null;
                newQueriesButtonText = null;
            }
            else
            {
                newUserIsPro = IsAPerformer;

                newQueryCount = newUserIsPro ? DataManager.Instance.BookQueries.Count : 0;

                newStatusString = newUserIsPro ?
                     $"Profile professionel renseigné" :
                    "Profile professionel non renseigné";
                newQueriesButtonText = newUserIsPro ?
                     $"{newQueryCount} demandes valides en cours" :
                    "Profile professionel non renseigné";
                newAvatar = UserHelpers.Avatar(user.UserName);
            }
            SetProperty<bool>(ref haveAnUser, newHaveAnUser, "HaveAnUser");
            SetProperty<string>(ref performerStatus, newStatusString, "PerformerStatus");
            SetProperty<string>(ref userQueries, newQueriesButtonText, "UserQueries");
            SetProperty<long>(ref queryCount, newQueryCount, "QueryCount");
            SetProperty<ImageSource>(ref avatar, newAvatar, "Avatar");

            NotifyPropertyChanged("UserName");
            NotifyPropertyChanged("AllowProBookingOnly");
            NotifyPropertyChanged("AllowUseMyPosition");
            NotifyPropertyChanged("ReceivePushNotifications");
            NotifyPropertyChanged("AllowUseMyPosition");
            NotifyPropertyChanged("IsAPerformer");
        }

        private void User_PropertyChanged(object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateUserMeta();
        }

        public RelayGesture UserNameGesture { get; set; }

    }
}