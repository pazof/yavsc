using System.Linq;

namespace  Yavsc.Models.Auth
{
    public class Me {
        public Me(ApplicationUser user)
        {
           id = user.Id;
           givenName = user.UserName;
           emails = new string [] { user.Email } ;
           roles = user.Roles.Select(r=>r.RoleId).ToArray();
        }
        public string id { get;  set; }
        public string givenName { get; set; }
        public string[] emails { get; set; }
        public string[] roles { get; set; }
        /// <summary>
        /// Known as profile, could point to an avatar
        /// </summary>
        /// <returns></returns>
        public string url { get; set; }
        
    }

}