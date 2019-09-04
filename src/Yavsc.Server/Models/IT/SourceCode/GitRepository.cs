using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;
using Yavsc.Models;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public class GitRepositoryReference
    {       
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [YaRequired]
        public string Path { get; set; }

        [YaStringLength(2048)]
        public string Url { get; set; }

        [YaStringLength(512)]
        public string Branch { get; set; }

        [YaStringLength(1024)]
        public string OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        public override string ToString()
        {
            return $"[Git ref {Path} {Branch} {Url}]";
        }
    }
}