namespace Yavsc.Abstract.Identity.Security
{
    public interface ICircleAuthorized
    {
        long Id { get; set; }
        string GetOwnerId ();
        bool AuthorizeCircle(long circleId);
        ICircleAuthorization [] GetACL();

    }
}
