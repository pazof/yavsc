using System;
using System.Linq;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.ViewModels.FrontOffice
{
    public class PerformerProfileViewModel
    {
        public string UserName { get; set; }
        public string PerformerId { get; set; }
        public bool Active { get; set; }
        public bool AcceptNotifications { get; set; }
        public bool AcceptPublicContact { get; set; }
        public UserActivity Context { get; set; }

        public object Settings { get; set; }
        public UserActivity[] Extra { get; set; }

        public string WebSite { get; set; }
        public string SettingsClassName { get; set; }

        public PerformerProfileViewModel(PerformerProfile profile, string activityCode, object settings)
        {
            UserName = profile.Performer.UserName;
            PerformerId = profile.PerformerId;
            Active = profile.Active;
            AcceptNotifications = profile.AcceptNotifications;
            AcceptPublicContact = profile.AcceptPublicContact;
            Context = profile.Activity.FirstOrDefault(a => a.DoesCode == activityCode);
            SettingsClassName = Context.Does.SettingsClassName;

            Settings = settings;
            WebSite = profile.WebSite;
            Extra = profile.Activity.Where(a => a.DoesCode != activityCode).ToArray();
             

        }
    }
}