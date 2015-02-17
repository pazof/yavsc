//
//  WebApiConfig.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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

﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Yavsc.Formatters;
using Yavsc.Model.FrontOffice;
using System.Web.Http;

namespace Yavsc
{
	/// <summary>
	/// Web API config.
	/// </summary>
	public static class WebApiConfig
	{
		/// <summary>
		/// Gets the URL prefix.
		/// </summary>
		/// <value>The URL prefix.</value>
		public static string UrlPrefix { get { return "api"; } }

		/// <summary>
		/// Gets the URL prefix relative.
		/// </summary>
		/// <value>The URL prefix relative.</value>
		public static string UrlPrefixRelative { get { return "~/api"; } }

		/// <summary>
		/// Register the specified config.
		/// </summary>
		/// <param name="config">Config.</param>
		public static void Register(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: WebApiConfig.UrlPrefix + "/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
	
}
