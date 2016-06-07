
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Http.Authentication;

namespace Yavsc.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        
        /// <summary>
        /// This value indicates the OAuth client method recieving the code,
        /// in case of.
        /// </summary>
        /// <returns></returns>
        public string ReturnUrl { get; set; }
        
        public IEnumerable<AuthenticationDescription> ExternalProviders { get; set; }
    }
}
