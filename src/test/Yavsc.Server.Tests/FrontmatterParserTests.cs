using Yavsc.Server.Services;

namespace Yavsc.Server.Tests;

public class FrontmatterParserTests
{
    [Fact]
    public void Parse_extracts_formulaire_devis_and_body_for_unix_line_endings()
    {
        const string markdown = "---\nformulaire: Prestation Musicale\ndevis: Arrangement Orchestral\n---\n\nContenu du devis\n";

        var result = FrontmatterParser.Parse(markdown);

        Assert.Equal("Prestation Musicale", result.Formulaire);
        Assert.Equal("Arrangement Orchestral", result.Devis);
        Assert.Equal("Contenu du devis", result.Corps.Trim());
    }

    [Fact]
    public void Parse_accepts_windows_line_endings()
    {
        const string markdown = "---\r\nformulaire: Prestation Musicale\r\ndevis: Arrangement Orchestral\r\n---\r\n\r\nContenu du devis\r\n";

        var result = FrontmatterParser.Parse(markdown);

        Assert.Equal("Prestation Musicale", result.Formulaire);
        Assert.Equal("Arrangement Orchestral", result.Devis);
        Assert.Equal("Contenu du devis", result.Corps.Trim());
    }

    [Fact]
    public void Parse_returns_original_markdown_when_no_frontmatter_is_present()
    {
        const string markdown = "# Hello\n\nThis is plain markdown.";

        var result = FrontmatterParser.Parse(markdown);

        Assert.Equal(string.Empty, result.Formulaire);
        Assert.Equal(string.Empty, result.Devis);
        Assert.Equal(markdown, result.Corps);
    }
}
