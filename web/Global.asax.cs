
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
using System.Web.SessionState;

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
			routes.IgnoreRoute ("users/{*pathInfo}");
			routes.IgnoreRoute ("files/{*pathInfo}");
			routes.IgnoreRoute ("avatars/{*pathInfo}");
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
				"Blogs",
				"Blogs/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			); 
			routes.MapRoute (
				"Account",
				"Account/{action}/{user}",
				new { controller = "Account", action = "Index", user=UrlParameter.Optional }
			); 
			routes.MapRoute (
				"Default",
				"{controller}/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			);
		}

		/// <summary>
		/// Starts the Application.
		/// </summary>
		protected void Application_Start ()
		{
			AreaRegistration.RegisterAllAreas ();
			WebApiConfig.Register (GlobalConfiguration.Configuration);
			RegisterRoutes (RouteTable.Routes);
		}

		/// <summary>
		/// Applications the post authorize request.
		/// </summary>
		protected void Application_PostAuthorizeRequest()
		{
			if (IsWebApiRequest())
			{
				HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
			}
		}

		private bool IsWebApiRequest()
		{
			return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(WebApiConfig.UrlPrefixRelative);
		}

	}
}
