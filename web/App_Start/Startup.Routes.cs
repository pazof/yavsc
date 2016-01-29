using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Configuration;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Yavsc.Helpers;
using Yavsc.Model;
using Yavsc.Providers;
using System.Web.Routing;
using System.Web.Mvc;

namespace Yavsc.App_Start
{
	/// <summary>
	/// Startup.
	/// </summary>
	public partial class Startup
	{
		/// <summary>
		/// Registers the routes.
		/// </summary>
		/// <param name="routes">Routes.</param>
		public static void RegisterRoutes (RouteCollection routes)
		{
			
			routes.IgnoreRoute ("{resource}.axd/{*pathInfo}"); // not used
			routes.IgnoreRoute ("Scripts/{*pathInfo}"); // web user side scripts
			routes.IgnoreRoute ("App_Theme/{*pathInfo}"); // sites themes
			routes.IgnoreRoute ("App_Data/{*pathInfo}"); // sites themes
			routes.IgnoreRoute ("App_Code/{*pathInfo}"); // sites themes
			routes.IgnoreRoute ("users/{*pathInfo}"); // user's files
			routes.IgnoreRoute ("avatars/{*pathInfo}"); // user's avatar
			routes.IgnoreRoute ("bfiles/{*pathInfo}"); // Blog files
			routes.IgnoreRoute ("xmldoc/{*pathInfo}"); // xml doc 
			routes.IgnoreRoute ("htmldoc/{*pathInfo}"); // html doc 
			routes.IgnoreRoute ("fonts/{*pathInfo}"); // fonts 
			routes.IgnoreRoute ("favicon.ico"); // favorite icon
			routes.IgnoreRoute ("favicon.png"); // favorite icon
			routes.IgnoreRoute ("robots.txt");  // for search engine robots


			/*
			 * 
routes.MapRoute(
				name: "Google API Sign-in",
				url: "signin-google",
				defaults: new { controller = "OAuth", action = "ExternalCallbackRedirect" }
			);
			routes.MapRoute(
				name: "Facebook API Sign-in",
				url: "signin-facebook",
				defaults: new { controller = "OAuth", action = "ExternalCallback" }
			); 
			*/

			routes.MapRoute (
				"FrontOffice",
				"do/{action}/{MEACode}/{Id}",
				new { controller = "FrontOffice", action = "Index", MEACode = UrlParameter.Optional, Id = UrlParameter.Optional }
			);

			routes.MapRoute (
				"Titles",
				"title/{title}",
				new { controller = "Blogs", action = "Index", 
					title=UrlParameter.Optional }
			);
			routes.MapRoute (
				"Blogs",
				"blog/{user}",
				new { controller = "Blogs", 
					action = "UserPosts", 
					user= UrlParameter.Optional }
			); 

			routes.MapRoute (
				"BlogByTitle",
				"by/{user}/{title}/{id}",
				new { controller = "Blogs", 
					action = "UserPosts", 
					user="Paul Schneider",
					title=UrlParameter.Optional,
					id=UrlParameter.Optional }
			);

			routes.MapRoute (
				"BlogById",
				"b/{action}/{postid}",
				new { controller = "Blogs", action = "Index", 
					postid = UrlParameter.Optional }
			); 

			routes.MapRoute (
				"BackCompat",
				"Blogs/{action}/{user}/{title}",
				new { controller = "Blogs", action = "Index", user = UrlParameter.Optional, title = UrlParameter.Optional }
			);

			routes.MapRoute (
				"Performance",
				"to/{action}/{Performer}",
				new { controller = "FrontOffice", action = "Contact", Performer = UrlParameter.Optional }
			);

			routes.MapRoute (
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

		}

	}
}

