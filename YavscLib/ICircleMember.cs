namespace Yavsc.Models
{
    public interface ICircleMember
    {
        ICircle Circle { get; set; }
        long Id { get; set; }
        IApplicationUser Member { get; set; }
    }
}