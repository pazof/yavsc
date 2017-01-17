using System.Globalization;

namespace Yavsc
{
    using Microsoft.AspNet.Mvc.Filters;
    using Microsoft.Extensions.Logging;

    public class LanguageActionFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public LanguageActionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("LanguageActionFilter");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string culture = null;
            var routedCulture = context.RouteData.Values["culture"];
            if (routedCulture != null) {
                culture = routedCulture.ToString();
                _logger.LogInformation($"Setting the culture from the URL: {culture}");

            }
            else {
                   if (context.HttpContext.Request.Headers.ContainsKey("accept-language"))
                   {
                       // fr,en-US;q=0.7,en;q=0.3
                       string spec = context.HttpContext.Request.Headers["accept-language"];
                        _logger.LogInformation($"Setting the culture from language header spec: {spec}");

                       string firstpart = spec.Split(';')[0];
                       foreach (string lang in firstpart.Split(','))
                       {
                           // TODO do it from the given options  ...
                           // just take the main part :-)
                           string mainlang = lang.Split('-')[0];
                           if (mainlang=="fr"||mainlang=="en") {
                                culture = mainlang;
                              _logger.LogInformation($"Setting the culture from header: {culture}");
                              break;
                           }
                       }
                   }
            }
            if (culture != null) {
#if DNX451
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
#else
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
#endif
}
            base.OnActionExecuting(context);
        }
    }
}


