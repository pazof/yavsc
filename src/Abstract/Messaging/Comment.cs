
using Yavsc.Interfaces;

public class CommentPost : IComment<long>
    {
        public long ReceiverId { get; set; }
        public long? ParentId { get; set; }
        public string Content { get; set; }
    }
