using Microsoft.AspNet.Authorization;
using Yavsc.ViewModels.Auth;

namespace Yavsc.AuthorizationHandlers
{
    public class HasBadgeHandler : AuthorizationHandler<PrivateChatEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, PrivateChatEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "BadgeNumber" &&
                                            c.Issuer == Startup.Authority))
            {
                return;
            }
            context.Succeed(requirement);
        }
    }
}