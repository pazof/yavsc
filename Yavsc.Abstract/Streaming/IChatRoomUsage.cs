namespace Yavsc.Abstract.Streaming
{

    public interface IChatRoomUsage {

         string ChannelName { get; set; }

         string ChatUserConnectionId { get; set; }
        
        ChatRoomUsageLevel Level { get; set; }
    }
}