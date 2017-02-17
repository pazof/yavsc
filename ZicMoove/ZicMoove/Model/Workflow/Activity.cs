using Newtonsoft.Json;
using System;
using YavscLib;

namespace ZicMoove.Model.Workflow
{
    public class Activity : IActivity
    {
        public string Code
        {
            get; set;
        }

        public DateTime DateCreated
        {
            get; set;
        }

        public DateTime DateModified
        {
            get; set;
        }

        public string ModeratorGroupName
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string ParentCode
        {
            get; set;
        }

        public string Photo
        {
            get; set;
        }

        [JsonIgnore]
        public string PhotoUri
        {
            get { return Constants.YavscHomeUrl + Photo; }
        }

        public int Rate
        {
            get; set;
        }

        public string SettingsClassName
        {
            get; set;
        }

        public string UserCreated
        {
            get; set;
        }

        public string UserModified
        {
            get; set;
        }

        public CommandForm[] Forms { get; set; }

        public string Description
        {
            get;
            set;
        }
    }
}
