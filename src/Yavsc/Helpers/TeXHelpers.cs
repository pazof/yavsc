using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Yavsc.Helpers
{
    using Microsoft.AspNetCore.Mvc.Infrastructure;
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

        public static bool GenerateEstimatePdf(this PdfGenerationViewModel Model)
        {
            string errorMsg = null;
            var billdir = Model.DestDir;
            var tempdir = Config.SiteSetup.TempDir;
            string name = Model.BaseFileName;
            string fullname = new FileInfo(
                 System.IO.Path.Combine(tempdir, name)).FullName;
            string ofullname = new FileInfo(
                 System.IO.Path.Combine(billdir, name)).FullName;

            FileInfo fi = new FileInfo(fullname + ".tex");
            FileInfo fo = new FileInfo(ofullname + ".pdf");
            using (StreamWriter sw = new StreamWriter(fi.FullName))
            {
                sw.Write(Model.TeXSource);
            }
            if (!fi.Exists)
            {
                errorMsg = "Source write failed";
            }
            else
            {
                using (Process p = new Process())
                {
                    p.StartInfo.WorkingDirectory = tempdir;
                    p.StartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        WorkingDirectory = tempdir,
                        FileName = "/usr/bin/texi2pdf",
                        Arguments = $"--batch --build-dir=. -o {fo.FullName} {fi.FullName}"
                    };
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        errorMsg = $"Pdf generation failed with exit code: {p.ExitCode}";
                    }
                    else 
                    {
                      fi.Delete();
                      var di = new DirectoryInfo(Path.Combine(tempdir,$"{Model.BaseFileName}.t2d"));
                      di.Delete(true);

                    }
                }
            }
            Model.Generated = fo.Exists;
            Model.GenerationErrorMessage = new HtmlString(errorMsg);
            return fo.Exists;
        }

        public static string RenderViewToString(
            this Controller controller, IViewEngine engine,
            IActionContextAccessor contextAccessor,
         string viewName, object model, bool isMainPage = true)
        {
            using (var sw = new StringWriter())
            {
                if (engine == null)
                    throw new InvalidOperationException("no engine");

                // try to find the specified view
                controller.TryValidateModel(model);
                ViewEngineResult viewResult = engine.FindView(contextAccessor.ActionContext, viewName, isMainPage);

                // create the associated context
                ViewContext viewContext = new ViewContext();
                viewContext.ActionDescriptor = contextAccessor.ActionContext.ActionDescriptor;
                viewContext.HttpContext = contextAccessor.ActionContext.HttpContext;
                viewContext.TempData = controller.TempData;
                viewContext.View = viewResult.View;
                viewContext.Writer = sw;
                // write the render view with the given context to the stringwriter
                viewResult.View.RenderAsync(viewContext).Wait();
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
