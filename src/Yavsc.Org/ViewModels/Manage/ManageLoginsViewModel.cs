using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Yavsc.ViewModels.Manage
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
    }
}
