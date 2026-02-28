namespace Yavsc.Interfaces
{
    
    public interface IComment<TReceiverId> 
    {
        string Article { get; set; }
        TReceiverId ReceiverId { get; set; }
    }
}
