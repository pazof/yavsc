using System;

namespace BookAStar.Interfaces
{
    public interface IBillingLine
    {
        int Count { get; set; }
        string Description { get; set; }
        TimeSpan Duration { get; set; }
        decimal UnitaryCost { get; set; }
    }
}