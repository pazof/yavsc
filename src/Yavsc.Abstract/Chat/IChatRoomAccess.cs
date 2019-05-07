namespace Yavsc.Abstract.Chat
{

    public interface IChatRoomAccess 
    {
        long Id { get; }
        
        ChatRoomAccessLevel Level { get; set; }

        string UserId { get; }
    }
}