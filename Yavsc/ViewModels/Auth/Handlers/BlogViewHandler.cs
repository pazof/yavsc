using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;

namespace Yavsc.ViewModels.Auth.Handlers
{
    public class BlogViewHandler : AuthorizationHandler<ViewRequirement, Blog>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Blog resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName)
            || context.User.IsInRole(Constants.AdminGroupName))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement); 
            else if (resource.Visible) {
                if (resource.ACL.Count>0)
                    {
                        var uid = context.User.GetUserId();
                        if (resource.ACL.Any(a=>a.Allowed.Members.Any(m=>m.MemberId == uid )))
                            context.Succeed(requirement); 
                        else context.Fail();
                    }
                else context.Succeed(requirement); 
            }
            else context.Fail();
        }
    }
}