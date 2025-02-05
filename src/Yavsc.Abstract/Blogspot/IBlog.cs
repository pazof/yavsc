﻿


namespace Yavsc
{
    public interface IBlogPost : ITrackedEntity, IIdentified<long>, IRating<long>, ITitle
    {
        string AuthorId { get; set; }
        string Content { get; set; }
        string Photo { get; set; }
        bool Visible { get; set; }
    }
}
