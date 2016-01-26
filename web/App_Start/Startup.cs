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
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Yavsc.Formatters;
using Yavsc.Model;


[assembly: OwinStartup(typeof(Yavsc.App_Start.Startup))]
namespace Yavsc.App_Start
{
	public partial class Startup 
	{
		public void Configuration(IAppBuilder app)
		{
			ModelBinders.Binders.Add(typeof(double), new DoubleModelBinder());
			AjaxHelper.GlobalizationScriptPath = "~/Scripts/globalize";
			AreaRegistration.RegisterAllAreas();

			ConfigureSecurity (app);
			ConfigureAuth(app);
			//ConfigureChat (app);
			ConfigureWebApi(app);

			RegisterRoutes(RouteTable.Routes);
			// FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			/* RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles); */
		}

		public void ConfigureSecurity(IAppBuilder app)
		{
			app.SetDataProtectionProvider(new MonoDataProtectionProvider
				(app.Properties["host.AppName"] as string));
			
		}
	}
}

