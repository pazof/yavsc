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
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataHandler;

namespace Yavsc.Model.Authentication
{
	public class YavscAuthenticationMiddleware : AuthenticationMiddleware<YavscAuthenticationOptions>
	{
		public YavscAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, YavscAuthenticationOptions options)
			: base(next, options)
		{ 
			if(string.IsNullOrEmpty(Options.SignInAsAuthenticationType))
			{
				options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
			}

			if(options.StateDataFormat == null)
			{
				var dataProtector = app.CreateDataProtector(typeof(YavscAuthenticationMiddleware).FullName,
				                                            options.AuthenticationType);

				options.StateDataFormat = new PropertiesDataFormat(dataProtector);
			}
		}

		// Called for each request, to create a handler for each request.
		protected override AuthenticationHandler<YavscAuthenticationOptions> CreateHandler()
		{
			return new YavscAuthenticationHandler();
		}
	}
}

