

using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;
using CommonMark;
using CommonMark.Syntax;
using System.IO;

namespace Yavsc.TagHelpers
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



        /// <summary>
        /// Transforms a string of Markdown into HTML.
        /// </summary>
        /// <param name="text">The Markdown that should be transformed.</param>
        /// <param name="urlBaseLocation">The url Base Location.</param>
        /// <returns>The HTML representation of the supplied Markdown.</returns>
        public string Markdown(string commonMark, string urlBaseLocation = "")
        {
            // Transform the supplied text (Markdown) into HTML.
            string actual;
            var settings = CommonMarkSettings.Default.Clone();
            settings.OutputFormat = OutputFormat.Html;
            settings.AdditionalFeatures |= CommonMarkAdditionalFeatures.StrikethroughTilde;

            Block document;

            // Act
            using (var reader = new StringReader(commonMark))
            using (var writer = new StringWriter())
            {
                var prologue = CommonMarkConverter.ProcessPrologue(reader, settings);
                document = CommonMarkConverter.ProcessStage1(reader, settings, prologue);
                CommonMarkConverter.ProcessStage2(document, settings);
                CommonMarkConverter.ProcessStage3(document, writer, settings);
                actual = writer.ToString();
            }
            return actual;
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
