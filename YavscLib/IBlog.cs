using System;

namespace Yavsc.Models
{
    public interface IBlog
    {
        string AuthorId { get; set; }
        string Content { get; set; }
        long Id { get; set; }
        DateTime Modified { get; set; }
        string Photo { get; set; }
        DateTime Posted { get; set; }
        int Rate { get; set; }
        string Title { get; set; }
        bool Visible { get; set; }
    }
}