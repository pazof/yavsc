using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Yavsc.ViewModels.Account;

namespace Yavsc.Helpers {
    public static class HttpContextExtensions {
        public static IEnumerable<YaAuthenticationDescription> GetExternalProviders(this HttpContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            return from description in context.Authentication.GetAuthenticationSchemes()
                   where !string.IsNullOrEmpty(description.DisplayName)
                   select 
                            ( new YaAuthenticationDescription 
                            {
                                DisplayName = description.DisplayName,
                                AuthenticationScheme = description.AuthenticationScheme,
                                Items = description.Items
                            });;
        }

        public static bool IsProviderSupported(this HttpContext context, string provider) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            return (from description in context.GetExternalProviders()
                    where string.Equals(description.AuthenticationScheme, provider, StringComparison.OrdinalIgnoreCase)
                    select description).Any();
        }
    }


}