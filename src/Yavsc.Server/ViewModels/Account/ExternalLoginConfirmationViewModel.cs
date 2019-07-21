
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [YaStringLength(2,Constants.MaxUserNameLength)]
        [YaRegularExpression(Constants.UserNameRegExp)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
