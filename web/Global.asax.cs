
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Yavsc.Formatters;
using Yavsc.Model.FrontOffice;
using System.Web.SessionState;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.WebPages.Scope;
using System.Reflection;
using System.Web.Configuration;

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
			// Should be FrontOffice in a POS,
			string defaultController = 
				WebConfigurationManager.AppSettings ["DefaultController"]; 
			if (defaultController == null)
				defaultController = "Home";
			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}"); // not used
			routes.IgnoreRoute ("Scripts/{*pathInfo}"); // web user side scripts
			routes.IgnoreRoute ("App_Theme/{*pathInfo}"); // sites themes
			routes.IgnoreRoute ("images/{*pathInfo}"); // site images
			routes.IgnoreRoute ("users/{*pathInfo}"); // user's files
			routes.IgnoreRoute ("avatars/{*pathInfo}"); // user's avatar
			routes.IgnoreRoute ("bfiles/{*pathInfo}"); // Blog files
			routes.IgnoreRoute ("xmldoc/{*pathInfo}"); // xml doc 
			routes.IgnoreRoute ("htmldoc/{*pathInfo}"); // html doc 
			routes.IgnoreRoute ("favicon.ico"); // favorite icon
			routes.IgnoreRoute ("favicon.png"); // favorite icon
			routes.IgnoreRoute ("robots.txt");  // for search engine robots
			routes.MapRoute (
				"Blogs",
				"Blogs/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			); 
			routes.MapRoute (
				"BlogByTitleRO",
				"Blog/{user}/{title}",
				new { controller = "Blogs", action = "Index", user=UrlParameter.Optional, title = UrlParameter.Optional }
			); 
			routes.MapRoute (
				"BlogById",
				"B/{action}/{id}",
				new { controller = "Blogs", action = "UserPost", id = UrlParameter.Optional }
			); 
			routes.MapRoute (
				"Default",
				"{controller}/{action}/{id}",
				new { controller = defaultController, 
					action = "Index", 
					user=UrlParameter.Optional,
					id = UrlParameter.Optional }
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

		/// <summary>
		/// begins a request against this application.
		/// </summary>
		protected void Application_BeginRequest()
		{
			var ob = typeof(
				AspNetRequestScopeStorageProvider).Assembly.GetType(
					"System.Web.WebPages.WebPageHttpModule").GetProperty
				("AppStartExecuteCompleted", 
					BindingFlags.NonPublic | BindingFlags.Static);
			ob.SetValue(null, true, null);
			
		}
	}
}

