using ZicMoove.Model.Workflow.Messaging;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System.Collections.Generic;

namespace ZicMoove.Settings
{
    public static class ChatSettings
    {
        public static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }
        private const string statusKey = "chat.status";
        private const string ignoreListKey = "chat.ignoreList";
        private const string denyAnonymousAccessKey = "chat.denyAnonymousAccess";
        private const string denyTouristAccessKey = "chat.denyTouristAccess";

        public static ChatStatus Status
        {
            get
            {
                return AppSettings.GetValueOrDefault<ChatStatus>(statusKey, ChatStatus.OffLine);
            }
            set
            {
                AppSettings.AddOrUpdateValue<ChatStatus>(statusKey, value);
            }
        }
        public static List<string> IgnoreList
        {
            get
            {
                return AppSettings.GetValueOrDefault<List<string>>(ignoreListKey);
            }
        }
        public static void Ignore(string userId)
        {
            var ignoreList = IgnoreList;
            ignoreList.Add(userId);
            AppSettings.AddOrUpdateValue(ignoreListKey, ignoreList);
        }
        public static void UnIgnore(string userId)
        {
            var ignoreList = IgnoreList;
            ignoreList.Remove(userId);
            AppSettings.AddOrUpdateValue(ignoreListKey, ignoreList);
        }
        public static void UnIgnoreAll()
        {
            var ignoreList = new List<string>();
            AppSettings.AddOrUpdateValue(ignoreListKey, ignoreList);
        }
        public static bool DenyAnonymousAccess
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(denyAnonymousAccessKey);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(denyAnonymousAccessKey, value);
            }
        }
        public static bool DenyTouristAccess
        {
            get
            {
                return AppSettings.GetValueOrDefault<bool>(denyTouristAccessKey);
            }
            set
            {
                AppSettings.AddOrUpdateValue<bool>(denyTouristAccessKey, value);
            }
        }
    }
}
