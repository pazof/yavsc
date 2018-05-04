using Yavsc.Models;

namespace Yavsc.ViewModels.Manage
{
    public class ProfileEMailUsageViewModel
    {
        public bool Allow { get; set; }

        public ProfileEMailUsageViewModel()
        {
            
        }
        public ProfileEMailUsageViewModel(ApplicationUser user=null)
        {
            Allow = user.AllowMonthlyEmail;
        }
    }
}