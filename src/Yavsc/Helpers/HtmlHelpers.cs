using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Yavsc.Models.Drawing;
using AsciiDocNet;
using Yavsc.Models.Blog;
using System.Linq.Expressions;


public static class AsciiDocHelpers
        {
            public static IHtmlContent AsciiDocFor<TModel> (this IHtmlHelper<TModel>  html, 
            Expression<Func<TModel, string>> expression)
            {
                var span = new TagBuilder("p"){ TagRenderMode = TagRenderMode.SelfClosing };
                span.InnerHtml.Append (
                html.ValueFor<string>(expression, "{0}"));
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
            }
        }



namespace Yavsc.Helpers
{
    
        
    public static class HtmlHelpers
    {
        public static HtmlString Color(this Color c)
        {
            if (c == null) return new HtmlString("#000");
            return new HtmlString(String.Format("#{0:X2}{1:X2}{2:X2}", c.Red, c.Green, c.Blue));
        }
        public static string ToAbsolute(this HttpRequest request, string url)
        {
            var host = request.Host;
            var isSecure = request.Headers[Constants.SshHeaderKey] == "on";
            return (isSecure ? "https" : "http") + $"://{host}/{url}";
        }

    }
}
