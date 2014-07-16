using System;
using System.Web;

namespace AspNetResources.CustomErrors4
{
	public class MyErrorModule : IHttpModule
	{
        public void Init (HttpApplication app)
        {
            app.Error += new System.EventHandler (OnError);
        }

        public void OnError (object obj, EventArgs args)
        {
            // At this point we have information about the error
            HttpContext ctx = HttpContext.Current;

            Exception exception = ctx.Server.GetLastError ();

            string errorInfo = "<br>Offending URL: " + ctx.Request.Url.ToString () +
                "<br>Source: " + exception.Source + 
                "<br>Message: " + exception.Message +
                "<br>Stack trace: " + exception.StackTrace;

            ctx.Response.Write (errorInfo);

            // --------------------------------------------------
            // To let the page finish running we clear the error
            // --------------------------------------------------
            //ctx.Server.ClearError ();
        }

        public void Dispose () {}
    }
}
