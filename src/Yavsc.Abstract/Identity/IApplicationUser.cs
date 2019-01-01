namespace Yavsc.Abstract.Identity
{
    public interface IApplicationUser
    {
        string Id { get; set; }
        string UserName { get; set; }
        string Avatar { get ; set; }
        IAccountBalance AccountBalance { get; set; }
        string DedicatedGoogleCalendar { get; set; }
        ILocation PostalAddress { get; set; }
    }
}
