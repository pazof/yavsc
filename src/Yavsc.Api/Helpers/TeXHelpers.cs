using System.Diagnostics;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Yavsc.Helpers
{
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

        public static bool GenerateEstimatePdf(this SiteSettings settings, PdfGenerationViewModel Model)
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
            int exitCode;
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
                            + $" -jobname=\"{name}\" -output-directory=\"{billdir}\"",
                        RedirectStandardInput = true,
                    };
                    p.Start();
                    using (var stdin = p.StandardInput)
                    {
                        stdin.Write(Model.TeXSource);
                    }
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                errorMsg = "lualatex invocation failed: " + ex.Message;
                exitCode = -1;
            }

            if (exitCode != 0 && errorMsg == null)
                errorMsg = $"Pdf generation failed with exit code: {exitCode}";
            else if (!fo.Exists && errorMsg == null)
                errorMsg = "Pdf generation produced no output";

            // Remove the LaTeX build artifacts left beside the pdf so only
            // <name>.pdf persists in the bills directory.
            foreach (var ext in new[] { ".aux", ".log", ".out", ".fls",
                        ".fdb_latexmk", ".synctex.gz", ".toc" })
            {
                var f = new FileInfo(System.IO.Path.Combine(billdir, name + ext));
                if (f.Exists) { try { f.Delete(); } catch { } }
            }

            Model.Generated = fo.Exists;
            Model.GenerationErrorMessage = new HtmlString(errorMsg);
            return fo.Exists;
        }

        public static string RenderViewToString(
            this Controller controller, IViewEngine engine,
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
