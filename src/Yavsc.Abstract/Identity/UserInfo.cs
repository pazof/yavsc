#nullable enable

namespace  Yavsc.Abstract.Identity
{
    public class UserInfo {

        public UserInfo()
        {
        }

        public UserInfo(string userId, string userName, string email, string avatar)
        {
            UserId = userId;
            UserName = userName;
            Email = email;
            Avatar = avatar;
        }

        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string[] Roles { get; set; } = [];
        public string? DedicatedGoogleCalendar { get; set; }
    }
}
