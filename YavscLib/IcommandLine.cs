namespace YavscLib.Billing
{
    public interface IBillItem {
        string Description { get; set; }
        int Count { get; set; }
        decimal UnitaryCost { get; set; }
        string Currency { get; set; }

    }
    public interface ICommandLine : IBillItem
    {
        // FIXME too hard: no such generic name in any interface
         long Id { get; set; }

        // FIXME too far: perhaps no existing estimate
         long EstimateId { get; set; }

    }
}
