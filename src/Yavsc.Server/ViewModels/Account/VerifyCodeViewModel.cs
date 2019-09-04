using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class VerifyCodeViewModel
    {
        [YaRequired]
        public string Provider { get; set; }

        [YaRequired]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Se souvenir de ce navigateur?")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "Se souvenir de moi?")]
        public bool RememberMe { get; set; }
    }
}
