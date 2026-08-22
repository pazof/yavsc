
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{
    public class SetUserNameViewModel
    {
        [Required]
        [Display(Name = "User name"),RegularExpression(Constants.UserNameRegExp)]
        public string UserName { get; set; }

    }
}
