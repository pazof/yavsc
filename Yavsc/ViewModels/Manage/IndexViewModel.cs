using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace Yavsc.ViewModels.Manage
{
    using Models.Bank;
    using Models;
    using Models.Workflow;
    public class IndexViewModel
    {
        public string UserName {get; set; }

        public string Avatar { get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public List<UserActivity> Activity { get; set; }

        public bool HaveProfessionalSettings { get; set; }
        public bool HaveActivityToConfigure { get; set; }

        public long PostsCounter { get; set; }

        public AccountBalance Balance { get; set; }

        public long ActiveCommandCount { get; set; }

        public bool HasDedicatedCalendar { get; set; }

        public IEnumerable<string> Roles { get; set; }

        public string FullName { get; set; }

        public string PostalAddress { get; set; }

        public BankIdentity BankInfo { get; set; }
        public long DiskQuota { get; set; }
        public long DiskUsage { get; set; }

        public string DedicatedCalendarId { get; set; }
    }
}
