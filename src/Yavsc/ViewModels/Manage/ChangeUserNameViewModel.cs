
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{
    public class ChangeUserNameViewModel
    {
        [YaRequired]
        [Display(Name = "New user name"),RegularExpression(Constants.UserNameRegExp)]
        public string NewUserName { get; set; }

    }
}
