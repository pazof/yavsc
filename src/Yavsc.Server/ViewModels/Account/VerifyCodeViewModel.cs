using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Se souvenir de ce navigateur?")]
        public bool RememberMe { get; set; }
    }
}
