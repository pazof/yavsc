using System.Web;
using System.Web.Mvc;
using MarkdownDeep;
using System.Text.RegularExpressions;

namespace Yavsc.Model.Blogs
{
	/// <summary>
	/// Helper class for transforming Markdown.
	/// </summary>
	public static partial class MarkdownHelper
	{
		public static Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);
		public static string FormatCodePrettyPrint(MarkdownDeep.Markdown m, string code)
		{
			// Try to extract the language from the first line
			var match = rxExtractLanguage.Match(code);
			string language = null;

			if (match.Success)
			{
				// Save the language
				var g = (Group)match.Groups[2];
				language = g.ToString();

				// Remove the first line
				code = code.Substring(match.Groups[1].Length);
			}

			// If not specified, look for a link definition called "default_syntax" and
			// grab the language from its title
			if (language == null)
			{
				var d = m.GetLinkDefinition("default_syntax");
				if (d != null)
					language = d.title;
			}

			// Common replacements
			if (language == "C#")
				language = "csharp";
			if (language == "C++")
				language = "cpp";

			// Wrap code in pre/code tags and add PrettyPrint attributes if necessary
			if (string.IsNullOrEmpty(language))
				return string.Format("<pre><code>{0}</code></pre>\n", code);
			else
				return string.Format("<pre class=\"prettyprint lang-{0}\"><code>{1}</code></pre>\n", 
					language.ToLowerInvariant(), code);
		}

		/// <summary>
		/// Transforms a string of Markdown into HTML.
		/// </summary>
		/// <param name="text">The Markdown that should be transformed.</param>
		/// <returns>The HTML representation of the supplied Markdown.</returns>
		public static IHtmlString Markdown(string text)
		{
			// Transform the supplied text (Markdown) into HTML.
			var markdownTransformer = GetMarkdownTransformer();
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
			var markdownTransformer=GetMarkdownTransformer ();
			markdownTransformer.UrlBaseLocation = urlBaseLocation;
			string html = markdownTransformer.Transform(text);
			// Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
			return new MvcHtmlString(html);
		}

		/// <summary>
		/// Markdowns the intro.
		/// </summary>
		/// <returns>The intro.</returns>
		/// <param name="markdown">Markdown.</param>
		/// <param name="truncated">Truncated.</param>
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

		internal static Markdown GetMarkdownTransformer()
		{
			var markdownTransformer = new Markdown();
			markdownTransformer.ExtraMode = true;
			markdownTransformer.NoFollowLinks = true;
			markdownTransformer.SafeMode = false;
			markdownTransformer.FormatCodeBlock = FormatCodePrettyPrint;
			markdownTransformer.ExtractHeadBlocks = true;
			markdownTransformer.UserBreaks = true;
			return markdownTransformer;
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
			var markdownTransformer=GetMarkdownTransformer ();
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
