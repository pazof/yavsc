using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Yavsc.ApiControllers;
using Microsoft.AspNet.Identity;
using Yavsc.Model.RolesAndMembers;
using System.Net;
using Yavsc.Model.Identity;
using Yavsc.Helpers;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Security;
using Yavsc.Model.Google;
using Yavsc.Client.Accounts;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Controllers
{
	/// <summary>
	/// Sign in.
	/// </summary>
	public class SignIn: UserNameBase
	{
		/// <summary>
		/// Gets or sets the E mail.
		/// </summary>
		/// <value>The E mail.</value>
		[Required]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		/// <value>The display name.</value>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the avatar.
		/// </summary>
		/// <value>The avatar.</value>
		public string Avatar { get; set; }

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>The location.</value>
		public string Location { get; set; }

	}

}
