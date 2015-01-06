
﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using Yavsc.Formatters;

namespace Yavsc
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes (RouteCollection routes)
		{

			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute ("Scripts/{*pathInfo}");
			routes.IgnoreRoute ("Theme/{*pathInfo}");
			routes.IgnoreRoute ("images/{*pathInfo}");

			routes.MapRoute (
				"Blog",
				"Blog/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			); 

			/*routes.MapRoute (
				"Default", 
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }
				);*/
			routes.MapRoute (
				"Default",
				"{controller}/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			);
		}

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
