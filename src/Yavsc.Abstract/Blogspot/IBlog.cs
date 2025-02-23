


namespace Yavsc
{
    public interface IBlogPostPayLoad
    {      
        string Content { get; set; }
        string Photo { get; set; }

    }
    public interface IBlogPost :IBlogPostPayLoad, ITrackedEntity, IIdentified<long>, ITitle
    {
        string AuthorId { get; set; }
    }
}
