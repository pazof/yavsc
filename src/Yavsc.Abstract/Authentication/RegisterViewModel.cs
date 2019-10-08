using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;
using Yavsc.Abstract;
    using Yavsc;

namespace Yavsc.ViewModels.Account
{
    public class RegisterModel
    {

        [YaStringLength(2,Constants.MaxUserNameLength)]
        [YaRegularExpression(Constants.UserNameRegExp)]
        public string UserName { get; set; }

        [YaRequired()]
        [YaStringLength(2,102)]
   //    [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [YaStringLength(6,100)]
        [DataType(DataType.Password)]

        // ErrorMessage = "Les mots de passe doivent contenir au moins un caractère spécial, qui ne soit ni une lettre ni un chiffre.")]
        
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }


    }
}
