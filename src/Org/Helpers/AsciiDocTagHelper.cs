
using System.Web;
using AsciiDocSharp;
using AsciiDocSharp.Converters.Html;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yavsc.Helpers
{
    public class AsciidocTagHelper : TagHelper
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
                var processor = new AsciiDocProcessor(
                    
                );
                var htmlConverter = new HtmlDocumentConverter();

                var document = processor.ParseFromText(text);
                var htmlResult = processor.ConvertDocument(document, htmlConverter);


                output.Content.AppendHtml(htmlResult);
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
