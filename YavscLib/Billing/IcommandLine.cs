namespace YavscLib.Billing
{
    public interface ICommandLine
    {
         long Id { get; set; }
         string Description { get; set; }

         int Count { get; set; }
         decimal UnitaryCost { get; set; }
         long EstimateId { get; set; }

    }
}