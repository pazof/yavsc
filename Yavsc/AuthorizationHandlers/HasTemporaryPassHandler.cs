using System;
using Microsoft.AspNet.Authorization;

namespace Yavsc.ViewModels.Auth.Handlers
{
    public class HasTemporaryPassHandler : AuthorizationHandler<PrivateChatEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, PrivateChatEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "TemporaryBadgeExpiry" &&
                                            c.Issuer == Startup.Authority))
            {
                return;
            }

            var temporaryBadgeExpiry =
                Convert.ToDateTime(context.User.FindFirst(
                                       c => c.Type == "TemporaryBadgeExpiry" &&
                                       c.Issuer == Startup.Authority).Value);

            if (temporaryBadgeExpiry > DateTime.Now)
            {
                context.Succeed(requirement);
            }
        }
    }
}