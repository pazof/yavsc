namespace Yavsc
{
    using System.Collections.Generic;
    public interface IEstimate
    {
        List<string> AttachedFiles { get; set; }
        List<string> AttachedGraphics { get; }
        string ClientId { get; set; }
        long? CommandId { get; set; }
        string CommandType { get; set; }
        string Description { get; set; }
        long Id { get; set; }
        string OwnerId { get; set; }
        string Title { get; set; }

    }
}
