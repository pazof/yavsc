namespace Yavsc
{
    public interface IConnection
    {
         string ConnectionId { get; set; }
         string UserAgent { get; set; }
         bool Connected { get; set; }
    }

    public interface IChatUserInfo
    {
        IConnection[] Connections { get; set; }
        string UserId { get; set; }

        string UserName { get; set; }

        string Avatar { get; set; }

        string[] Roles { get; set; }
    }
}
