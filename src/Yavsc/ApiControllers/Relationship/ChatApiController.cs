
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using System.Security.Claims;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using Yavsc.ViewModels.Chat;


namespace Yavsc.Controllers
{

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
            var cxsQuery = dbContext.ChatConnection?.Include(c => c.Owner)
            .Where(cx => cx.Connected).GroupBy(c => c.ApplicationUserId);

            if (cxsQuery != null)
                foreach (var g in cxsQuery)
                {

                    var uid = g.Key;
                    var cxs = g.ToList();
                    if (cxs != null)
                        if (cxs.Count > 0)
                        {
                            var user = cxs.First().Owner;
                            if (user != null)
                            {
                                result.Add(new ChatUserInfo
                                {
                                    UserName = user.UserName,
                                    UserId = user.Id,
                                    Avatar = user.Avatar,
                                    Connections = cxs,
                                    Roles = (userManager.GetRolesAsync(user)).Result.ToArray()
                                });
                            }
                            else
                            {
                                result.Add(new ChatUserInfo { Connections = cxs });
                            }
                        }
                }

            return result;
        }

        // GET: api/chat/userName
        [HttpGet("user/{userName}", Name = "uinfo")]
        public IActionResult GetUserInfo([FromRoute] string userName)
        {
            if (!ModelState.IsValid)
            // Miguel mech profiler
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            var user = dbContext.ApplicationUser.Include(u => u.Connections).FirstOrDefault(u => u.UserName == userName);

            if (user == null) return HttpNotFound();

            return Ok(new ChatUserInfo
            {
                UserName = user.UserName,
                UserId = user.Id,
                Avatar = user.Avatar,
                Connections = user.Connections,
                Roles = (userManager.GetRolesAsync(user)).Result.ToArray()
            });
        }

        public class ChannelShortInfo {
            public string RoomName {get; set;}
            public string Topic { get; set; }
        }

        /// <summary>
        /// Get firsts 10 biggest channels having
        /// a name starting with given prefix.
        /// </summary>
        /// <param name="chanNamePrefix">chan Name Prefix</param>
        /// <returns></returns>
        [HttpGet("chanlist/{chanNamePrefix}")]
        public IActionResult GetChanList([FromRoute] string chanNamePrefix)
        {
            var list = ChatHub.Channels.Where(c => c.Key.StartsWith(chanNamePrefix)).OrderByDescending(c=>c.Value.Users.Count).Select(c=>new ChannelShortInfo { RoomName= c.Key, Topic = c.Value.Topic }).Take(10);
            return Ok(list);
        }

        /// <summary>
        /// Get firsts 10 biggest channels
        /// </summary>
        /// <returns></returns>
        [HttpGet("chanlist")]
        public IActionResult GetChanList()
        {
            return Ok(ChatHub.Channels.OrderByDescending(c=>c.Value.Users.Count).Select(c=> new ChannelShortInfo { RoomName= c.Key, Topic = c.Value.Topic })
                .Take(10));
        }

    }
}
