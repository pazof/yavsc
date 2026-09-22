using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    using Microsoft.AspNetCore.Mvc.Razor;
    using ViewModels.Gen;
    public class TeXString : HtmlString
    {

        public TeXString(TeXString teXString): base(teXString.ToString())
        {
        }

        public TeXString(string str) : base(str)
        {


        }

        public static TeXString operator+ (TeXString a, TeXString b) {
            return new TeXString(a.ToString()+b.ToString());
        }

    }
    public class Replacement
        {
            string target;
            string replacement;
            public Replacement(string target, string replacement)
            {
                this.target = target;
                this.replacement = replacement;
            }
            public string Execute(string source)
            {
                return source?.Replace(target, replacement) ?? null;
            }
        }
    public static class TeXHelpers
    {
        public static readonly  Replacement[] SpecialCharsDefaultRendering =
        {
            new Replacement("<","\\textless"),
            new Replacement(">","\\textgreater"),
            new Replacement("©","\\copyright"),
            new Replacement("®","\\textregistered"),
            new Replacement("\\","\\textbackslash"),
            new Replacement("™","\\texttrademark"),
            new Replacement("¶","\\P"),
            new Replacement("|","\\textbar"),
            new Replacement("%","\\%"),
            new Replacement("{","\\{"),
            new Replacement("}","\\}"),
            new Replacement("_","\\_"),
            new Replacement("#","\\#"),
            new Replacement("$","\\$"),
            new Replacement("_","\\_"),
            new Replacement("¿","\\textquestiondown"),
            new Replacement("§","\\S"),
            new Replacement("£","\\pounds"),
            new Replacement("&","\\&"),
            new Replacement("¡","\\textexclamdown"),
            new Replacement("†","\\dag"),
            new Replacement("–","\\textendash"),
            new Replacement("°","\\textdegree")
        };

         public static TeXString ToTeX(this string source, string defaultValue="\\textit{néant}")
        {
            if (source==null) return new TeXString(defaultValue);
            string result=source;
            foreach (var r in SpecialCharsDefaultRendering)
            {
                result = r.Execute(result);
            }
            return new TeXString(result);
        }

        public static TeXString ToTeXCell(this string source, string defaultValue="\\textit{néant}")
        {
            if (source==null) return new TeXString(defaultValue);
            string result=source;
            foreach (var r in SpecialCharsDefaultRendering)
            {
                result = r.Execute(result);
            }
            result = result.Replace("\n","\\tabularnewline ");
            return new TeXString(result);
        }


        public static string NewLinesWith(this string target, string separator)
        {
            var items = target.Split(new char[] { '\n' }).Where(
                s => !string.IsNullOrWhiteSpace(s));

            return string.Join(separator, items);
        }

        public static TeXString ToTeXLines(this string source, string defaultValue, string lineSeparator = "\n\\\\")
        {
            if (source == null) return new TeXString(defaultValue);
            return new TeXString( source.ToTeX().ToString().NewLinesWith(lineSeparator) );
        }

        public static TeXString SplitAddressToTeX (this string source, string lineSeparator = "\n\\\\", string defaultValue = "\\textit{pas d'adresse postale}")
        {
            if (string.IsNullOrWhiteSpace(source)) return new TeXString(defaultValue);
            var alines = source.Split(',');
            var texlines = alines.Select(l=>l.ToTeX().ToString());
            return new TeXString(string.Join(lineSeparator,texlines));
        }

        /// <summary>
        /// Renders a captured signature as TikZ <c>\draw</c>/<c>\fill</c>
        /// paths, suitable for embedding in a <c>tikzpicture</c> at the
        /// foot of a devis. The signature is the PostIt wire format — a
        /// length-prefixed sequence of strokes, each stroke being
        /// <c>[k, x0, y0, x1, y1, …]</c> with coordinates normalised to
        /// <c>[0, CoordinateMax]</c> and y growing downward (screen
        /// orientation). We denormalise to a <paramref name="widthCm"/> ×
        /// <paramref name="heightCm"/> canvas and flip y so the signature
        /// appears upright on paper. A single-point stroke (a pen-down
        /// tap) is rendered as a filled dot, mirroring the round-capped
        /// <c>Polyline</c> the Avalonia pad draws for it.
        /// </summary>
        /// <param name="sig">The persisted signature. Null/empty yields
        /// <see cref="HtmlString.Empty"/>, so the caller can gate the
        /// <c>tikzpicture</c> on a non-empty result.</param>
        public static HtmlString SignatureToTikz(
            this Signature sig, double widthCm, double heightCm)
        {
            if (sig is null || sig.Strokes is null || sig.Strokes.Length == 0)
                return HtmlString.Empty;

            double max = sig.CoordinateMax > 0 ? sig.CoordinateMax : 10_000.0;
            var strokes = sig.Strokes;
            var sb = new StringBuilder();

            int i = 0;
            while (i < strokes.Length)
            {
                int k = strokes[i];
                if (k <= 0) break;
                if (i + 1 + 2 * k > strokes.Length) break;
                i++; // skip the length prefix

                // A single-point stroke: a tap. Draw a round dot so it
                // shows instead of a zero-length, invisible \draw.
                if (k == 1)
                {
                    double px = strokes[i] / max * widthCm;
                    double py = heightCm - strokes[i + 1] / max * heightCm;
                    sb.Append(CultureInfo.InvariantCulture,
                        $"\\fill ({px:F3},{py:F3}) circle (0.8pt); ");
                    i += 2;
                    continue;
                }

                sb.Append("\\draw[line cap=round, line join=round, line width=1.5pt] ");
                for (int p = 0; p < k; p++)
                {
                    double px = strokes[i + 2 * p] / max * widthCm;
                    double py = heightCm - strokes[i + 2 * p + 1] / max * heightCm;
                    if (p == 0) sb.Append(CultureInfo.InvariantCulture,
                        $"({px:F3},{py:F3})");
                    else sb.Append(CultureInfo.InvariantCulture,
                        $" -- ({px:F3},{py:F3})");
                }
                sb.Append("; ");
                i += 2 * k;
            }

            return new HtmlString(sb.ToString());
        }

        public static FileInfo GenerateEstimatePdf(this PdfGenerationViewModel Model)
        {
            string errorMsg = null;
            // Resolve to an absolute path: SiteSettings.Bills may be a
            // relative path (e.g. "bills"), and lualatex resolves
            // -output-directory against its working directory — passing a
            // relative path to both would nest them (<cwd>/bills/bills) and
            // the log/pdf write would fail.
            var billdir = new DirectoryInfo(Model.DestDir).FullName;
            string name = Model.BaseFileName;
            FileInfo fo = new FileInfo(System.IO.Path.Combine(billdir, name + ".pdf"));
            System.IO.Directory.CreateDirectory(billdir);

            // Compile the LaTeX source with the system lualatex, feeding it
            // the TeX on standard input (no intermediate .tex file) and
            // directing the resulting <name>.pdf straight into the bills
            // directory. lualatex is a file-based engine — it cannot emit
            // the PDF on stdout — so the pdf lands at its final destination
            // and we only clean the transient aux/log artifacts beside it.
            //
            // We pass "/dev/stdin" as the input filename so lualatex treats
            // stdin as a regular input *file* rather than the terminal: with
            // no filename argument it reads stdin in terminal mode, which
            // emergency-stops after the first line of a multi-line document.
            // /dev/stdin makes the full multi-line TeX compile cleanly. (The
            // API host is Linux — the systemd service and lualatex dep are
            // Linux-only too.)
            //
            // We capture stdout/stderr so a LaTeX error (missing .sty,
            // missing \includegraphics target, …) is surfaced in
            // GenerationErrorMessage instead of being buried in the .log
            // we then delete.
            int exitCode;
            string stdout = null, stderr = null;
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        WorkingDirectory = billdir,
                        FileName = "lualatex",
                        Arguments = $"-interaction=nonstopmode -halt-on-error"
                            + $" -jobname=\"{name}\" -output-directory=\"{billdir}\""
                            + " /dev/stdin",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };
                    p.Start();
                    // Write the TeX source, then drain stdout/stderr. Read
                    // them after WaitForExit — lualatex's output is small
                    // (a few KB), well under the pipe buffer, so no deadlock.
                    using (var stdin = p.StandardInput)
                    {
                        stdin.Write(Model.TeXSource);
                    }
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                    stdout = p.StandardOutput.ReadToEnd();
                    stderr = p.StandardError.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                errorMsg = "lualatex invocation failed: " + ex.Message;
                exitCode = -1;
            }

            if (exitCode != 0 && errorMsg == null)
            {
                // Surface the engine's own diagnostics (stderr carries the
                // "! Emergency stop" / "! LaTeX Error: …" lines; stdout has
                // the transcript banner) so the caller can see why it failed
                // without having to find the (soon-deleted) .log file.
                var diag = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                errorMsg = $"Pdf generation failed with exit code {exitCode}: {diag}";
            }
            else if (!fo.Exists && errorMsg == null)
                errorMsg = "Pdf generation produced no output";

            // Remove the LaTeX build artifacts left beside the pdf so only
            // <name>.pdf persists in the bills directory. Keep the .log on
            // failure so it can be inspected.
            foreach (var ext in new[] { ".aux", ".log", ".out", ".fls",
                        ".fdb_latexmk", ".synctex.gz", ".toc" })
            {
                if (ext == ".log" && !fo.Exists) continue;
                var fileName = name + ext;
                var f = new FileInfo(System.IO.Path.Combine(billdir, fileName));
                if (f.Exists) { try { f.Delete(); } catch { } }
            }

            Model.Generated = fo.Exists;
            Model.GenerationErrorMessage = new HtmlString(errorMsg);
            return fo;
        }

        public static string RenderViewToString(
            this Controller controller, IRazorViewEngine engine,
            string viewName, object model, bool isMainPage = true)
        {
            if (engine == null)
                throw new InvalidOperationException("no engine");

            // Build an ActionContext from the controller's own context —
            // .NET 10 deprecated IActionContextAccessor, and the controller
            // already holds the live request context on its ControllerContext.
            var controllerContext = controller.ControllerContext;
            var actionContext = new ActionContext(
                controllerContext.HttpContext,
                controllerContext.RouteData,
                controllerContext.ActionDescriptor);

            ViewEngineResult viewResult = engine.FindView(actionContext, viewName, isMainPage);
            if (!viewResult.Success)
                throw new InvalidOperationException(
                    $"View '{viewName}' not found. Searched locations: "
                    + string.Join(", ", viewResult.SearchedLocations));

            // Hand the model to the view through ViewData so the template's
            // @model directive resolves it (the previous implementation never
            // set ViewData.Model, leaving Model null inside the template).
            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    sw,
                    new HtmlHelperOptions());
                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
