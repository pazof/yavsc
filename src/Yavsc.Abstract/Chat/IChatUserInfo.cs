namespace Yavsc.Abstract.Chat
{

    public interface IChatUserInfo
    {
        string UserId { get; set; }

        string UserName { get; set; }

        string Avatar { get; set; }

        string[] Roles { get; set; }
    }
}