
﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;

namespace Yavsc
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes (RouteCollection routes)
		{

			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}");

			routes.MapRoute (
				"Blog",
				"Blog/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			); 
			routes.MapRoute (
				"Blogs",
				"Blogs/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional}
			);

			routes.MapRoute (
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = "" }
			);

		}

		protected void Application_Start ()
		{
			AreaRegistration.RegisterAllAreas ();

			GlobalConfiguration.Configuration.Routes.MapHttpRoute(
						name: "DefaultApi",
				routeTemplate: "api/{controller}/{action}/{*path}",
				defaults: new { controller = "WorkFlow", action="Index", path = "" }
					);

			RegisterRoutes (RouteTable.Routes);
			Error += HandleError;
		}

		void HandleError (object sender, EventArgs e)
		{
			if (Server.GetLastError ().GetBaseException () is System.Web.HttpRequestValidationException) {
				Response.Clear ();
				Response.Write ("Invalid characters.<br>");
				Response.Write ("You may want to use your " +
				"browser to go back to your form. " +
				"You also can <a href=\"" +
				Request.UrlReferrer +
				"\">hit the url referrer</a>.");
				Response.StatusCode = 200;
				Response.End ();        
			} 
		}
	 
	}
}
