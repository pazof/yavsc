using Microsoft.AspNet.Authorization;

namespace Yavsc.ViewModels.Auth.Handlers
{
    using System.Security.Claims;
    using Models.Blog;
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