using BookAStar.Helpers;
using BookAStar.Model.Auth.Account;
using BookAStar.Pages;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using XLabs.Forms.Behaviors;
using XLabs.Forms.Controls;
using XLabs.Forms.Mvvm;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.ViewModels
{
    
    internal class DashboardViewModel : ViewModel
    {
        int rating;
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

        public string UserId
        {
            get
            {
                return User?.Id;
            }
        }

        public bool AllowUseMyPosition
        {
            get
            {
                return MainSettings.AllowGPSUsage;
            }
            set
            {
                MainSettings.AllowGPSUsage = value;
            }
        }

        public bool AllowProBookingOnly
        {
            get
            {
                return MainSettings.AllowProBookingOnly;
            }
            set
            {
                MainSettings.AllowProBookingOnly = value;
            }
        }

        public bool ReceivePushNotifications
        {
            get
            {
                return MainSettings.PushNotifications;
            }
            set
            {
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

        private bool userIsPro = false;

        public DashboardViewModel()
        {
            Accounts = MainSettings.AccountList;
            User = MainSettings.CurrentUser;
            UpdateUserMeta();
            Rating = 2;
            UserNameGesture = new RelayGesture((g, x) =>
            {
                if (g.GestureType == GestureType.LongPress)
                {
                    Resolver.Resolve<INavigationService>().NavigateTo<AccountChooserPage>(true);
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

        public bool UserIsPro
        {
            get { return User?.Roles?.Contains("Performer") ?? false ; }
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
                newUserIsPro = UserIsPro;

                newQueryCount = newUserIsPro ? DataManager.Current.BookQueries.Count : 0;

                newStatusString = newUserIsPro ?
                     $"Profile professionel renseigné" :
                    "Profile professionel non renseigné";
                newQueriesButtonText = newUserIsPro ?
                     $"{newQueryCount} demandes valides en cours" :
                    "Profile professionel non renseigné";
                newAvatar = UserHelpers.Avatar(user.Avatar);
            }
            SetProperty<bool>(ref haveAnUser, newHaveAnUser, "UserIsPro");
            SetProperty<bool>(ref userIsPro, newUserIsPro, "UserIsPro");
            SetProperty<string>(ref performerStatus, newStatusString, "PerformerStatus");
            SetProperty<string>(ref userQueries, newQueriesButtonText, "PerformerStatus");
            SetProperty<long>(ref queryCount, newQueryCount, "QueryCount");
            SetProperty<ImageSource>(ref avatar, newAvatar, "Avatar");
        }

        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateUserMeta();
        }

        public RelayGesture UserNameGesture { get; set; }

    }
}