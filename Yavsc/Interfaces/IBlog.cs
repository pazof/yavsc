namespace Yavsc.Interfaces
{
    public interface IBlog: IIdentified<long>, IRating<long>, ITitle, ILifeTime

    {
        string AuthorId { get; set; }
        string Content { get; set; }
        string Photo { get; set; }
        bool Visible { get; set; }
    }
}