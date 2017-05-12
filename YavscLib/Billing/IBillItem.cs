namespace YavscLib.Billing
{
    public interface IBillItem {
        string Name { get; set; }
        string Description { get; set; }
        int Count { get; set; }
        decimal UnitaryCost { get; set; }
        string Currency { get; set; }

        string Reference { get; }

    }
}
