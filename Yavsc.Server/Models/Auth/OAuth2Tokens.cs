using System;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Auth
{
    /// <summary>
    /// OffLine OAuth2 Token
    /// To use against a third party Api
    /// </summary>
    public partial class OAuth2Tokens
    {
        /// <summary>
        /// Unique identifier, equals the user email from OAuth provider
        /// </summary>
        /// <returns></returns>
        [Key]
        public string UserId { get; set; }

        /// <summary>
        /// Expiration date &amp; time
        /// </summary>
        /// <returns></returns>
        public DateTime Expiration { get; set; }
        /// <summary>
        /// Expiration time span in seconds
        /// </summary>
        /// <returns></returns>
        public string ExpiresIn { get; set; }

        /// <summary>
        /// Should always be <c>Bearer</c> ...
        /// </summary>
        /// <returns></returns>
        public string TokenType { get; set; }

        /// <summary>
        /// The Access Token!
        /// </summary>
        /// <returns></returns>
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token
        /// </summary>
        /// <returns></returns>
        public string RefreshToken { get; set; }
    }
}
