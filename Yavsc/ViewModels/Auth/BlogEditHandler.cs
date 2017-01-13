using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;

namespace Yavsc.ViewModels.Auth
{
    public class BlogEditHandler : AuthorizationHandler<EditRequirement, Blog>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, Blog resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}