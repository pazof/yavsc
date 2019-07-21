using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Account
{
    public class UnregisterViewModel
    {
        [Required]
        public string UserId { get; set; }
        
        public string ReturnUrl { get; set; }

    }
}
