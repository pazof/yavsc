using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models;

namespace Yavsc.Server.Models.Blog
{
    public class BlogTrad
    {
        [Required]
        public long PostId { get; set; }

        [Required]
        public string Lang { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string TraducerId  { get; set; }
        
        [ForeignKey("TraducerId"),JsonIgnore]
        public ApplicationUser Traducer { set; get; }

    }
}