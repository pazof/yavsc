using System.Collections.Generic;
using System.Linq;

namespace  Yavsc.Models.Auth
{
    public class Me {
        public Me(string useruserid, 
        string username, 
        IEnumerable<string> emails, 
        IEnumerable<string> roles,
        string avatar)
        {
           Id = useruserid;
           UserName = username;
           EMails = emails.ToArray();
           Roles = roles.ToArray();
           Avatar = avatar;
        }
        public string Id { get;  set; }
        public string UserName { get; set; }
        public string[] EMails { get; set; }
        public string[] Roles { get; set; }
        /// <summary>
        /// Known as profile, could point to an avatar
        /// </summary>
        /// <returns></returns>
        public string Avatar { get; set; }
        
    }

}