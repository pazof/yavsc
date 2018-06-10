using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Account
{
    public class RegisterViewModel
    {
        // ErrorMessage = "",
         

        [StringLength(102)]
        [YaRegularExpression(@"[a-zA-Z0-9 .'_-]+", ErrorMessageResourceName="InvalidUserName", ErrorMessageResourceType = typeof(RegisterViewModel))]
        public string UserName { get; set; }

        [YaRequired()]
   //    [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]

        // ErrorMessage = "Les mots de passe doivent contenir au moins un caractère spécial, qui ne soit ni une lettre ni un chiffre.")]
        
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceName = "PassAndConfirmDontMach", ErrorMessageResourceType = typeof(RegisterViewModel) )]
        public string ConfirmPassword { get; set; }

        public string GoogleRegId { get; set; }
    }
}
