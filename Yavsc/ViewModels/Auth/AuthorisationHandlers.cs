using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;
using Yavsc.Models.Booking;

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

    public class FileSpotInfo : IAuthorizationRequirement
    {
        public DirectoryInfo PathInfo { get; private set; } 
        public FileSpotInfo(string path, Blog b) {
            PathInfo = new DirectoryInfo(path);
            AuthorId = b.AuthorId;
            BlogEntryId = b.Id;
        }
        public string AuthorId { get; private set; }
        public long BlogEntryId { get; private set; }
        
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
            if (context.User.IsInRole(Constants.BlogModeratorGroupName))
                context.Succeed(requirement);
            else if (context.User.Identity.IsAuthenticated)
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
        }

    }
    public class PostUserFileHandler : AuthorizationHandler<EditRequirement, FileSpotInfo>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, FileSpotInfo resource)
        {
            if (context.User.IsInRole(Constants.BlogModeratorGroupName)
            || context.User.IsInRole(Constants.AdminGroupName))
                context.Succeed(requirement);
            if (!context.User.Identity.IsAuthenticated)
                context.Fail();
            if (resource.AuthorId == context.User.GetUserId())
                context.Succeed(requirement);
            else context.Fail();
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
            else if (resource.visible)
            // TODO && ( resource.Circles == null || context.User belongs to resource.Circles )
                context.Succeed(requirement); 
        }
    }

    public class CommandViewHandler : AuthorizationHandler<ViewRequirement, BookQuery>
    {
        protected override void Handle(AuthorizationContext context, ViewRequirement requirement, BookQuery resource)
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
    public class CommandEditHandler : AuthorizationHandler<EditRequirement, BookQuery>
    {
        protected override void Handle(AuthorizationContext context, EditRequirement requirement, BookQuery resource)
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