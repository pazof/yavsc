using Microsoft.AspNet.Authorization;
using System.Security.Claims;
using Yavsc.Models.Blog;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    public class BlogEditHandler : AuthorizationHandler<EditRequirement, BlogPost>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, BlogPost resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
}