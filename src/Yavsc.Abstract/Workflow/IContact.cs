namespace Yavsc
{
    using Abstract.Identity;
    public interface IContact
    {
        IApplicationUser Owner { get; set; }
        string OwnerId { get; set; }
        string UserId { get; set; }
    }
}
