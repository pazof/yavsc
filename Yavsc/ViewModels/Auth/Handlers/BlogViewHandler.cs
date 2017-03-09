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
            bool ok=false;
             if (resource.Visible) {
                if (resource.ACL==null)
                    ok=true;
                else if (resource.ACL.Count==0) ok=true; 
                 else   {
                        if (context.User.IsSignedIn()) {
                            var uid = context.User.GetUserId();
                            if (resource.ACL.Any(a=>a.Allowed!=null && a.Allowed.Members.Any(m=>m.MemberId == uid )))
                                ok=true; 
                        } 
                    }
            } 
            if (ok) context.Succeed(requirement);
            else {
                if (context.User.IsInRole(Constants.AdminGroupName) || 
                context.User.IsInRole(Constants.BlogModeratorGroupName))
                 context.Succeed(requirement);
                 else context.Fail();
             }
        }
    }
}