
using Microsoft.AspNetCore.Html;
using AsciiDocNet;

namespace Yavsc.Helpers
{
    public static class AsciiDocHelpers
    {
        static void ToHtml(this IElement elt, IHtmlContentBuilder contentbuilder)
        {
            switch (elt.GetType().FullName)
            {
                case "AsciiDocNet.Paragraph":
                    Paragraph p = (Paragraph)elt;
                    contentbuilder.AppendHtmlLine("<p>");
                    foreach (var pitem in p)
                    {
                        pitem.ToHtml(contentbuilder);
                    }
                    contentbuilder.AppendHtmlLine("</p>");
                    break;
                case "AsciiDocNet.SectionTitle":
                    SectionTitle stitle = (SectionTitle)elt;

                    contentbuilder.AppendHtmlLine($"<h{stitle.Level}>");
                    foreach (var titem in stitle)
                    {
                        titem.ToHtml(contentbuilder);
                    }
                    contentbuilder.AppendHtmlLine("</h>");
                    break;
                case "AsciiDocNet.UnorderedList":
                    UnorderedList ul = (UnorderedList)elt;
                    contentbuilder.AppendHtmlLine("<ul>");
                    foreach (var li in ul.Items)
                    {
                        contentbuilder.AppendHtmlLine("<li>");

                        foreach (var lii in li)
                        {
                            lii.ToHtml(contentbuilder);
                        }
                        contentbuilder.AppendHtmlLine("</li>");
                    }
                    contentbuilder.AppendHtmlLine("</ul>");
                    break;
                case "AsciiDocNet.Source":
                    Source source = (Source)elt;
                    // TODO syntact hilighting and fun js modules
                    contentbuilder.AppendHtmlLine("<pre><code>");
                    contentbuilder.AppendHtml(source.Text);
                    contentbuilder.AppendHtmlLine("</code></pre>");
                    break;
                default:
                    string unsupportedType = elt.GetType().FullName;
                    throw new InvalidProgramException(unsupportedType);
            }
        }

        public static string GetValidHRef(this Link link)
        {
            if (link.Href.StartsWith("link:\\"))
                return link.Href.Substring(7);
            if (link.Href.StartsWith("link:"))
                return link.Href.Substring(5);
            return link.Href;
        }

        static void ToHtml(this IInlineElement elt, IHtmlContentBuilder sb)
        {
            switch (elt.GetType().FullName)
            {
                case "AsciiDocNet.Monospace":
                    sb.AppendHtml("<code>");
                    Monospace mono = (Monospace)elt;
                    AllItemsToHtml(sb, mono);
                    break;
                case "AsciiDocNet.Link":
                    Link link = (Link)elt;
                    Uri uri;
                    if (Uri.TryCreate(link.Href,
                    UriKind.RelativeOrAbsolute
                    , out uri))
                    {
                        if (string.IsNullOrEmpty(link.Text))
                        {
                            link.Text = $"{uri.Host}({uri.LocalPath})";
                        }
                    }
                    sb.AppendFormat("<a href=\"{0}\">{1}</a> ", link.GetValidHRef(), link.Text);
                    break;

                case "AsciiDocNet.TextLiteral":
                    RenderLitteral(elt, sb);
                    break;

                case "AsciiDocNet.Emphasis":
                    sb.AppendHtml("<i>");
                    AsciiDocNet.Emphasis em = (Emphasis)elt;
                    sb.AppendHtml(em.Text);
                    sb.AppendHtml("</i>");
                    break;

                case "AsciiDocNet.Strong":
                    sb.AppendHtml("<b>");
                    Strong str = (Strong)elt;
                    AllItemsToHtml(sb, str);
                    sb.AppendHtml("</b>");
                    break;
                case "AsciiDocNet.InternalAnchor":
                    InternalAnchor a = (InternalAnchor)elt;
                    sb.AppendFormat("<a name=\"{0}\">{1}</a> ", a.Id, a.XRefLabel);
                    break;
                case "AsciiDocNet.Subscript":
                    sb.AppendHtml("<sup>");
                    Subscript sub = (Subscript)elt;
                    RenderLitteral(sub, sb);
                    sb.AppendHtml("</sup>");
                    break;
                case "AsciiDocNet.Superscript":
                    sb.AppendHtml("<sup>");
                    Superscript sup = (Superscript)elt;
                    RenderLitteral(sup, sb);
                    sb.AppendHtml("</sup>");
                    break;
                case "AsciiDocNet.Mark":
                    sb.AppendHtml("<em>");

                    Mark mark = (Mark)elt;
                    if (mark.DoubleDelimited)
                    {
                        sb.AppendHtml("<b>");
                        RenderLitteral(mark, sb);
                        sb.AppendHtml("</b>");
                    }
                    else
                        RenderLitteral(mark, sb);
                    sb.AppendHtml("</em>");
                    break;

                default:
                    string unsupportedType = elt.GetType().FullName;
                    if (elt is InlineContainer inlineContainer)
                    {
                        sb.AppendHtml($"<span title=\"E NOT SUPPORTED : {unsupportedType}\">");
                        AllItemsToHtml(sb, inlineContainer);
                        sb.AppendHtml("</span>");
                    }
                    else if (elt is IInlineElement inlineElement)
                    {
                        sb.AppendHtml($"<span title=\"E NOT SUPPORTED : {unsupportedType}\">");
                        RenderLitteral(inlineElement, sb);
                        sb.AppendHtml("</span>");
                    }
                    else
                    {
                        throw new InvalidProgramException(unsupportedType);
                    }
                    break;
            }
        }

