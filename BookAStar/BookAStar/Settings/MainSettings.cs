// Helpers/Settings.cs
using BookAStar.Model;
using BookAStar.Model.Auth.Account;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
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

        public static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants
        public static readonly string SettingsDefault = string.Empty;
        public static readonly string EntityDataSettingsPrefix = Constants.YavscApiUrl;
        private const string userNameKey = "user_id";
        private const string PushNotificationsKey = "pushNotifs";
        private const string AllowGPSUsageKey = "allowGPSUsage";
        private const string UserListsKey = "userList";
        private const string GoogleRegIdKey = "googleRedId";
        private const string AllowProBookingOnlyKey = "allowProBookingOnly";


        private static readonly string UserIdDefault =
            string.Empty;
        private static readonly bool PushNotificationsDefault = false;

        public static readonly string GoogleSenderId = "325408689282";

        private const string MusicalKey = "musical_prefs";
        private const string EnvironKey = "environ_prefs";
        private static readonly Dictionary<string, double> MusicalDefault =
            new Dictionary<string, double> {
                { "Pop", 0.5 }, { "Hip Hop" , 0.5 }, { "Rock" , 0.5 }, { "Funk", 0.5 },
                { "R&B", 0.5 }, { "Jazz", 0.5 }
            };
        private static readonly Dictionary<string, double> musical = new Dictionary<string, double>();
        private static readonly Dictionary<string, double> EnvironDefault =
            new Dictionary<string, double> {
                { "Discothèque", 0.5 }, { "Salles de concert", 0.5 },
                { "Piano bar", 0.5 }, { "Bar", 0.5 }, { "Cinema", 0.5 },
                { "Théatre", 0.5 }, { "Salles des fêtes", 0.5 },
                { "Espace publique", 0.5 }
            };
        private static readonly Dictionary<string, double> environ = new Dictionary<string, double>();


        #endregion

        public static string UserName
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(userNameKey, null);
            }
        }
        public const string bookQueryNotificationsKey = "BookQueryNotifications";
        public static BookQueryData[] GetBookQueryNotifications()
        {
            // Do not return any null List
            var json = AppSettings.GetValueOrDefault<string>(bookQueryNotificationsKey);
            if (!string.IsNullOrWhiteSpace(json))
                return JsonConvert.DeserializeObject<BookQueryData[]>(json);
            return new BookQueryData[] { };
        }

        public static BookQueryData[] AddBookQueryNotification(BookQueryData query)
        {
            var existing = new List<BookQueryData>(GetBookQueryNotifications());
            existing.Add(query);
            var result = existing.ToArray();
            AppSettings.AddOrUpdateValue(bookQueryNotificationsKey,
                JsonConvert.SerializeObject(result));
            return result;
        }

        public static string GoogleRegId
        {
            set
            {
                var oldregid = GoogleRegId;
                AppSettings.AddOrUpdateValue<string>(GoogleRegIdKey, value);
                // TODO If it changed, and there's an identified user,
                // Inform the server of it.
                if (oldregid != value)
                {
                    App.PostDeviceInfo();
                }
            }
            get { return AppSettings.GetValueOrDefault<string>(GoogleRegIdKey); }
        }

        private static ObservableCollection<User> accountList = null;
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

        public static User CurrentUser
        {
            get
            {
                var uname = UserName;
                if (uname == null) return null;
                return AccountList.Where(
                    u => u.UserName == uname
                    ).FirstOrDefault();
            }
            set
            {
                var olduserid = CurrentUser?.Id;
                AppSettings.AddOrUpdateValue<string>(userNameKey, value?.UserName);
                // TODO if it changes, for a valid 
                // ident, and we've got a GoogleRedId, inform the server
                // of it.
                if (value != null)
                {
                    if (olduserid != value.Id)
                    {
                        App.PostDeviceInfo();
                        if (UserChanged!=null)
                            UserChanged.Invoke(App.Current, new EventArgs());
                    }
                }
                else if (olduserid != null)
                {
                    if (UserChanged != null)
                        UserChanged.Invoke(App.Current, new EventArgs());
                    // TODO else Unregister this device
                }
            }
        }

        public static event EventHandler<EventArgs> UserChanged;

        /// <summary>
        /// Saves the given user account in the account list.
        /// An existent presenting the same Id will be dropped.
        /// </summary>
        /// <param name="user"></param>
        public static void SaveUser(User user)
        {
            var existent = AccountList.FirstOrDefault(u => u.Id == user.Id);
            if (existent != null)
                AccountList.Remove(existent);
            AccountList.Add(user);
            var json = JsonConvert.SerializeObject(AccountList.ToArray());
            AppSettings.AddOrUpdateValue(UserListsKey, json);
        }
        /// <summary>
        /// Gets an account connection info, given its name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static User GetUser(string username)
        {
            return AccountList.FirstOrDefault(a => a.UserName == username);
        }

        // FIXME real time usage
        /// <summary>
        /// Enables/disables push notifications
        /// </summary>
        public static bool PushNotifications
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(
                    PushNotificationsKey,
                    PushNotificationsDefault);
            }
            set
            {
                // TODO Stop Broadcast receiver
                AppSettings.AddOrUpdateValue<bool>(
                    PushNotificationsKey,
                    value);
            }
        }

        // FIXME real time usage
        /// <summary>
        /// Enables/disables GPS usage
        /// </summary>
        public static bool AllowGPSUsage
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(
                    AllowGPSUsageKey,
                    PushNotificationsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(
                    AllowGPSUsageKey,
                    value);
            }
        }

        // TODO make it a server side user's parameter
        /// <summary>
        /// Only allow professionals to ask for user's services
        /// </summary>
        public static bool AllowProBookingOnly
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(
                    AllowProBookingOnlyKey,
                    PushNotificationsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(
                    AllowProBookingOnlyKey,
                    value);
            }
        }
        public static void SetMusical(string key, double value)
        {
            AppSettings.AddOrUpdateValue<double>(MusicalKey + key, value);
        }

        public static double GetMusical(string key)
        {
            return AppSettings.GetValueOrDefault<double>(MusicalKey + key, MusicalDefault[key]);
        }

        public static Dictionary<string, double> Musical
        {
            get
            {
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
        /// <summary>
        /// Use a sub-key to make persist different tables with the same definition.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="collection"></param>
        /// <param name="subKey"></param>
        public static bool Populate<V,K>(this ILocalEntity<V, K> collection, string subKey = null) where K : IEquatable<K>
        {
            var key = GetCollectionKey<V>(subKey);
            var data = AppSettings.GetValueOrDefault<string>(key, null);
            if (!string.IsNullOrWhiteSpace(data))
            {
                var items = JsonConvert.DeserializeObject<IList<V>>(data);
                if (items != null)
                foreach (var item in items)
                    collection.Add(item);
                var cursorKey = GetCursorKey<V>(subKey);
                var index = AppSettings.GetValueOrDefault<int>(cursorKey, -1);
                if (index>=0)
                {
                    collection.Seek(index);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves a list
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="collection"></param>
        /// <param name="subKey"></param>
        public static void SaveEntity<V,K>(this ILocalEntity<V, K> collection, string subKey=null) where K : IEquatable<K>
        {
            if (collection == null) return;
            var key = GetCollectionKey<V>(subKey);
            var cursorKey = GetCursorKey<V>(subKey);
            AppSettings.AddOrUpdateValue(key, JsonConvert.SerializeObject(collection));
            if (collection.CurrentItem!=null)
            {
                int index =  collection.IndexOf(collection.CurrentItem) ;
                AppSettings.AddOrUpdateValue<int>(cursorKey, index);
            }
        }
        private static string GetCursorKey<V>(string subKey)
        {
            return $"{EntityDataSettingsPrefix}/{subKey}/{typeof(V).FullName}/c";
        }
        private static string GetCollectionKey<V>(string subKey)
        {
            return $"{EntityDataSettingsPrefix}/{subKey}/{typeof(V).FullName}";
        }
    }
}
