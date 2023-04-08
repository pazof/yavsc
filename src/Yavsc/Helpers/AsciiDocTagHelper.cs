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
            string text = content.GetContent();
            if (string.IsNullOrWhiteSpace(text)) return;
            Document document = Document.Parse(text);
            var html = document.ToHtml(4);
            using var stringWriter = new StringWriter();
            html.WriteTo(stringWriter, HtmlEncoder.Default);
            output.Content.AppendHtml(stringWriter.ToString());
        }
    }
}
