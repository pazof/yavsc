using Microsoft.AspNet.Http;
using System;

namespace OAuth.AspNet.AuthServer
{

    /// <summary>
    /// Contains data about the OAuth client redirect URI
    /// </summary>
    public class OAuthValidateClientRedirectUriContext : BaseValidatingClientContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthValidateClientRedirectUriContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="clientId"></param>
        /// <param name="redirectUri"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "redirect_uri is a string parameter")]
        public OAuthValidateClientRedirectUriContext(HttpContext context, OAuthAuthorizationServerOptions options, string clientId, string redirectUri) : base(context, options, clientId)
        {
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Gets the client redirect URI
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "redirect_uri is a string parameter")]
        public string RedirectUri { get; private set; }

        /// <summary>
        /// Marks this context as validated by the application. IsValidated becomes true and HasError becomes false as a result of calling.
        /// </summary>
        /// <returns></returns>
        public override bool Validated()
        {
            if (string.IsNullOrEmpty(RedirectUri))
            {
                // Don't allow default validation when redirect_uri not provided with request
                return false;
            }
            return base.Validated();
        }

        /// <summary>
        /// Checks the redirect URI to determine whether it equals <see cref="RedirectUri"/>.
        /// </summary>
        /// <param name="redirectUri"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "redirect_uri is a string parameter")]
        public bool Validated(string redirectUri)
        {
            if (redirectUri == null)
            {
                throw new ArgumentNullException("redirectUri");
            }

            if (!string.IsNullOrEmpty(RedirectUri) &&
                !string.Equals(RedirectUri, redirectUri, StringComparison.Ordinal))
            {
                // Don't allow validation to alter redirect_uri provided with request
                return false;
            }

            RedirectUri = redirectUri;

            return Validated();
        }
    }

}
