namespace Yavsc.Abstract.Streaming
{
    public interface IConnection
    {
         string ConnectionId { get; set; }
         string UserAgent { get; set; }
         bool Connected { get; set; }
    }

}
