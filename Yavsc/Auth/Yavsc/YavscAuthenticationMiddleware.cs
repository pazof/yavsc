//
//  YavscAuthenticationMiddleware.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.WebEncoders;

namespace Yavsc.Authentication
{
    public class YavscAuthenticationMiddleware : OAuthMiddleware<YavscAuthenticationOptions>
	{
		RequestDelegate _next;
		ILogger _logger;

		public YavscAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            IOptions<SharedAuthenticationOptions> sharedOptions,
			YavscAuthenticationOptions options)
			: base(next, dataProtectionProvider, loggerFactory, encoder, sharedOptions, options)
		{

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            _next = next;

            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _logger = loggerFactory.CreateLogger<YavscAuthenticationMiddleware>();

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (sharedOptions == null)
            {
                throw new ArgumentNullException(nameof(sharedOptions));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

			if(string.IsNullOrEmpty(options.SignInAsAuthenticationType))
			{
				options.SignInAsAuthenticationType = sharedOptions.Value.SignInScheme;
			}

			if(options.StateDataFormat == null)
			{
				var dataProtector = dataProtectionProvider.CreateProtector(typeof(YavscAuthenticationMiddleware).FullName,
				                                            options.AuthenticationScheme);

				options.StateDataFormat = new PropertiesDataFormat(dataProtector);
			}
		}

		// Called for each request, to create a handler for each request.
		protected override AuthenticationHandler<YavscAuthenticationOptions> CreateHandler()
		{
			return new YavscAuthenticationHandler(this.Backchannel,_logger);
		}
	}
}

