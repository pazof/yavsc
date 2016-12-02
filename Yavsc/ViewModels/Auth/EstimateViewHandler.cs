using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models.Billing;

namespace Yavsc.ViewModels.Auth
{
    public class EstimateViewHandler : AuthorizationHandler<ViewRequirement, Estimate>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Estimate resource)
        {
            
            if (context.User.IsInRole(Constants.AdminGroupName)
            || context.User.IsInRole(Constants.FrontOfficeGroupName))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated) {
                var uid = context.User.GetUserId();
                if (resource.OwnerId == uid || resource.ClientId == uid)
                    context.Succeed(requirement); 
                // TODO && ( resource.Circles == null || context.User belongs to resource.Circles )
            }
        }
    }
}