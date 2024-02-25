
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [YaRequired]
        [YaStringLength(512)]
        public string? LoginOrEmail { get; set; }
    }
}
