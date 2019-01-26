using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    using Billing;

    public class BillViewHandler : AuthorizationHandler<ViewRequirement, IBillable>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, IBillable resource)
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