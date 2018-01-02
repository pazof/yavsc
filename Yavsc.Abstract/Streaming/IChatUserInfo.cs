namespace Yavsc.Abstract.Streaming
{

    public interface IChatUserInfo
    {
        IConnection[] Connections { get; set; }
        string UserId { get; set; }

        string UserName { get; set; }

        string Avatar { get; set; }

        string[] Roles { get; set; }
    }
}