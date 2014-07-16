using System;
using System.Web;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Yavsc.WebControls
{
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
		public ResultPages ()  
		{
		}


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

		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (ResultCount > 0) {
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
					writer.Write (pi);
					writer.RenderEndTag ();
					writer.Write ("&nbsp;");
				}
				writer.Write ("("+ResultCount.ToString()+" resultat(s))");
			} else {
				writer.Write ("(Pas de resultat)");
			}
		}
	}
}


