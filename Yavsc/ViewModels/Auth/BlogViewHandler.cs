using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;

namespace Yavsc.ViewModels.Auth
{
    public class BlogViewHandler : AuthorizationHandler<ViewRequirement, Blog>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Blog resource)
        {
            if (context.User.IsInRole("Moderator"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement); 
            else if (resource.Visible)
            // TODO && ( resource.Circles == null || context.User belongs to resource.Circles )
                context.Succeed(requirement); 
        }
    }
}