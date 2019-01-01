
using System.Web.Optimization;

namespace Yavsc
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/bootjq").Include(
                "~/bower_components/bootstrap/dist/js",
            "~/bower_components/jquery/dist/js",
            "~/bower_components/jquery.validation/dist/js",
            "~/bower_components/jquery-validation-unobtrusive/dist/js",
            "~/bower_components/bootstrap-datepicker/dist/js"));
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            ));
            bundles.Add(new ScriptBundle("~/bundles/markdown").Include(
                "~/bower_components/dropzone/dist/min/dropzone-amd-module.min.js",
                "~/bower_components/dropzone/dist/min/dropzone.min.js"
            ));
            bundles.Add(new StyleBundle("~/Content/markdown").Include(
                "~/bower_components/dropzone/dist/min/basic.min.css",
                "~/bower_components/dropzone/dist/min/dropzone.min.css"
            ));
        }
    }
}
