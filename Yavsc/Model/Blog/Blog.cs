using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class Blog
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name="Content")]
        public string bcontent { get; set; }

        [Display(Name="Modified")]
        public DateTime modified { get; set; }
        [Display(Name="Photo")]
        public string photo { get; set; }
        [Display(Name="Posted")]
        public DateTime posted { get; set; }
        [Display(Name="Rate")]
        public int rate { get; set; }
        [Display(Name="Title")]
        public string title { get; set; }
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser Author { set; get; }
        [Display(Name="Visible")]
        public bool visible { get; set; }
    }
}
