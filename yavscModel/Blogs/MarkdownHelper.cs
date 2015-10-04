using System.Web;
using System.Web.Mvc;
using MarkdownDeep;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Helper class for transforming Markdown.
	/// </summary>
	public static partial class MarkdownHelper
	{
		/// <summary>
		/// Transforms a string of Markdown into HTML.
		/// </summary>
		/// <param name="text">The Markdown that should be transformed.</param>
		/// <returns>The HTML representation of the supplied Markdown.</returns>
		public static IHtmlString Markdown(string text)
		{
			// Transform the supplied text (Markdown) into HTML.
			var markdownTransformer = new Markdown();
			markdownTransformer.ExtraMode = true;
			string html = markdownTransformer.Transform(text);

			// Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
			return new MvcHtmlString(html);
		}


		/// <summary>
		/// Transforms a string of Markdown into HTML.
		/// </summary>
		/// <param name="helper">HtmlHelper - Not used, but required to make this an extension method.</param>
		/// <param name="text">The Markdown that should be transformed.</param>
		/// <param name="urlBaseLocation">The url Base Location.</param>
		/// <returns>The HTML representation of the supplied Markdown.</returns>
		public static IHtmlString Markdown(this HtmlHelper helper, string text, string urlBaseLocation="")
		{
			// Transform the supplied text (Markdown) into HTML.
			var markdownTransformer = new Markdown();
			markdownTransformer.ExtraMode = true;
			markdownTransformer.UrlBaseLocation = urlBaseLocation;
			string html = markdownTransformer.Transform(text);
			// Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
			return new MvcHtmlString(html);
		}

		public static string MarkdownIntro(string markdown, out bool truncated) { 
			int maxLen = 250;
			if (markdown.Length < maxLen) {
				truncated = false;
				return markdown;
			}
			string intro = markdown.Remove (maxLen);
			truncated = true;
			int inl = intro.LastIndexOf ("\n");
			if (inl > 20)
				intro = intro.Remove (inl);
			intro += " ...";
			return intro;
		}
		/// <summary>
		/// Markdowns to html intro.
		/// </summary>
		/// <returns>The to html intro.</returns>
		/// <param name="truncated">Truncated.</param>
		/// <param name="text">Text.</param>
		/// <param name="urlBaseLocation">URL base location.</param>
		public static string MarkdownToHtmlIntro(out bool truncated, string text, string urlBaseLocation="") {
			var md = MarkdownIntro(text, out truncated);
			var markdownTransformer = new Markdown();
			markdownTransformer.ExtraMode = true;
			markdownTransformer.UrlBaseLocation = urlBaseLocation;
			string html = markdownTransformer.Transform(md);
			return html;
		}
		/// <summary>
		/// Markdowns to html intro.
		/// </summary>
		/// <returns>The to html intro.</returns>
		/// <param name="helper">Helper.</param>
		/// <param name="truncated">Truncated.</param>
		/// <param name="text">Text.</param>
		/// <param name="urlBaseLocation">URL base location.</param>
		public static IHtmlString MarkdownToHtmlIntro(this HtmlHelper helper, out bool truncated, string text, string urlBaseLocation="")
		{
			// Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
			return new MvcHtmlString(MarkdownToHtmlIntro (out truncated, text, urlBaseLocation));
		}

		/// <summary>
		/// Transforms a string of Markdown into HTML.
		/// </summary>
		/// <param name="helper">HtmlHelper - Not used, but required to make this an extension method.</param>
		/// <param name="text">The Markdown that should be transformed.</param>
		/// <returns>The HTML representation of the supplied Markdown.</returns>
		public static IHtmlString Markdown(this HtmlHelper helper, string text)
		{
			// Just call the other one, to avoid having two copies (we don't use the HtmlHelper).
			return Markdown(text);
		}
	}
}
