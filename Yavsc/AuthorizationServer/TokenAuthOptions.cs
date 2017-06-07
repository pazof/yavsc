using System;
using System.IdentityModel.Tokens;

namespace Yavsc
{
    [Obsolete("Use OAuth2AppSettings instead")]
    public class TokenAuthOptions
    {
        /// <summary>
        /// Public's identification
        /// </summary>
        /// <returns></returns>
        public string Audience { get; set; }
        /// <summary>
        /// Identity authority
        /// </summary>
        /// <returns></returns>
        public string Issuer { get; set; }
        /// <summary>
        /// Signin key and signature algotythm
        /// </summary>
        /// <returns></returns>
        public SigningCredentials SigningCredentials { get; set; }
        public int ExpiresIn { get; set; }
    }
}