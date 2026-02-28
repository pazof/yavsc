
using System.ComponentModel.DataAnnotations;
using Yavsc.Abstract;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [YaRequired]
        [YaStringLength(2,Constants.MaxUserNameLength)]
        [YaRegularExpression(Constants.UserNameRegExp)]
        public string Name { get; set; }

        [YaRequired]
        [EmailAddress]
        public string Email { get; set; }

    }
}
