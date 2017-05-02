namespace  Yavsc.Models.Auth
{
    public class UserInfo {

        public UserInfo()
        {

        }

        public UserInfo(string userId, string userName, string avatar)
        {
            UserId = userId;
            UserName = userName;
            Avatar = avatar;
        }
        public string UserId { get; set; }

        public string UserName { get; set; }
        public string Avatar { get; set; }
    }
}
