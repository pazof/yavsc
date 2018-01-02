using System.Collections.Generic;

namespace Yavsc.Abstract.Streaming
{
    public interface IChatConnection<T> : IConnection where T: IChatRoomUsage
    {
          List<T> Rooms { get; }

    }
}