﻿using Microsoft.AspNet.Builder;
using Microsoft.Owin.Builder;
using Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yavsc
{
    using Microsoft.AspNet.SignalR;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseAppBuilder(
            this IApplicationBuilder app,
            Action<IAppBuilder> configure)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties["builder.DefaultApp"] = next;

                    configure(appBuilder);

                    return appBuilder.Build<AppFunc>();
                });
            });

            return app;
        }

        public static void UseSignalR(this IApplicationBuilder app)
        {
            app.UseAppBuilder(appBuilder => appBuilder.MapSignalR(
               new HubConfiguration() {
                  EnableDetailedErrors = true,
                  EnableJSONP = true
               }
            ));
        }
    }
}