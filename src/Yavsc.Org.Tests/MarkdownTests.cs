using DotMarkdown.Linq;
using Markdig;
using Xunit;
using static DotMarkdown.Linq.MFactory;


public class MarkdownTests
{
    [Fact]
    public void TestMarkdownRendering()
    {
        MDocument document = Document(
            Heading1("Markdown Sample"),
            Heading2("Bullet List"),
            BulletList(
                "text",
                Bold("bold text")),
            HorizontalRule(),
            Heading2("IndentedCodeBlock"),
            IndentedCodeBlock("string s = null;"));

        Console.WriteLine(document.ToString());

    }

    [Fact]
    public void TestAnotherMarkdownRendering()
    {
       var result = Markdown.ToHtml("This is a text with some *emphasis*");


        Console.WriteLine(result);
    }
}
