
using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [StringLength(512)]
        public string LoginOrEmail { get; set; }
    }
}
