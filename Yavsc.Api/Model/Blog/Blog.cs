using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class Blog
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string bcontent { get; set; }

        [DisplayAttribute(Name="Modified")]
        public DateTime modified { get; set; }
        public string photo { get; set; }
        public DateTime posted { get; set; }
        public int rate { get; set; }
        public string title { get; set; }
        [Required]
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser Author { set; get; }
        public bool visible { get; set; }
    }
}
