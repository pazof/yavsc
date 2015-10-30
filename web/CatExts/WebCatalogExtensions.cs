using System;
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Yavsc.Model.FrontOffice;
using System.Web.Mvc.Html;
using Yavsc.Model.FrontOffice.Catalog;

namespace Yavsc.CatExts
{
	/// <summary>
	/// Web catalog extensions.
	/// </summary>
	public static class WebCatalogExtensions
	{
		/// <summary>
		/// Commands the form.
		/// </summary>
		/// <returns>The form.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="pos">Position.</param>
		/// <param name="atc">Atc.</param>
		public static string CommandForm(this HtmlHelper<PhysicalProduct> helper, Product pos,string atc="Add to backet") {
			StringBuilder sb = new StringBuilder ();
			sb.Append (helper.ValidationSummary ());
			TagBuilder ft = new TagBuilder ("form");
			ft.Attributes.Add("action","/FrontOffice/Command");
			ft.Attributes.Add("method","post");
			ft.Attributes.Add("enctype","multipart/form-data");
			TagBuilder fieldset = new TagBuilder ("fieldset");

			TagBuilder legend = new TagBuilder ("legend");
			legend.SetInnerText (pos.Name);
			TagBuilder para = new TagBuilder ("p");

			StringBuilder sbfc = new StringBuilder ();
			if (pos.CommandForm != null)
				foreach (FormElement e in pos.CommandForm.Items) {
					sbfc.Append (e.ToHtml ());
					sbfc.Append ("<br/>\n");
				}
			sbfc.Append (
				string.Format(
					"<input type=\"submit\" value=\"{0}\"/><br/>\n",
					atc
				));

			sbfc.Append (helper.Hidden ("ref", pos.Reference));
			para.InnerHtml = sbfc.ToString ();
			fieldset.InnerHtml = legend.ToString ()+"\n"+para.ToString ();
			ft.InnerHtml = fieldset.ToString ();
			sb.Append (ft.ToString ());
			return sb.ToString ();
		}
		/// <summary>
		/// Commands the form.
		/// </summary>
		/// <returns>The form.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="pos">Position.</param>
		/// <param name="atc">Atc.</param>
		public static string CommandForm(this HtmlHelper<Service> helper, Product pos,string atc="Add to backet") {
			StringBuilder sb = new StringBuilder ();
			sb.Append (helper.ValidationSummary ());
			TagBuilder ft = new TagBuilder ("form");
			ft.Attributes.Add("action","/FrontOffice/Command");
			ft.Attributes.Add("method","post");
			ft.Attributes.Add("enctype","multipart/form-data");
			TagBuilder fieldset = new TagBuilder ("fieldset");

			TagBuilder legend = new TagBuilder ("legend");
			legend.SetInnerText (pos.Name);
			TagBuilder para = new TagBuilder ("p");

			StringBuilder sbfc = new StringBuilder ();
			if (pos.CommandForm != null)
				foreach (FormElement e in pos.CommandForm.Items) {
					sbfc.Append (e.ToHtml ());
					sbfc.Append ("<br/>\n");
				}
			sbfc.Append (
				string.Format(
					"<input type=\"submit\" value=\"{0}\"/><br/>\n",atc));
			sbfc.Append (helper.Hidden ("ref", pos.Reference));
			para.InnerHtml = sbfc.ToString ();
			fieldset.InnerHtml = legend.ToString ()+"\n"+para.ToString ();
			ft.InnerHtml = fieldset.ToString ();
			sb.Append (ft.ToString ());
			return sb.ToString ();
		}
	}
}

