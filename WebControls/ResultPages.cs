using System;
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

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
		ParseChildren (true, "Action"),
		DefaultProperty ("Action"),
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
		[Bindable (true)]
		[DefaultValue(10)]
		public int ResultsPerPage {
			get {
				return (int)( ViewState["ResultsPerPage"]==null?10:ViewState["ResultsPerPage"]);
			}
			set {
				ViewState["ResultsPerPage"]=value;
			}
		}


		/// <summary>
		/// Gets or sets the result count.
		/// </summary>
		/// <value>The result count.</value>
		[Bindable (true)]
		[DefaultValue(0)]
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
		[Bindable (true)]
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
		[DefaultValue("")]
		public string Action {
			get {

				string s = (string)ViewState["Action"];
				return (s == null) ? String.Empty : s;
			}
			set {
				ViewState["Action"]  = value;
			}
		}


		/// <summary>
		/// Gets or sets the current page.
		/// </summary>
		/// <value>The current page.</value>
		[Bindable (true)]
		[DefaultValue(0)]
		public int CurrentPage {
			get {
				int i = (int)(ViewState["CurrentPage"]==null?0:ViewState["CurrentPage"]);
				return i;
			}
			set {
				ViewState["CurrentPage"]  = value;
			}
		}
		/// <summary>
		/// Renders the contents as the list of links to pages of results.
		/// </summary>
		/// <param name="writer">Writer.</param>
		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (ResultCount > 0 &&  ResultCount > ResultsPerPage ) {
				writer.WriteEncodedText (Text);
				int pageCount = ((ResultCount-1) / ResultsPerPage) + 1;
				for (int pi = (CurrentPage < 5) ? 0 : CurrentPage - 5; pi < pageCount && pi < CurrentPage + 5; pi++) {
					if (CurrentPage == pi)
						writer.RenderBeginTag ("b");
					else {
						writer.AddAttribute (HtmlTextWriterAttribute.Href,
							string.Format (Action, pi));
						writer.RenderBeginTag ("a");
					}
					writer.Write (pi+1);
					writer.RenderEndTag ();
					writer.Write ("&nbsp;");
				}
			} 
			writer.Write ("(");
			if (ResultCount == 0) {
				writer.Write ("Pas de resultat");
			} else {
				writer.Write (ResultCount.ToString () + " resultat");
				if (ResultCount>1) writer.Write("s");
			}
			writer.Write (")");

		}
	}
}


