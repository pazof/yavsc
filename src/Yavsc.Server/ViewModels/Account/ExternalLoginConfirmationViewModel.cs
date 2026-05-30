
using System.ComponentModel.DataAnnotations;
using Yavsc.Abstract;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [YaStringLength(2,YavscConstants.MaxUserNameLength)]
        [YaRegularExpression(YavscConstants.UserNameRegExp)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
