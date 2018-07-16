using Yavsc.Abstract.Identity;

namespace Yavsc.Models.Auth
{
    public class Me : IApplicationUser {
        public Me(string userId, string userName, string email, string avatar, ILocation address, string gCalId)
        {
           Id = userId;
           UserName = userName;
           EMail = email;
           Avatar = avatar;
           PostalAddress = address;
           DedicatedGoogleCalendar = gCalId;
        }
        public string Id { get;  set; }
        public string UserName { get; set; }
        public string EMail { get; set; }
        public string[] Roles { get; set; }
        /// <summary>
        /// Known as profile, could point to an avatar
        /// </summary>
        /// <returns></returns>
        public string Avatar { get; set; }

        public IAccountBalance AccountBalance
        {
            get; set; 
        }

        public string DedicatedGoogleCalendar
        {
            get; set; 
        }

        public ILocation PostalAddress
        {
            get; set; 
        }
    }

}