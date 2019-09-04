using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [YaRequired]
        [EmailAddress]
        public string Email { get; set; }

        [YaRequired]
        [YaStringLength(100, ErrorMessage = "Le {0} doit être long d'au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et sa confirmation ne sont pas les mêmes.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
