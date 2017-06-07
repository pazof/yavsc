
using System;
using Microsoft.AspNet.Builder;

namespace Yavsc.Auth
{
    /// <summary>
    /// Extension methods to add Google authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class GoogleAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="GoogleMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Google authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<GoogleMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="GoogleMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Google authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="YavscGoogleOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseGoogleAuthentication(this IApplicationBuilder app, YavscGoogleOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<GoogleMiddleware>(options);
        }
    }
}