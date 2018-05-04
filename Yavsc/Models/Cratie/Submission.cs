using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Cratie
{
    public class Submission
    {
        [ForeignKey("CodeScrutin")]
        public virtual Scrutin Context { get; set; }
        public string CodeScrutin { get; set ; }

        [ForeignKey("CodeOption")]
        public virtual Option Choice { get; set; }
        public string CodeOption { get; set; }

        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; }
        public string AuthorId { get ; set ;}
    }
}