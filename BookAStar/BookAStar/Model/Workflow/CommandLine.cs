
using System;

namespace BookAStar.Model.Workflow
{
    public class CommandLine
    {
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public int Count { get; set; }
        public decimal UnitaryCost { get; set; }
    }
}
