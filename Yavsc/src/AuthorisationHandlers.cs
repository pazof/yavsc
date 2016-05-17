using System;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;

namespace Yavsc {
    public class PrivateChatEntryRequirement : IAuthorizationRequirement
    {
    }

    public class EditRequirement : IAuthorizationRequirement
    {
        public EditRequirement()
        {
        }
    }

    public class ViewRequirement : IAuthorizationRequirement
    {
        public ViewRequirement()
        {
        }
    }
    public class BlogEditHandler : AuthorizationHandler<EditRequirement, Blog>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, Blog resource)
        {
            if (context.User.IsInRole("Moderator"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
public class BlogViewHandler : AuthorizationHandler<ViewRequirement, Blog>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Blog resource)
        {
            if (context.User.IsInRole("Moderator"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
            // TODO else if (resource.Circles && context.User belongs to
        }

    }

    public class CommandViewHandler : AuthorizationHandler<ViewRequirement, Command>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, Command resource)
        {
            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.ClientId == context.User.GetUserId())
                context.Succeed(requirement);
            else if (resource.PerformerId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
    public class CommandEditHandler : AuthorizationHandler<EditRequirement, Command>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, Command resource)
        {
            if (context.User.IsInRole("FrontOffice"))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.ClientId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
    public class HasTemporaryPassHandler : AuthorizationHandler<PrivateChatEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, PrivateChatEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "TemporaryBadgeExpiry" &&
                                            c.Issuer == Constants.Issuer))
            {
                return;
            }

            var temporaryBadgeExpiry =
                Convert.ToDateTime(context.User.FindFirst(
                                       c => c.Type == "TemporaryBadgeExpiry" &&
                                       c.Issuer == Constants.Issuer).Value);

            if (temporaryBadgeExpiry > DateTime.Now)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class HasBadgeHandler : AuthorizationHandler<PrivateChatEntryRequirement>
    {
        protected override void Handle(AuthorizationContext context, PrivateChatEntryRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "BadgeNumber" &&
                                            c.Issuer == Constants.Issuer))
            {
                return;
            }
            context.Succeed(requirement);
        }
    }

}