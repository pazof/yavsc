﻿using ZicMoove.Model.Workflow;
using System.Collections.Generic;

namespace ZicMoove.Model.Interfaces
{
    public interface IEstimate
    {
        List<string> AttachedFiles { get; set; }
        List<string> AttachedGraphics { get; }
        List<BillingLine> Bill { get; set; }
        string ClientId { get; set; }
        long? CommandId { get; set; }
        string CommandType { get; set; }
        string Description { get; set; }
        long Id { get; set; }
        string OwnerId { get; set; }
        string Title { get; set; }
    }
}