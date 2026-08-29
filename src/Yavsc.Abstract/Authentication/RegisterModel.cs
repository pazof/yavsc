using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Account
{
    public class RegisterModel
    {

        [StringLength(Constants.MaxUserNameLength)]
        [RegularExpression(Constants.UserNameRegExp)]
        [DataType(DataType.Text)]
        [Display(Name = "UserName", Description = "User name")]
        public string UserName { get; set; }

        [Required()]
        [StringLength(maximumLength: 102, MinimumLength = 5)]
        [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
        [Display(Name = "Email", Description = "E-Mail")]
        public string Email { get; set; }

        [StringLength(maximumLength:100, MinimumLength = 6,
        ErrorMessage = "Le mot de passe doit contenir au moins 8 caratères")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "ConfirmPassword", Description ="Password Confirmation")]
        public string ConfirmPassword { get; set; }

    }
}
