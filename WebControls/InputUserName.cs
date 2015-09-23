//
//  SelectUserControl.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.Security;


namespace Yavsc.WebControls
{
	/// <summary>
	/// Select user control.
	/// </summary>
	[
		AspNetHostingPermission (SecurityAction.Demand,
			Level = AspNetHostingPermissionLevel.Minimal),
		AspNetHostingPermission (SecurityAction.InheritanceDemand, 
			Level = AspNetHostingPermissionLevel.Minimal),
		ParseChildren (true),
		DefaultProperty ("Name"),
		ToolboxData ("<{0}:InputUserName runat=\"server\"> </{0}:InputUserName>")
	]
	public class InputUserName: WebControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebControls.InputUserName"/> class.
		/// </summary>
		public InputUserName ()
		{
			Multiple = false;
			EmptyValue = null;
		}
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[Bindable (true)]
		[DefaultValue("")]
		[Localizable(true)]
		public string Name {
			get {
				return (string) ViewState["Name"];
			}
			set {
				ViewState ["Name"]  = value;
			}
		}
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[Bindable (true),DefaultValue(""),Localizable(true)]
		public string Value {
			get {
				return (string) ViewState["Value"];
			}
			set {
				ViewState ["Value"]  = value;
			}
		}

		/// <summary>
		/// Gets or sets the client side action on change.
		/// </summary>
		/// <value>The on change.</value>
		[Bindable (true),DefaultValue(""),Localizable(false)]
		public string OnChange {
			get {
				return (string) ViewState["OnChange"];
			}
			set {
				ViewState ["OnChange"]  = value;
			}
		}


		/// <summary>
		/// Gets or sets the in role.
		/// </summary>
		/// <value>The in role.</value>
		[Bindable (true),DefaultValue(""),Localizable(true)]
		public string InRole {
			get {
				return (string) ViewState["InRole"];
			}
			set {
				ViewState ["InRole"]  = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.WebControls.InputUserName"/> is multiple.
		/// </summary>
		/// <value><c>true</c> if multiple; otherwise, <c>false</c>.</value>
		[Bindable (true), DefaultValue(false)]
		public bool Multiple {
			get {

				return (bool) ViewState["Multiple"];
			}
			set {
				ViewState ["Multiple"]  = value;

			}
		}

		/// <summary>
		/// Gets or sets the empty value.
		/// </summary>
		/// <value>The empty value.</value>
		[Bindable (true), DefaultValue(null)]
		public string EmptyValue {
			get {
				return (string) ViewState["EmptyValue"];
			}
			set {
				ViewState ["EmptyValue"]  = value;

			}
		}



		/// <summary>
		/// Renders the contents.
		/// </summary>
		/// <param name="writer">Writer.</param>
		protected override void RenderContents (HtmlTextWriter writer)
		{
			writer.AddAttribute ("id", ID);
			writer.AddAttribute ("name", Name);
			writer.AddAttribute ("class", CssClass);
			if (!string.IsNullOrWhiteSpace(OnChange))
				writer.AddAttribute ("onchange", OnChange);
			if (Multiple)
				writer.AddAttribute ("multiple","true");
			writer.RenderBeginTag ("select");
			string[] selected = null;
			string[] roles = null;
			if (!string.IsNullOrWhiteSpace (Value)) {
				selected = Value.Split (',');
			}
			if (!string.IsNullOrWhiteSpace (InRole)) {
				roles = InRole.Split (',');
			}
			if (EmptyValue!=null) {
				writer.AddAttribute ("value", "");
				writer.RenderBeginTag ("option");
				writer.Write (EmptyValue);
				writer.RenderEndTag ();
			}
			foreach (MembershipUser u in Membership.GetAllUsers()) {
				// if roles are specified, members must be in one of them
				if (roles != null)
					if (!Array.Exists (roles, x => Roles.IsUserInRole (x)))
						continue;
				if (selected!=null)
				if (Array.Exists(selected, x=> x == u.UserName))
						writer.AddAttribute ("selected",null);
				writer.RenderBeginTag ("option");
				writer.Write (u.UserName);
				writer.RenderEndTag ();
			}
			writer.RenderEndTag ();
		}
	}
}

