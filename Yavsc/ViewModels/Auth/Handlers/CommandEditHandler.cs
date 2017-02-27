using System.Security.Claims;
using Microsoft.AspNet.Authorization;

namespace Yavsc.ViewModels.Auth.Handlers
{
    using Models.Workflow;
    public class CommandEditHandler : AuthorizationHandler<EditRequirement, RdvQuery>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, RdvQuery resource)
        {
            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.ClientId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}