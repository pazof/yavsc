using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Yavsc.Helpers
{
    public static class TeXHelpers
    {
        public static string NewLinesWith(this string target, string separator)
        { 
            var items = target.Split(new char[] {'\n'}).Where(
                s=> !string.IsNullOrWhiteSpace(s) ) ;

            return string.Join(separator, items);
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