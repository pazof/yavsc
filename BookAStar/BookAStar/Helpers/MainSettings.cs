// Helpers/Settings.cs
using BookAStar.Model.Auth.Account;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar
{
	
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	public static class MainSettings
	{
		
		private static ISettings AppSettings {
			get {
				return CrossSettings.Current;
			}
		}

		#region Setting Constants

		private const string userNameKey = "user_id";
		private const string PushNotificationsKey = "pushNotifs";
        private const string UserListsKey = "userList";
        private const string GoogleRegIdKey = "googleRedId";

        

        private static readonly string UserIdDefault = 
			string.Empty;
		private static readonly bool PushNotificationsDefault = false;

		public static readonly string GoogleSenderId = "325408689282";

		private const string MusicalKey = "musical_prefs";
		private const string EnvironKey = "environ_prefs";
		private static readonly Dictionary<string,double> MusicalDefault =
			new Dictionary<string, double> {
				{ "Pop", 0.5 }, { "Hip Hop" , 0.5 }, { "Rock" , 0.5 }, { "Funk", 0.5 }, 
				{ "R&B", 0.5 }, { "Jazz", 0.5 }
			};
        private static readonly Dictionary<string, double> musical = new Dictionary<string, double>();
        private static readonly Dictionary<string,double> EnvironDefault =
			new Dictionary<string, double> {
				{ "Discothèque", 0.5 }, { "Salles de concert", 0.5 }, 
				{ "Piano bar", 0.5 }, { "Bar", 0.5 }, { "Cinema", 0.5 }, 
				{ "Théatre", 0.5 }, { "Salles des fêtes", 0.5 },
				{ "Espace publique", 0.5 }
			};
        private static readonly Dictionary<string, double> environ = new Dictionary<string, double>();

        public static readonly string YavscApiUrl = "dev.pschneider.fr";

		#endregion

		public static string UserName
        {
			get {
				return AppSettings.GetValueOrDefault<string>(userNameKey, null);
			}
		}

        public  static string GoogleRegId
        {
             set {
                var oldregid = GoogleRegId;
                AppSettings.AddOrUpdateValue<string>(GoogleRegIdKey, value);
                // TODO If it changed, and there's an identified user,
                // Inform the server of it.
                if (oldregid != value)
                {
                    Task.Run( async () => {
                        await App.CurrentApp.PostDeviceInfo();
                    });
                }
            }
            get { return AppSettings.GetValueOrDefault<string>(GoogleRegIdKey);  }
        }

        private static ObservableCollection<User> accountList=null;
        public static ObservableCollection<User> AccountList
        {
            get
            {
                if (accountList == null)
                {
                    accountList = new ObservableCollection<User>();
                    var json = AppSettings.GetValueOrDefault<string>
                        (UserListsKey, null);
                    if (json != null)
                    {
                        var users = JsonConvert.DeserializeObject<User[]>(json);
                        if (users != null)
                        foreach (User user in users)
                        {
                            accountList.Add(user);
                        }
                    }
                }
                return accountList;
            }
        }

        private static User currentUser = null;
        public static User CurrentUser { get
            {
                var uname = UserName;
                if (uname == null) return null;
                return AccountList.Where(
                    u => u.UserName == uname
                    ).FirstOrDefault();
            }
            set
            {
                var olduserid = currentUser?.Id;
                currentUser = value;
                AppSettings.AddOrUpdateValue<string>(userNameKey, value?.UserName);
                // TODO if it changes, for a valid 
                // ident, and we've got a GoogleRedId, inform the server
                // of it.
                if (value != null)
                {
                    if (olduserid != value.Id)
                    {
                        Task.Run(async () => {
                            await App.CurrentApp.PostDeviceInfo();
                        });
                    }
                }
            }
        }

        public static void SaveUser(User user)
        {
            var existent = AccountList.FirstOrDefault(u => u.UserName == user.UserName);
            if (existent != null)
                AccountList.Remove(existent);
            AccountList.Add(user);
            var json = JsonConvert.SerializeObject(AccountList.ToArray());
            AppSettings.AddOrUpdateValue(UserListsKey, json);
        }

        public static User GetUser(string username)
        {
            return AccountList.FirstOrDefault(a=>a.UserName == username);
        }

        public static bool PushNotifications { 
			get { 
				return AppSettings.GetValueOrDefault<bool>(
					PushNotificationsKey,
					PushNotificationsDefault);
			}
			set { 
				AppSettings.AddOrUpdateValue<bool> (
					PushNotificationsKey,
					value);
            }
		}

		public static void SetMusical (string key, double value)
		{
			AppSettings.AddOrUpdateValue <double> (MusicalKey + key, value);
		}

		public static double GetMusical (string key)
		{
			return AppSettings.GetValueOrDefault <double> (MusicalKey + key, MusicalDefault [key]);
		}

		public static Dictionary<string,double> Musical {
			get {
				return musical;
			}
		}

		public static Dictionary<string, double> Environ
        {
            get
            {
                return environ;
            }
        }

        public const string MobileRegistrationUrl = "http://dev.pschneider.fr/api/gcm/register";

        public const string ApplicationName = "BookAStar";
        public static readonly string UserInfoUrl = "http://dev.pschneider.fr/api/me";
    }
}
