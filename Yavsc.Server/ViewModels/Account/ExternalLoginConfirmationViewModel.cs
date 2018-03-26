
using System.ComponentModel.DataAnnotations;


namespace Yavsc.ViewModels.Account
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
