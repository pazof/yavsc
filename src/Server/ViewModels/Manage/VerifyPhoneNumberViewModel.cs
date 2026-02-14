using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{
    public class VerifyPhoneNumberViewModel
    {
        [YaRequired]
        public string Code { get; set; }

        [YaRequired]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
