//
//  Startup.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
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
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.DataProtection;
using Yavsc.Model;
using System.Web.Mvc;
using Yavsc.Formatters;
using System.Web.Http;
using System.Web.Routing;


[assembly: OwinStartup(typeof(Yavsc.App_Start.Startup))]
namespace Yavsc.App_Start
{
	public partial class Startup 
	{
		public void Configuration(IAppBuilder app)
		{
			AjaxHelper.GlobalizationScriptPath = "~/Scripts/globalize";
			AreaRegistration.RegisterAllAreas();

			ModelBinders.Binders.Add(typeof(double), new DoubleModelBinder());
			ConfigureSecurity (app);
			ConfigureAuth(app);
			GlobalConfiguration.Configure (WebApiConfig.Register);
			//ConfigureChat (app);
			RegisterRoutes(RouteTable.Routes);
			/*  FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles); */
		}

		public void ConfigureSecurity(IAppBuilder app)
		{
			app.SetDataProtectionProvider(new MonoDataProtectionProvider(app.Properties["host.AppName"] as string));
		}

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
			routes.IgnoreRoute ("signalr/{*pathInfo}"); // fonts 

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

