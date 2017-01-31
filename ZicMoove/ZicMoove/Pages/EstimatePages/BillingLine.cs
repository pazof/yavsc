
using ZicMoove.Interfaces;
using System;

namespace ZicMoove.Model.Workflow
{
    public class BillingLine : IBillingLine
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public int Count { get; set; } = 1;
        public decimal UnitaryCost { get; set; }
    }
}
