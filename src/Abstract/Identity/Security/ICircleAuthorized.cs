namespace Yavsc.Abstract.Identity.Security
{
    public interface ICircleAuthorized
    {
        long Id { get; set; }
        string AuthorId { get; }
        bool AuthorizeCircle(long circleId);
        ICircleAuthorization [] GetACL();

    }
}
