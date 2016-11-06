namespace Yavsc.Interfaces
{
    public interface IContact
    {
        IApplicationUser Owner { get; set; }
        string OwnerId { get; set; }
        string UserId { get; set; }
    }
}