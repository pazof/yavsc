
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Extensions.WebEncoders;


namespace Yavsc.Formatters {
    public class PdfFormatter : OutputFormatter
    {
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {

throw new NotImplementedException();

        }
    }

    public class TexEncoder : IHtmlEncoder
    {
        public string HtmlEncode(string value)
        {
            return value;
        }

        public void HtmlEncode(string value, int startIndex, int charCount, TextWriter output)
        {
            output.Write(value.Substring(startIndex,charCount));
        }

        public void HtmlEncode(char[] value, int startIndex, int charCount, TextWriter output)
        {
            output.Write(value,startIndex,charCount);
        }
    }

}