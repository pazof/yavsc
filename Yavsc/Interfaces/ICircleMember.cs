namespace Yavsc.Interfaces
{
    public interface ICircleMember: IIdentified<long>
    {
        ICircle Circle { get; set; }
        IApplicationUser Member { get; set; }
    }
}