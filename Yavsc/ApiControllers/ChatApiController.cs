
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using Models;
    using ViewModels.Chat;
    [Route("api/chat")]
    public class ChatApiController : Controller
    {

        [HttpGet("users")]
        public List<ChatUserInfo> GetUserList()
        {
            using (var db = new ApplicationDbContext()) {

                var cxsQuery = db.Connections.Include(c=>c.Owner).GroupBy( c => c.ApplicationUserId );

                List<ChatUserInfo> result = new List<ChatUserInfo>();

                foreach (var g in cxsQuery) {

                    var uid = g.Key;
                    var cxs = g.ToList();
                    var user = cxs.First().Owner;

                    result.Add(new ChatUserInfo { UserName = user.UserName,
                    UserId = user.Id, Avatar = user.Avatar, Connections = cxs } );

                }
               return result;
            }
        }
    }
}
