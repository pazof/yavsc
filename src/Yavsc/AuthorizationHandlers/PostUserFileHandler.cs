using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    public class PostUserFileHandler : AuthorizationHandler<EditRequirement, FileSpotInfo>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, FileSpotInfo resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName)
            || context.User.IsInRole(Constants.AdminGroupName))
                context.Succeed(requirement);
            if (!context.User.Identity.IsAuthenticated)
                context.Fail();
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
            else context.Fail();
        }

    }
}