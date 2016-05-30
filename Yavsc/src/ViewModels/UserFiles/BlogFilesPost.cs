using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

using Microsoft.AspNet.Http;

public class BlogFilesPost {
    [Required]
    public long PostId {get; set; }
    [Required]
    public IList<IFormFile> File { get; set; }
}