        private static void AllItemsToHtml(IHtmlContentBuilder sb, InlineContainer mono)
        {
            foreach (var item in mono)
            {
                item.ToHtml(sb);
            }
            sb.AppendHtml("</code>");
        }

        private static void RenderLitteral(IInlineElement elt, IHtmlContentBuilder sb)
        {
            var tl = elt as TextLiteral;
            if (tl?.Attributes.Anchor != null)
            {
                sb.AppendHtmlLine($"<a name=\"{tl.Attributes.Anchor.Id}\">{tl.Attributes.Anchor.XRefLabel}</a> ");
            }
            if (tl != null) sb.AppendHtml(tl.Text);
        }

        public static IHtmlContent ToHtml(this Document doc, int doclevel = 4)
        {
            var contentbuilder = new HtmlContentBuilder();
            if (doc.Title != null)
            {
                if (!string.IsNullOrWhiteSpace(doc.Title.Title))
                {
                    contentbuilder.AppendHtmlLine($"<h{doclevel}>{doc.Title.Title}</h{doclevel}>");
                    if (!string.IsNullOrWhiteSpace(doc.Title.Subtitle))
                    {
                        contentbuilder.AppendHtmlLine($"<i>{doc.Title.Title}</i><br/>");
                    }
                }
            }
            foreach (var item in doc)
            {
                item.ToHtml(contentbuilder);
            }
            return contentbuilder;
        }

        /*
                public static IHtmlContent AsciiDocFor<TModel>(this IHtmlHelper<TModel> html,
                Expression<Func<TModel, string>> expression)
                {
                    string ascii = html.ValueFor<string>(expression, "{0}");
                    if (string.IsNullOrWhiteSpace(ascii))
                        return new HtmlString(string.Empty);
                    Document document = Document.Parse(ascii);
                    var htmlDoc = document.ToHtml();
                    var span = new TagBuilder("p") { TagRenderMode = TagRenderMode.SelfClosing };
                    span.InnerHtml.AppendHtml(htmlDoc);
                    return span.RenderBody();
                }

                public static string AsciiDoc(IHtmlHelper<BlogPost> htmlHelper, string text)
                {
                    return AsciiDoc(htmlHelper, text, null);
                }

                private static string AsciiDoc(IHtmlHelper<BlogPost> htmlHelper, string text, object htmlAttributes)
                {
                    // Create tag builder
                    var builder = new TagBuilder("div");
                    var document = Document.Parse(text);

                    // builder.InnerHtml = .

                    // Add attributes
                    builder.MergeAttribute("class", "ascii");
                    builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

                    // Render tag
                    return builder.ToString();
                } */
    }
}
