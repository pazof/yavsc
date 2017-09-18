using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required][Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Le {0} doit être long d'au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password, ErrorMessage="Les mots de passe doivent contenir au moins un caractère spécial, qui ne soit ni une lettre ni un chiffre.")]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Le mot de passe et sa confirmation ne sont pas les mêmes.")]
        public string ConfirmPassword { get; set; }

        public string GoogleRegId { get; set; }
    }
}
