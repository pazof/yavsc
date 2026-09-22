using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Yavsc.Helpers;
using Yavsc.Models.Billing;
using Yavsc.ViewModels.Gen;

namespace Yavsc.Api.Test;

// Unit test of the estimate PDF generation pipeline (no HTTP fixture
// needed): it exercises TeXHelpers.SignatureToTikz — the vector drawing
// of the provider and client signatures — and TeXHelpers.GenerateEstimatePdf
// — the lualatex stdin compile that writes <name>.pdf into DestDir. The
// TeX source mirrors what the Estimate_tex template emits for a devis
// signed by both parties: two side-by-side minipages, each with a
// tikzpicture bounded by \useasboundingbox and the strokes drawn by
// SignatureToTikz.
//
// The PDF test requires the system `lualatex` (and the TeX Live packages
// listed in the README runtime-deps section) on the host running the
// tests. On hosts without lualatex it skips cleanly rather than failing.
public sealed class EstimatePdfGenerationTests
{
    // A provider signature: a downstroke, a cross, and a tap (dot).
    // PostIt wire format — [k, x0,y0, x1,y1, …] per stroke, coords in
    // [0, 10000], y-down.
    private static readonly int[] ProStrokes =
    {
        4, 2000, 8000, 2000, 5000, 2000, 2000, 2100, 1900,
        3, 2000, 5000, 3500, 4500, 3500, 5100,
        1, 4000, 3000,
    };

    // A client signature: a wavy stroke and a shorter one.
    private static readonly int[] CliStrokes =
    {
        5, 1000, 7000, 1500, 4000, 2200, 3000, 3000, 2500, 4000, 1500,
        2, 3000, 6000, 4200, 5500,
    };

    [Fact]
    public void SignatureToTikz_RendersBothSignaturesAsVectorDraws()
    {
        var pro = MakeSig(SignatureType.Pro, ProStrokes);
        var cli = MakeSig(SignatureType.Client, CliStrokes);

        var proTex = pro.SignatureToTikz(5, 2.5).ToString();
        var cliTex = cli.SignatureToTikz(5, 2.5).ToString();

        Assert.Contains("\\draw[line cap=round, line join=round, line width=1.5pt]", proTex);
        Assert.Contains("\\fill", proTex);            // the single-point tap → dot
        Assert.Contains("(1.000,0.500)", proTex);    // first point (2000,8000), y flipped
        Assert.Contains("\\draw[line cap=round, line join=round, line width=1.5pt]", cliTex);
        Assert.Contains("--", cliTex);
    }

    [Fact]
    public void SignatureToTikz_EmptySignatureIsEmpty()
    {
        var empty = MakeSig(SignatureType.Pro, Array.Empty<int>());
        Assert.Equal("", empty.SignatureToTikz(5, 2.5).ToString());
    }

    [Fact]
    public void GenerateEstimatePdf_WithBothSignatures_ProducesPdfInDestDir()
    {
        if (!IsLualatexAvailable())
            throw Xunit.Sdk.SkipException.ForSkip(
                "lualatex is not installed on this host; skipping PDF generation test. " +
                "Install the TeX Live runtime deps documented in the README.");

        var destDir = Path.Combine(Path.GetTempPath(),
            "yavsc-estimate-pdf-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(destDir);

        try
        {
            var pro = MakeSig(SignatureType.Pro, ProStrokes);
            var cli = MakeSig(SignatureType.Client, CliStrokes);
            var baseName = "estimate-test-both-signed";

            var model = new PdfGenerationViewModel
            {
                TeXSource = BuildEstimateTeX(pro, cli),
                BaseFileName = baseName,
                DestDir = destDir,
            };

            var fo = model.GenerateEstimatePdf();

            // The PDF must exist, at the expected path, and be non-empty.
            var expectedPath = Path.Combine(destDir, baseName + ".pdf");
            Assert.True(fo is not null && fo.Exists,
                "GenerateEstimatePdf returned a non-existent file. Error: "
                + model.GenerationErrorMessage?.Value);
            Assert.Equal(expectedPath, fo.FullName);
            Assert.True(fo.Length > 0, "generated PDF is empty");

            // Only the PDF should persist in DestDir — aux/log artifacts
            // are cleaned up by GenerateEstimatePdf on success.
            var leftovers = Directory.GetFiles(destDir)
                .Where(p => !Path.GetFileName(p).Equals(baseName + ".pdf", StringComparison.Ordinal))
                .ToArray();
            Assert.Empty(leftovers);
        }
        finally
        {
            if (Directory.Exists(destDir))
                Directory.Delete(destDir, recursive: true);
        }
    }

    private static Signature MakeSig(SignatureType type, int[] strokes)
        => new()
        {
            Type = type,
            CoordinateMax = 10_000,
            Strokes = strokes,
            CapturedAtUtc = new DateTime(2026, 9, 22, 0, 0, 0, DateTimeKind.Utc),
        };

    // Builds the LaTeX source the Estimate_tex template would emit for a
    // devis signed by both parties: same preamble (incl. tikz + bera) and
    // the two-minipage signature foot, with the strokes drawn by
    // SignatureToTikz.
    private static string BuildEstimateTeX(Signature pro, Signature cli)
    {
        var fr = CultureInfo.CreateSpecificCulture("fr-FR");
        var date = new DateTime(2026, 9, 22).ToString("dddd dd MMMM yyyy", fr);
        var proTikz = pro.SignatureToTikz(5, 2.5).ToString();
        var cliTikz = cli.SignatureToTikz(5, 2.5).ToString();

        return $@"\documentclass[french,11pt]{{article}}
\usepackage{{eurosym}}
\usepackage{{babel}}
\usepackage[T1]{{fontenc}}
\usepackage[utf8]{{inputenc}}
\usepackage[a4paper]{{geometry}}
\usepackage{{units}}
\usepackage{{bera}}
\usepackage{{graphicx}}
\usepackage{{fancyhdr}}
\usepackage{{fp}}
\usepackage{{tikz}}
\geometry{{verbose,tmargin=4em,bmargin=8em,lmargin=6em,rmargin=6em}}
\setlength{{\parindent}}{{0pt}}
\begin{{document}}
Devis n°1 --- test signe par les deux parties

\vspace{{2em}}
\noindent
\begin{{minipage}}[t]{{0.45\textwidth}}
  \textbf{{Signature du fournisseur}}\\
\small Valide le {date}\\
\begin{{tikzpicture}}
  \useasboundingbox (0,0) rectangle (5,2.5);
  {proTikz}
\end{{tikzpicture}}
\end{{minipage}}\hfill
\begin{{minipage}}[t]{{0.45\textwidth}}
  \textbf{{Signature du client}}\\
\small Valide le {date}\\
\begin{{tikzpicture}}
  \useasboundingbox (0,0) rectangle (5,2.5);
  {cliTikz}
\end{{tikzpicture}}
\end{{minipage}}
\end{{document}}
";
    }

    private static bool IsLualatexAvailable()
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("lualatex", "--version")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (p is null) return false;
            return p.WaitForExit(5000) && p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}