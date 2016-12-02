using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models.Billing;

namespace Yavsc.ViewModels.Auth
{
    public class EstimateViewHandler : AuthorizationHandler<ViewRequirement, Estimate>
    {
         protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Estimate resource)
        {
            if (context.User.IsInRole("Moderator"))
                context.Succeed(requirement);
            else if (!context.User.Identity.IsAuthenticated)
                context.Fail();
            else {
            var uid = context.User.GetUserId();
            
            if (resource.OwnerId == uid || resource.Query.ClientId == uid)
                context.Succeed(requirement); 
            else
            // TODO && ( resource.Circles == null || context.User belongs to resource.Circles )
                context.Fail();
            }
        }
    }
}