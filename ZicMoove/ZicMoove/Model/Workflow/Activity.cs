using Newtonsoft.Json;
using System;
using XLabs.Forms.Mvvm;

namespace ZicMoove.Model.Workflow
{
    using YavscLib;
    public class Activity : ViewModel, IActivity
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
        private string photo;

        public string LocalPhoto {
            get { return photo; }
               set { SetProperty<string>(ref photo, value);
            } }
    }
}
