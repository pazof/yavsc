namespace Yavsc.Interfaces
{
    
    public interface IComment<TReceiverId> 
    {
        string Content { get; set; }
        TReceiverId ReceiverId { get; set; }
    }
}
