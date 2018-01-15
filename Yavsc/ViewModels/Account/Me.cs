using System;
using System.Collections.Generic;
using System.Linq;

namespace  Yavsc.Models.Auth
{
    public class Me : IApplicationUser {
        public Me(ApplicationUser user)
        {
           Id = user.Id;
           UserName = user.UserName;
           EMail = user.Email;
           Avatar = user.Avatar;
           PostalAddress = user.PostalAddress;
           DedicatedGoogleCalendar = user.DedicatedGoogleCalendar;
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