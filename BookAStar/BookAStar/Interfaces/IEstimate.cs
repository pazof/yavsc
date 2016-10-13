using BookAStar.Model.Workflow;
using System.Collections.Generic;

namespace BookAStar.Model.Interfaces
{
    public interface IEstimate
    {
        IList<string> AttachedFiles { get; set; }
        IList<string> AttachedGraphics { get; }
        IList<BillingLine> Bill { get; set; }
        string ClientId { get; set; }
        long? CommandId { get; set; }
        string CommandType { get; set; }
        string Description { get; set; }
        long Id { get; set; }
        string OwnerId { get; set; }
        int? Status { get; set; }
        string Title { get; set; }
    }
}