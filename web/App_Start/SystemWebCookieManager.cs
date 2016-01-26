//
//  SystemWebCookieManager.cs
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
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;

namespace Yavsc.App_Start
{
	public class SystemWebCookieManager : ICookieManager
	{
		public string GetRequestCookie(IOwinContext context, string key)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
			var cookie = webContext.Request.Cookies[key];
			return cookie == null ? null : cookie.Value;
		}

		public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

			bool domainHasValue = !string.IsNullOrEmpty(options.Domain);
			bool pathHasValue = !string.IsNullOrEmpty(options.Path);
			bool expiresHasValue = options.Expires.HasValue;

			var cookie = new HttpCookie(key, value);
			if (domainHasValue)
			{
				cookie.Domain = options.Domain;
			}
			if (pathHasValue)
			{
				cookie.Path = options.Path;
			}
			if (expiresHasValue)
			{
				cookie.Expires = options.Expires.Value;
			}
			if (options.Secure)
			{
				cookie.Secure = true;
			}
			if (options.HttpOnly)
			{
				cookie.HttpOnly = true;
			}

			webContext.Response.AppendCookie(cookie);
		}

		public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			AppendResponseCookie(
				context,
				key,
				string.Empty,
				new CookieOptions
				{
					Path = options.Path,
					Domain = options.Domain,
					Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				});
		}
	}

}