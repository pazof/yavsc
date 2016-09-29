
using BookAStar.Interfaces;
using System;

namespace BookAStar.Model.Workflow
{
    public class BillingLine : IBillingLine
    {
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public int Count { get; set; } = 1;
        public decimal UnitaryCost { get; set; }
    }
}
