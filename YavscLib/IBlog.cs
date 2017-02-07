
namespace YavscLib
{
    public interface IBlog : IBaseTrackedEntity
    {
        string AuthorId { get; set; }
        string Content { get; set; }
        long Id { get; set; }
        string Photo { get; set; }

        int Rate { get; set; }
        string Title { get; set; }
        bool Visible { get; set; }
    }
}
