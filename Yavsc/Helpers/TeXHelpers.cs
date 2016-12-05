using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Yavsc.ViewModels.Gen;

namespace Yavsc.Helpers
{
    public class TeXString
    {

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
                return source.Replace(target, replacement);
            }
        }

        public readonly static Replacement[] SpecialCharsRendering =
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
            new Replacement("–","\\textendash")
        };
        string data;
        public TeXString(string str)
        {
            data = str;
            foreach (var r in SpecialCharsRendering)
            {
                data = r.Execute(data);
            }
        }

        override public string ToString()
        {
            return data;
        }
    }
    public static class TeXHelpers
    {
        public static string NewLinesWith(this string target, string separator)
        {
            var items = target.Split(new char[] { '\n' }).Where(
                s => !string.IsNullOrWhiteSpace(s));

            return string.Join(separator, items);
        }

        public static TeXString ToTeX(string target, string lineSeparator = "\n\\\\")
        {
            if (target == null) return null;
            return new TeXString(target.NewLinesWith(lineSeparator));
        }
        public static bool GenerateEstimatePdf(this PdfGenerationViewModel Model)
        {
            string errorMsg = null;
            var billdir = Model.DestDir;
            var tempdir = Startup.SiteSetup.TempDir;
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
                    p.StartInfo = new ProcessStartInfo();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = "/usr/bin/texi2pdf";
                    p.StartInfo.Arguments = $"--batch --build-dir=. -o {fo.FullName} {fi.FullName}";
                    p.Start();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        errorMsg = $"Pdf generation failed with exit code: {p.ExitCode}";
                    }
                }
                fi.Delete();
            }
            Model.Generated = fo.Exists;
            Model.GenerationErrorMessage = new HtmlString(errorMsg);
            return fo.Exists;
        }

        public static string RenderViewToString(
            this Controller controller, IViewEngine engine,
            IHttpContextAccessor httpContextAccessor,
         string viewName, object model)
        {
            using (var sw = new StringWriter())
            {
                if (engine == null)
                    throw new InvalidOperationException("no engine");

                // try to find the specified view
                controller.TryValidateModel(model);
                ViewEngineResult viewResult = engine.FindPartialView(controller.ActionContext, viewName);
                // create the associated context
                ViewContext viewContext = new ViewContext();
                viewContext.ActionDescriptor = controller.ActionContext.ActionDescriptor;
                viewContext.HttpContext = controller.ActionContext.HttpContext;
                viewContext.TempData = controller.TempData;
                viewContext.View = viewResult.View;
                viewContext.Writer = sw;

                // write the render view with the given context to the stringwriter
                viewResult.View.RenderAsync(viewContext);
                viewResult.EnsureSuccessful();
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}