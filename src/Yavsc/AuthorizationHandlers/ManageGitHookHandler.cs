using Microsoft.AspNet.Authorization;
using Yavsc.Server.Models.IT.SourceCode;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    public class ManageGitHookHandler: AuthorizationHandler<EditRequirement, GitRepositoryReference>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, GitRepositoryReference resource)
        {
            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
                context.Succeed(requirement);
        }

    }
}