using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;
using Yavsc.ViewModels.Auth;
using System.Linq;

namespace Yavsc.AuthorizationHandlers
{
    public class SendMessageHandler : AuthorizationHandler<PrivateChatEntryRequirement, string>
    {
        ApplicationDbContext _dbContext ;

        public SendMessageHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override void Handle(AuthorizationContext context, PrivateChatEntryRequirement requirement, string destUserId)
        {
            var uid = context.User.GetUserId();
            if (context.User.IsInRole(Constants.BlogModeratorGroupName)
            || context.User.IsInRole(Constants.AdminGroupName))
                context.Succeed(requirement);
            else if (!context.User.Identity.IsAuthenticated)
                context.Fail();
            else if (destUserId == uid)
                context.Succeed(requirement);
            else if (_dbContext.Banlist.Any(b=>b.TargetId == uid)) context.Fail();
            else if (_dbContext.BlackListed.Any(b=>b.OwnerId == destUserId && b.UserId == uid)) context.Fail();
            else context.Succeed(requirement); 
        }

    }
}
