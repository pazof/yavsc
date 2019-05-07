namespace Yavsc.Abstract.Chat
{
    public interface IConnection
    {
         string ConnectionId { get; set; }
         string UserAgent { get; set; }
         bool Connected { get; set; }
    }

}
