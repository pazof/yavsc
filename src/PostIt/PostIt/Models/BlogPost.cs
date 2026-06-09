using System;

namespace PostIt.Models;

public class BlogPost
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Article { get; set; }
    public string? Photo { get; set; }
    public string? AuthorId { get; set; }
    public DateTime DateCreated { get; set; }
    public string? UserCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string? UserModified { get; set; }
}
