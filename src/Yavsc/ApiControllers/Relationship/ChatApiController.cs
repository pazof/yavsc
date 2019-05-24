
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using Yavsc.ViewModels.Chat;


namespace Yavsc.Controllers
{
    using System.Threading.Tasks;

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
        [HttpGet("{userName}", Name = "uinfo")]
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

    }
}
