namespace  Yavsc.Models.Auth
{
    public class UserInfo {

        public UserInfo()
        {

        }

        public UserInfo(ApplicationUser user)
        {
            UserId = user.Id;
            UserName = user.UserName;
            Avatar = user.Avatar;
        }
        public string UserId { get; set; }

        public string UserName { get; set; }
        public string Avatar { get; set; }
    }
}
