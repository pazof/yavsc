using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Yavsc.Org.Tests;

/// <summary>
/// Startup filter that injects <see cref="TestUserMiddleware"/> after
/// the authentication and authorization middleware. The
/// <see cref="IStartupFilter.Configure"/> contract wraps the existing
/// pipeline: the <c>next</c> delegate is the rest of the app's
/// pipeline, so we run our middleware <em>before</em> it but
/// <em>after</em> anything that was registered as a startup filter
/// earlier in the chain.
///
/// In practice this puts <c>TestUserMiddleware</c> ahead of
/// <c>UseAuthentication</c> (registered inside <c>ConfigurePipeline</c>)
/// because the production code path runs <c>UseAuthentication</c>
/// synchronously inside <c>Configure</c>, after all startup filters
/// have wrapped it. We want the opposite: the test identity must be
/// visible to authorization and the controller, so we register
/// <c>TestUserMiddleware</c> via <see cref="IApplicationBuilder.Use"/>
/// inside the filter such that it runs late in the chain. The
/// simplest way to achieve that is to register the middleware
/// after the production authorization pipeline: we wrap with our
/// middleware inside the filter, so our delegate sits between the
/// framework middleware (set up by Configure) and the rest of the
/// pipeline — meaning requests flow:
/// framework authN/authZ → TestUserMiddleware → next pipeline.
/// </summary>
public class TestUserStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Replay the production pipeline first (this is what
            // Program.Main + ConfigurePipeline set up, including
            // UseAuthentication and UseAuthorization).
            next(app);
            // Then add our middleware on top. UseMiddleware<T> wires
            // it through the same IMiddlewareActivator the framework
            // uses, so the dependency on TestUserMiddleware is
            // resolved from the request scope.
            app.UseMiddleware<TestUserMiddleware>();
        };
    }
}
