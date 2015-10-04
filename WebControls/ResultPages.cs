using System;
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

namespace Yavsc.WebControls
{
	/// <summary>
	/// Result pages.
	/// </summary>
	[
		AspNetHostingPermission (SecurityAction.Demand,
		Level = AspNetHostingPermissionLevel.Minimal),
		AspNetHostingPermission (SecurityAction.InheritanceDemand, 
		Level = AspNetHostingPermissionLevel.Minimal),
		ParseChildren (true),

		ToolboxData ("<{0}:ResultPages runat=\"server\"> </{0}:ResultPages>")
	]
	public class ResultPages: WebControl
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.WebControls.ResultPages"/> class.
		/// </summary>
		public ResultPages ()
		{
			
		}

		/// <summary>
		/// Gets or sets the results per page.
		/// </summary>
		/// <value>The results per page.</value>
		public int PageSize {
			get {
				return (int)( ViewState["PageSize"]==null?10:ViewState["PageSize"]);
			}
			set {
				ViewState["PageSize"]=value;
			}
		}


		/// <summary>
		/// Gets or sets the result count.
		/// </summary>
		/// <value>The result count.</value>
		public int ResultCount {
			get {
				return (int)( ViewState["ResultCount"]==null?0:ViewState["ResultCount"]);
			}
			set {
				ViewState["ResultCount"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		[DefaultValue("Pages:")]
		[Localizable(true)]
		public string Text {
			get {

				string s = (string)ViewState["Text"];
				return (s == null) ? "Pages:" : s;
			}
			set {
				ViewState["Text"]  = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the action.
		/// </summary>
		/// <value>The action.</value>
		[Bindable (true)]
		[DefaultValue("?pageIndex=")]
		public string Action {
			get {

				string s = (string)ViewState["Action"];
				return (s == null) ? "?pageIndex=" : s;
			}
			set {
				ViewState["Action"]  = value;
			}
		}

		/// <summary>
		/// Gets or sets the none.
		/// </summary>
		/// <value>The none.</value>
		[Bindable (true)]
		[DefaultValue("none")]
		public string None {
			get {
				return (string) ViewState["None"];
			}
			set {
				ViewState["None"]  = value;
			}
		}

		/// <summary>
		/// Gets or sets the current page.
		/// </summary>
		/// <value>The current page.</value>
		[Bindable (true)]
		[DefaultValue(0)]
		public int PageIndex {
			get {
				int i = (int)(ViewState["PageIndex"]==null?0:ViewState["PageIndex"]);
				return i;
			}
			set {
				ViewState["PageIndex"]  = value;
			}
		}

		/// <summary>
		/// Renders the contents as the list of links to pages of results.
		/// </summary>
		/// <param name="writer">Writer.</param>
		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (ResultCount > 0 &&  ResultCount > PageSize ) {
				writer.WriteEncodedText (Text);
				int pageCount = ((ResultCount-1) / PageSize) + 1;
				if ( pageCount > 1 ) {
					for (int pi = (PageIndex < 5) ? 0 : PageIndex - 5; pi < pageCount && pi < PageIndex + 5; pi++) {
						if (PageIndex == pi)
							writer.RenderBeginTag ("b");
						else {
							writer.AddAttribute (HtmlTextWriterAttribute.Href,
								string.Format (Action, pi));
							writer.RenderBeginTag ("a");
						}
						writer.Write (pi + 1);
						writer.RenderEndTag ();
						writer.Write ("&nbsp;");
					}
				}
			} 
			if (ResultCount == 0) {
				writer.Write (None);
			}

		}
	}
}


