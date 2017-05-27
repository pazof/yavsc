using System;

namespace ZicMoove.Model.Workflow
{
    using Yavsc.Billing;
    public class BillingLine : ICommandLine
    {
        public long Id { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public int Count { get; set; } = 1;

        public decimal UnitaryCost { get; set; }

        public long EstimateId { get; set; }
    }
}
