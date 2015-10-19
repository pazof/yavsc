//
//  BlogUnitTest.cs
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
using NUnit.Framework;
using System;
using Yavsc.Model.Blogs;
using Yavsc.Controllers;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.IO;
using System.Web.Http;
using Mono.WebServer;
using System.Net;
using System.Web.Hosting;
using Mono.Web.Util;
using Mono.WebServer.Options;

namespace Yavsc
{

	[TestFixture ()]
	public class ServerTestCase
	{

		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }

		AccountController accountController;

		public AccountController AccountController {
			get {
				return accountController;
			}
		}

		ApplicationServer WebAppServer;

		string defaultMembershipProvider = null;

		[Test]
		public virtual void Start()
		{
			// get the web config
			string physicalPath = @"/home/paul/workspace/totem/web/";
			string physicalPathToConfig =  physicalPath + "/Web.config";
			ExeConfigurationFileMap exemap = new ExeConfigurationFileMap ();
			exemap.ExeConfigFilename  = physicalPathToConfig ;
			Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration (exemap, ConfigurationUserLevel.None);


			string basedir = AppDomain.CurrentDomain.BaseDirectory;
			string curdir = Directory.GetCurrentDirectory ();
			string dummyVirtualPath = "/";
			int Port=8080;
			XSPWebSource websource=new XSPWebSource(IPAddress.Any,Port);
			WebAppServer=new ApplicationServer(websource,physicalPath);

			var broker = new XSPRequestBroker ();
			var host = new XSPApplicationHost ();

			host.RequestBroker = broker;
			host.Server = WebAppServer;
			broker.InitializeLifetimeService ();
			host.InitializeLifetimeService ();
			// ApplicationHost h = new XSPApplicationHost();

			//"[[hostname:]port:]VPath:realpath"
			string cmdLine=Port+":/:"+physicalPath;
			WebAppServer.AddApplicationsFromCommandLine (cmdLine);

			WebAppServer.Broker = broker;
			WebAppServer.AppHost = host;
			// WebAppServer.AddApplicationsFromConfigFile (physicalPath+"/Web.config");
//			WebConfigurationFileMap map = new WebConfigurationFileMap ();
//			map.VirtualDirectories.Add (dummyVirtualPath, new VirtualDirectoryMapping (physicalPath, true));
// TODO why not? Configuration configuration = WebConfigurationManager.OpenMappedWebConfiguration (map, dummyVirtualPath);

//			string da = (string)config.AppSettings.Settings ["DefaultAvatar"].Value;
//			MembershipSection s = config.GetSection ("system.web/membership") as MembershipSection;
//			defaultMembershipProvider = s.DefaultProvider;
			// ??? WebConfigurationManager.ConfigPath
			Configuration cfg = WebConfigurationManager.OpenWebConfiguration (dummyVirtualPath);
//			WebConfigurationManager.AppSettings.Clear ();
//			WebConfigurationManager.ConnectionStrings.Clear ();
//			var mbrssect = WebConfigurationManager.GetWebApplicationSection ("system.web/membership") as MembershipSection;
//
//			mbrssect.Providers.Clear ();
			var syswebcfg = WebConfigurationManager.GetWebApplicationSection ("system.web") as ConfigurationSection;

			WebAppServer.Start (true,2000);
//			System.Threading.Thread.Sleep(30000);
		}

		[Test ()]
		public virtual void Register ()
		{
			accountController = new AccountController ();

			ViewResult actionResult = accountController.Register (
				new Yavsc.Model.RolesAndMembers.RegisterViewModel () {
					UserName = UserName, Email = Email,
					Password = "tpwd", ConfirmPassword = Password, 
					IsApprouved = true
				}, 
				"/testreturnurl") as ViewResult;
			Assert.AreSame ("",actionResult.ViewName);
			MembershipUser u = Membership.GetUser (UserName, false);
			Assert.NotNull (u);
			Assert.False (u.IsApproved);
			// TODO : check mail for test, 
			// get the validation key from its body,
			// and use the accountController.Validate(username,key)
			u.IsApproved = true;
			Membership.UpdateUser (u);
			Assert.True (u.IsApproved);
		}

		[Test()]
		public virtual void Stop() {
			WebAppServer.Stop();
		}

		public virtual void Unregister()
		{
			ViewResult actionResult  = 
				accountController.Unregister (UserName, true)  as ViewResult;
			Assert.AreEqual (actionResult.ViewName, "Index");
		}
	}
}

