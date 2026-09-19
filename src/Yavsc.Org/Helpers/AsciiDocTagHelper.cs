
using System.Web;
using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yavsc.Helpers
{
    public class MarkdownTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            //await base.ProcessAsync(context, output);
            var content = await output.GetChildContentAsync();
            string text = HttpUtility.HtmlDecode(content.GetContent());

            if (string.IsNullOrWhiteSpace(text)) return;


            try
            {
                if (context.AllAttributes.ContainsName("summary"))
                {
                    var summaryLength = context.AllAttributes["summary"].Value;
                    if (summaryLength is HtmlString sumLenStr)
                    {
                        if (int.TryParse(sumLenStr.Value, out var sumLen))
                        {
                            if (text.Length > sumLen)
                            {
                                text = text.Substring(0, sumLen) + "(...)";
                            }
                        }
                    }
                }
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var result = Markdown.ToHtml(text, pipeline);

                output.Content.AppendHtml(result);
            }
            catch (ArgumentException ex)
            {
                // silently render the text
                output.Content.AppendHtml("<pre>" + text + "</pre>\n");
                // and an error
                output.Content.AppendHtml("<pre class=\"parsingError\">" + ex.Message + "</pre>\n");
            }
        }
    }
}
