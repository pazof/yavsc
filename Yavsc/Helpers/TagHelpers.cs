

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarkdownDeep;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Yavsc.Helpers
{
    [HtmlTargetElement("div", Attributes = MarkdownContentAttributeName)]
    [HtmlTargetElement("h1", Attributes = MarkdownContentAttributeName)]
    [HtmlTargetElement("h2", Attributes = MarkdownContentAttributeName)]
    [HtmlTargetElement("h3", Attributes = MarkdownContentAttributeName)]
    [HtmlTargetElement("p", Attributes = "ismarkdown")]
    [HtmlTargetElement("div", Attributes = "ismarkdown")]
    [HtmlTargetElement("h1", Attributes = "ismarkdown")]
    [HtmlTargetElement("h2", Attributes = "ismarkdown")]
    [HtmlTargetElement("h3", Attributes = "ismarkdown")]
    [HtmlTargetElement("markdown")]
    [OutputElementHint("p")]
    public class MarkdownTagHelper : TagHelper
    {
        private const string MarkdownContentAttributeName = "markdown";
        private const string MarkdownMarkAttributeName = "ismarkdown";
		[HtmlAttributeName("site")]
        public SiteSettings Site { get; set; }
        [HtmlAttributeName("base")]
        public string Base { get; set; }

        [HtmlAttributeName(MarkdownContentAttributeName)]
        public string MarkdownContent { get; set; }

        static Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);
        private static string FormatCodePrettyPrint(MarkdownDeep.Markdown m, string code)
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
        /// <param name="urlBaseLocation">The url Base Location.</param>
        /// <returns>The HTML representation of the supplied Markdown.</returns>
        public string Markdown(string text, string urlBaseLocation = "")
        {
            // Transform the supplied text (Markdown) into HTML.
            var markdownTransformer = GetMarkdownTransformer();
            markdownTransformer.UrlBaseLocation = urlBaseLocation;
            string html = markdownTransformer.Transform(text);
            // Wrap the html in an MvcHtmlString otherwise it'll be HtmlEncoded and displayed to the user as HTML :(
            return html;
        }

        internal Markdown GetMarkdownTransformer()
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

        public ModelExpression Content { get; set; }
        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (output.TagName == "markdown")
            {
                output.TagName = null;
            }
            output.Attributes.RemoveAll("markdown");

            var content = await GetContent(output);
            var markdown = content;

            var htbase = Base;
            

            var html = Markdown(markdown, htbase);

            output.Content.SetHtmlContent(html ?? "");
        }
        private async Task<string> GetContent(TagHelperOutput output)
        {
            if (MarkdownContent != null)
                return MarkdownContent;
            if (Content != null)
                return Content.Model?.ToString();
            return (await output.GetChildContentAsync(false)).GetContent();
        }
    }

}
