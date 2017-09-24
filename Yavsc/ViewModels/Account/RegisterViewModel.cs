using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class RegisterViewModel
    {
        // ErrorMessage = "",
         

        [Display(Name = "Nom d'utilisateur")]
        [StringLength(102)]
        [YaRegularExpression(@"[a-zA-Z0-9 ._-]+",
         ErrorMessage = "Caratères autorisés: lettres, chiffres, espace point tiret et souligné.")]
        public string UserName { get; set; }

        [YaRequired("Ce champ est requis.")]
   //    [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

       [YaRequired(ErrorMessage="Spécifiez un mot de passe.")]
      //  [StringLength(100, ErrorMessage = "Le {0} doit être long d'au moins {1} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password,
         ErrorMessage = "Les mots de passe doivent contenir au moins un caractère spécial, qui ne soit ni une lettre ni un chiffre.")]
        
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et sa confirmation ne sont pas les mêmes.")]
        public string ConfirmPassword { get; set; }

        public string GoogleRegId { get; set; }
    }
}
