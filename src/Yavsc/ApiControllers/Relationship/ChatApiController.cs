
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using Microsoft.AspNet.Identity;
    using Models;
    using ViewModels.Chat;
    [Route("api/chat")]
    public class ChatApiController : Controller
    {
        ApplicationDbContext dbContext;
        UserManager<ApplicationUser> userManager;
        public ChatApiController(ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpGet("users")]
        public IEnumerable<ChatUserInfo> GetUserList()
        {
            List<ChatUserInfo> result = new List<ChatUserInfo>();
            var cxsQuery = dbContext.ChatConnection?.Include(c=>c.Owner).GroupBy( c => c.ApplicationUserId );

            // List<ChatUserInfo> result = new List<ChatUserInfo>();
            if (cxsQuery!=null)
            foreach (var g in cxsQuery) {

                var uid = g.Key;
                var cxs = g.ToList();
                if (cxs !=null)
                if (cxs.Count>0) {
                    var user = cxs.First().Owner;
                    if (user!=null ) {
                        result.Add(new ChatUserInfo { UserName = user.UserName,
                        UserId = user.Id, Avatar = user.Avatar, Connections = cxs,
                        Roles = (  userManager.GetRolesAsync(user) ).Result.ToArray() });
                        }
                        else {
                            result.Add(new ChatUserInfo {  Connections = cxs });
                        }
                   }
            }
            return result;
        }
    }
}
