using System.Collections.Generic;

namespace Yavsc.Abstract.Streaming
{
    public interface IChatRoom<TUsage> where TUsage : IChatRoomUsage
    {
        string Name { get; set; }
        string Topic { get ; set; }
        List<TUsage> UserList { get; }
    }
}