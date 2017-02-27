using System.Security.Claims;
using Microsoft.AspNet.Authorization;

namespace Yavsc.ViewModels.Auth.Handlers
{
    using Models.Workflow;
    public class CommandViewHandler : AuthorizationHandler<ViewRequirement, RdvQuery>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, RdvQuery resource)
        {
            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.ClientId == context.User.GetUserId())
                context.Succeed(requirement);
            else if (resource.PerformerId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}