namespace YavscLib
{
    public interface IApplicationUser
    {
        IAccountBalance AccountBalance { get; set; }
        string DedicatedGoogleCalendar { get; set; }
        ILocation PostalAddress { get; set; }
    }
}
