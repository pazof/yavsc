using System.Text.Encodings.Web;
using AsciiDocNet;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yavsc.Helpers
{
    public class AsciidocTagHelper : TagHelper
    {
        public override async Task ProcessAsync (TagHelperContext context, TagHelperOutput output)
        {
            var content = await output.GetChildContentAsync();
       
            Document document = Document.Parse(content.GetContent());
            var html = document.ToHtml(4);
            using var stringWriter = new StringWriter();
            html.WriteTo(stringWriter, HtmlEncoder.Default);
            output.Content.AppendHtml(stringWriter.ToString());
        }
    }
}
