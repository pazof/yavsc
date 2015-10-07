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

namespace TestAPI
{
	[TestFixture ()]
	public class BlogUnitTest
	{

		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }


		AccountController accountController;
		[TestFixtureSetUp]
		public void Init()
		{
			accountController = new AccountController ();
		}

		[Test ()]
		public void Register ()
		{
			ViewResult actionResult = accountController.Register (
				new Yavsc.Model.RolesAndMembers.RegisterViewModel () {
					UserName = UserName, Email = Email,
					Password = "tpwd", ConfirmPassword = Password, 
					IsApprouved = true
				}, 
				"/testreturnurl") as ViewResult;
			Assert.AreEqual (actionResult.ViewName, "RegistrationPending");
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
		[TestFixtureTearDown()]
		public void Unregister()
		{
			ViewResult actionResult  = 
				accountController.Unregister (UserName, true)  as ViewResult;
			Assert.AreEqual (actionResult.ViewName, "Index");
		}
	}
}

