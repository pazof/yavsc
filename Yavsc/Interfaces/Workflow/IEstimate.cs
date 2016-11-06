namespace Yavsc.Interfaces
{
    using System.Collections.Generic;
    using Yavsc.Models.Billing;
    public interface IEstimate
    {
        List<string> AttachedFiles { get; set; }
        List<string> AttachedGraphics { get; }
        List<CommandLine> Bill { get; set; }
        string ClientId { get; set; }
        long? CommandId { get; set; }
        string CommandType { get; set; }
        string Description { get; set; }
        long Id { get; set; }
        string OwnerId { get; set; }
        string Title { get; set; }
    }
}