namespace  Yavsc.Models.Auth
{
    public class Me {
        public Me(ApplicationUser user)
        {
           Id = user.Id;
           GivenName = user.UserName;
           Emails = new string [] { user.Email } ;
        }
        public string Id { get;  set; }
        public string GivenName { get; set; }
        public string[] Emails { get; set; }
        /// <summary>
        /// Known as profile, could point to an avatar
        /// </summary>
        /// <returns></returns>
        public string Url { get; set; }
        
    }

}