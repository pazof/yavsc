using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Interfaces;

namespace Yavsc.Models
{
    public partial class Blog : IBlog
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Content { get; set; }
        public DateTime Modified { get; set; }
        public string Photo { get; set; }
        public DateTime Posted { get; set; }
        public int Rate { get; set; }
        public string Title { get; set; }
        public string AuthorId { get; set; }
        
        [ForeignKey("AuthorId"),JsonIgnore]
        public ApplicationUser Author { set; get; }
        public bool Visible { get; set; }
    }
}
