using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    using Billing;
    public class BillEditHandler : AuthorizationHandler<EditRequirement, IBillable>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, IBillable resource)
        {

            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.ClientId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}