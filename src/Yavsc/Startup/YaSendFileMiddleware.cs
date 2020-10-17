using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.Extensions.Logging;

namespace Yavsc
{
    public class YaSendFileMiddleware
    {


        private readonly RequestDelegate _next;

        private readonly ILogger _logger;

        //
        // Résumé :
        //     Creates a new instance of the SendFileMiddleware.
        //
        // Paramètres :
        //   next:
        //     The next middleware in the pipeline.
        //
        //   loggerFactory:
        //     An Microsoft.Extensions.Logging.ILoggerFactory instance used to create loggers.
        public YaSendFileMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException("loggerFactory");
            }
            _next = next;
            _logger = loggerFactory.CreateLogger<YaSendFileMiddleware>();
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Response.StatusCode < 400 || context.Response.StatusCode >= 600 )
            {
            if (context.Features.Get<IHttpSendFileFeature>() == null)
            {
                context.Features.Set((IHttpSendFileFeature)new YaSendFileWrapper(context.Response.Body, _logger));
            }
            }
            return _next(context);
        }
    }
}
