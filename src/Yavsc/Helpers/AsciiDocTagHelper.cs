using System.Text.Encodings.Web;
using AsciiDocNet;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yavsc.Helpers
{
    public class AsciidocTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
           
            await base.ProcessAsync(context, output);
            var content = await output.GetChildContentAsync();
            string text = content.GetContent();
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
                Document document = Document.Parse(text);
                var html = document.ToHtml(2);
                using var stringWriter = new StringWriter();
                html.WriteTo(stringWriter, HtmlEncoder.Default);
                var processedHere = stringWriter.ToString();
                output.Content.AppendHtml(processedHere);
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
