using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public class GitRepositoryReference
    {       
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Path { get; set; }

        [StringLength(2048)]
        public string Url { get; set; }

        [StringLength(512)]
        public string Branch { get; set; }

        [StringLength(1024)]
        public string OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        public override string ToString()
        {
            return $"[Git ref {Path} {Branch} {Url}]";
        }
    }
}