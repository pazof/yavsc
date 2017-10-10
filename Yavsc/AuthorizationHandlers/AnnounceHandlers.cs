

using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Interfaces;

namespace Yavsc.ViewModels.Auth.Handlers
{
    public class AnnouceEditHandler : AuthorizationHandler<EditRequirement, IOwned>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, 
        IOwned resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName)
            || context.User.IsInRole(Constants.AdminGroupName))
                context.Succeed(requirement);
            if (resource.OwnerId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}
