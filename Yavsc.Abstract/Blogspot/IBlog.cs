


namespace Yavsc
{
    public interface IBlog : IBaseTrackedEntity, IIdentified<long>, IRating<long>, ITitle
    {
        string AuthorId { get; set; }
        string Content { get; set; }
        string Photo { get; set; }
        bool Visible { get; set; }
    }
}
