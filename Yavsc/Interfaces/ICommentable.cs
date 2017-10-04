namespace Yavsc.Interfaces
{
    public interface IComment<T> : IIdentified<T>
    {
          T GetReceiverId(); 
          void SetReceiverId(T rid);
          string Content { get; set; }

    }
}