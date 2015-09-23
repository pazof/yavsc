﻿using System.Web;
using System.Web.Mvc;
using MarkdownDeep;

namespace Yavsc.Helpers
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
