
using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Manage
{
    public class ChangeUserNameViewModel
    {
        [Required]
        [Display(Name = "New user name"),RegularExpression(Constants.UserNameRegExp)]
        public string NewUserName { get; set; }

    }
}
