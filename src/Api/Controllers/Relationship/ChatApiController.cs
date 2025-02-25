using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Yavsc.Models;
using Yavsc.ViewModels.Chat;
using Yavsc.Services;
using Microsoft.EntityFrameworkCore;

namespace Yavsc.Controllers
{

    [Route("api/chat")]
    public class ChatApiController : Controller
    {
        readonly ApplicationDbContext dbContext;
        readonly UserManager<ApplicationUser> userManager;
        private readonly IConnexionManager _cxManager;
        public ChatApiController(ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager, 
        IConnexionManager cxManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            _cxManager = cxManager;
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
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.ApplicationUser.Include(u => u.Connections).FirstOrDefault(u => u.UserName == userName);

            if (user == null) return NotFound();

            return Ok(new ChatUserInfo
            {
                UserName = user.UserName,
                UserId = user.Id,
                Avatar = user.Avatar,
                Connections = user.Connections,
                Roles = (userManager.GetRolesAsync(user)).Result.ToArray()
            });
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
            var list = _cxManager.ListChannels(chanNamePrefix);
            return Ok(list);
        }

        /// <summary>
        /// Get firsts 10 biggest channels
        /// </summary>
        /// <returns></returns>
        [HttpGet("chanlist")]
        public IActionResult GetChanList()
        {
            return Ok(_cxManager.ListChannels(null));
        }

    }
}
