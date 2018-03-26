
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Http.Authentication;

namespace Yavsc.ViewModels.Account
{
    public class SignInViewModel
    {
        /// <summary>
        /// Local user's name.
        /// </summary>
        /// <returns></returns>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Local user's password .
        /// </summary>
        /// <returns></returns>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// When true, asks for a two-factor identification
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Se souvenir de moi?")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Indicates the authentication provider'name chosen to authenticate,
        /// contains "LOCAL" to choose the local application identity
        /// and user password credentials.
        /// </summary>
        /// <returns></returns>
        public string Provider { get; set; }

        
        /// <summary>
        /// This value does NOT indicate the OAuth client method recieving the code,
        /// but the one called once authorized.
        /// </summary>
        /// <returns></returns>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Lists external identity provider descriptions.
        /// </summary>
        /// <returns>an enumeration of the descriptions.</returns>
        public IEnumerable<AuthenticationDescription> ExternalProviders { get; set; }
    }
}
