using System;

namespace Yavsc.Models
{
    public interface IBlog
    {
        IApplicationUser Author { get; set; }
        string AuthorId { get; set; }
        string bcontent { get; set; }
        long Id { get; set; }
        DateTime modified { get; set; }
        string photo { get; set; }
        DateTime posted { get; set; }
        int rate { get; set; }
        string title { get; set; }
        bool visible { get; set; }
    }
}