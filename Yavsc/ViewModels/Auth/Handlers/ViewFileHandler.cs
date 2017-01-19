using Microsoft.AspNet.Authorization;

namespace Yavsc.ViewModels.Auth.Handlers
{
    public class ViewFileHandler : AuthorizationHandler<ViewRequirement, ViewFileContext>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement,  ViewFileContext fileContext)
        {
            // TODO file access rules
            if (fileContext.Path.StartsWith("/pub/"))
                context.Succeed(requirement);
            else {
                context.Succeed(requirement);
            }
        }
    }
}