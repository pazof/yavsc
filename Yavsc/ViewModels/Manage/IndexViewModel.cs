using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Yavsc.Models;

namespace Yavsc.ViewModels.Manage
{
    public class IndexViewModel
    {
        public string UserName {get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }

        public Activity Activity { get; set; }

        public long PostsCounter { get; set; }

        public IAccountBalance Balance { get; set; }

        public long ActiveCommandCount { get; set; }

        public bool HasDedicatedCalendar { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
