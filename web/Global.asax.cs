
﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using Yavsc.Formatters;
using Yavsc.Model.FrontOffice;

namespace Yavsc
{
	/// <summary>
	/// Mvc application.
	/// </summary>
	public class MvcApplication : System.Web.HttpApplication
	{
		/// <summary>
		/// Registers the routes.
		/// </summary>
		/// <param name="routes">Routes.</param>
		public static void RegisterRoutes (RouteCollection routes)
		{

			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute ("Scripts/{*pathInfo}");
			routes.IgnoreRoute ("Theme/{*pathInfo}");
			routes.IgnoreRoute ("images/{*pathInfo}");
			routes.IgnoreRoute ("xmldoc/{*pathInfo}"); // xml doc 
			routes.IgnoreRoute ("htmldoc/{*pathInfo}"); // html doc 
			routes.IgnoreRoute ("favicon.ico");
			routes.IgnoreRoute ("favicon.png");
			routes.IgnoreRoute ("robots.txt");
			routes.MapRoute (
				"Blog",
				"Blog/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			);
			routes.MapRoute (
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id=UrlParameter.Optional}
			);
		}
		/// <summary>
		/// Applications the start.
		/// </summary>
		protected void Application_Start ()
		{
			AreaRegistration.RegisterAllAreas ();
			GlobalConfiguration.Configuration.Routes.MapHttpRoute(
						name: "DefaultApi",
				routeTemplate: "api/{controller}/{action}/{*id}",
				defaults: new { controller = "WorkFlow", action="Index", id=0 }
					);

			RegisterRoutes (RouteTable.Routes);
		}
	}
}
