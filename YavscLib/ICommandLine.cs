namespace YavscLib.Billing
{

    public interface ICommandLine : IBillItem
    {
        // FIXME too hard: no such generic name in any interface
         long Id { get; set; }

        // FIXME too far: perhaps no existing estimate
         long EstimateId { get; set; }
    }
}
