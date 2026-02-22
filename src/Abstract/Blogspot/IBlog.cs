


using Yavsc.Abstract.Identity;

namespace Yavsc
{
    public interface IBlogPostPayLoad
    {      
        string? Article { get; set; }
        string? Photo { get; set; }

    }
    public interface IBlogPost : IBlogPostPayLoad, ITrackedEntity, IIdentified<long>, ITitle
    {
        string AuthorId { get; set; }
        IApplicationUser Author { get; }
    }
}
