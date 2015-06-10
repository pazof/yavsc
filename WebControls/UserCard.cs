//
//  UserCard.cs
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
using System.Web.UI.WebControls;
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.ComponentModel;
using System.Web.Security;

namespace WebControls
{
	[
		AspNetHostingPermission (SecurityAction.Demand,
			Level = AspNetHostingPermissionLevel.Minimal),
		AspNetHostingPermission (SecurityAction.InheritanceDemand, 
			Level = AspNetHostingPermissionLevel.Minimal),
		ParseChildren (true),
		DefaultProperty ("Name"),
		ToolboxData ("<{0}:UserCard runat=\"server\"> </{0}:UserCard>")
	]
	/// <summary>
	/// User card.
	/// </summary>
	public class UserCard: WebControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebControls.UserCard"/> class.
		/// </summary>
		public UserCard ()
		{
		}

		[Bindable (true), DefaultValue(""), Localizable(false)]
		string UserName { get; set; }

		[Bindable (true), DefaultValue("(<a href=\"/Account/Profile\">You</a>)"), Localizable(true)]
		string yourTag { get; set; }
		/// <summary>
		/// Renders the contents.
		/// </summary>
		/// <param name="writer">Writer.</param>
		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (UserName != null) {
				// icon, stats

				writer.AddAttribute ("id", ID);
				writer.AddAttribute ("class", CssClass);
				writer.RenderBeginTag ("div");
				writer.Write (UserName+" ");
				var vuser = Membership.GetUser();
				if (vuser != null) 
					if (vuser.UserName == UserName) 
						writer.Write (yourTag);
				writer.RenderEndTag ();
			}
		}


	}
}

